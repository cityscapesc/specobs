// Copyright (c) Microsoft Corporation
//
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License.  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER
// EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE,
// FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.

namespace Microsoft.Spectrum.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an in-memory buffer which allows a delegate to be invoked for each message posted into the buffer.
    /// The invocation of this delegate can be constrained to a maximum degree of parallelism to limit resource usage 
    /// (and be cancelled and use a custom task scheduler, if desired.)
    /// </summary>
    public class MessageBuffer<T> : IDisposable
    {
        private readonly Action<T> action;
        private readonly TaskFactory taskFactory;
        private readonly CancellationToken cancellationToken;
        private readonly BlockingCollection<T> blockingCollection;
        private readonly SemaphoreSlim semaphore;
        private readonly CountdownEvent semaphoreDisposalEvent;
        private readonly Task mainConsumerLoopTask;
        private readonly ILogger logger;

        private bool disposed = false;

        /// <param name="action">The action delegate which gets invoked for every message posted to the buffer.</param>
        public MessageBuffer(Action<T> action) : this(action, new MessageBufferOptions())
        {
        }

        /// <param name="action">The action delegate which gets invoked for every message posted to the buffer.</param>
        /// <param name="options">The options which allow more fine-grained control over the underlying task execution.</param>
        public MessageBuffer(Action<T> action, MessageBufferOptions options)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            this.action = action;
            this.cancellationToken = options.CancellationToken;
            this.logger = options.Logger;

            int maxMessagesInBuffer = options.MaxMessagesInBuffer;
            if (maxMessagesInBuffer == 0)
            {
                this.blockingCollection = new BlockingCollection<T>(new ConcurrentQueue<T>());
            }
            else
            {
                this.blockingCollection = new BlockingCollection<T>(new ConcurrentQueue<T>(), maxMessagesInBuffer);
            }

            Action<T> messageProcessor = this.action;
            
            int maxDegreeOfParallelism = options.MaxDegreeOfParallelism;
            if (maxDegreeOfParallelism != 0)
            {
                this.semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
                this.semaphoreDisposalEvent = new CountdownEvent(1); // Can't set this to 0: http://social.msdn.microsoft.com/Forums/br/parallelextensions/thread/aa49f92c-01a8-4901-9846-91bc1587f3ae
                messageProcessor = this.WrapWithSemaphore(messageProcessor);
            }

            if (typeof(ICancelableMessage).IsAssignableFrom(typeof(T)))
            {
                messageProcessor = this.WrapWithCancellableTaskStarter(messageProcessor);
            }
            else
            {
                messageProcessor = this.WrapWithTaskStarter(messageProcessor);
            }

            Action mainConsumerLoop = this.MakeMainConsumerLoop(messageProcessor);

            this.taskFactory = new TaskFactory(this.cancellationToken, TaskCreationOptions.None, TaskContinuationOptions.None, options.TaskScheduler);

            this.mainConsumerLoopTask = this.taskFactory.StartNew(mainConsumerLoop);            
        }

        ~MessageBuffer()
        {
            this.Dispose(false);
        }

        public void Post(T message)
        {
            this.ThrowIfDisposed();
            this.blockingCollection.Add(message, this.cancellationToken);            
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources here

                    // Finish adding items to the blocking collection, causing the foreach loop to terminate
                    this.blockingCollection.CompleteAdding();

                    // Wait for the main consumer loop to finish
                    try
                    {
                        this.mainConsumerLoopTask.Wait();
                    }
                    catch (AggregateException ae)
                    {
                        // But ignore all tasks that were cancelled
                        ae.Handle((e) =>
                        {
                            if (!(e is TaskCanceledException))
                            {
                                if (logger != null)
                                {
                                    this.logger.Log(TraceEventType.Error, LoggingMessageId.MessageBufferEventId, e.ToString());
                                }
                                else
                                {
                                    return false;
                                }
                            }

                            return true;
                        });
                    }

                    this.blockingCollection.Dispose();

                    // Can't dispose the semaphore directly here because there are tasks that have been spawned off that are still using it.
                    // Start up a new task to wait for all of those tasks to finish and then dispose the semaphore.
                    if (this.semaphore != null && this.semaphoreDisposalEvent != null)
                    {
                        this.taskFactory.StartNew(() =>
                            {
                                semaphoreDisposalEvent.Signal(); // Since it started at 1, not 0.
                                semaphoreDisposalEvent.Wait();
                                semaphore.Dispose();
                                semaphoreDisposalEvent.Dispose();
                            });
                    }
                }

                // Dispose of unmanaged resources here
                this.disposed = true;
            }
        }

        private Action MakeMainConsumerLoop(Action<T> messageProcessor)
        {
            return () =>
            {
                try
                {
                    // NOTE: According to http://msdn.microsoft.com/en-us/library/dd267312(v=vs.110).aspx GetConsumingEnumerable
                    // will block once the collection is empty unless BlockingCollection.CompleteAdding is called. Since this class doesn't
                    // call CompleteAdding until Dispose, it leverages the blocking behavior to turn a foreach loop into a thread.
                    foreach (T message in blockingCollection.GetConsumingEnumerable(cancellationToken))
                    {
                        // It's very important to capture the message value within the loop instead of passing the continuously-updating iterator value to the new task
                        T messageCapture = message;
                        messageProcessor(messageCapture);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Eat the exception
                    // Let this task complete                
                }
                catch (Exception e) // Protect the thread
                {
                    if (logger != null)
                    {
                        logger.Log(TraceEventType.Error, LoggingMessageId.MessageBufferEventId, e.ToString());
                    }
                    else
                    {
                        throw; // Should roll up in the aggregate exception
                    }
                }
            };
        }

        private Action<T> WrapWithCancellableTaskStarter(Action<T> messageProcessor)
        {
            return (message) =>
            {
                ICancelableMessage cancellableMessage = (ICancelableMessage)message;
                CancellationToken globalOrMessageCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellableMessage.CancellationToken).Token;
                taskFactory.StartNew(
                    () => 
                {
                    try
                    {
                        messageProcessor(message);
                    }
                    catch (Exception e) // Protect the thread
                    {
                        if (logger != null)
                        {
                            this.logger.Log(TraceEventType.Error, LoggingMessageId.MessageBufferEventId, e.ToString());
                        }
                        else
                        {
                            throw; // Should roll up in the aggregate exception
                        }
                    }
                },
                globalOrMessageCancellationToken);
            };
        }

        private Action<T> WrapWithTaskStarter(Action<T> messageProcessor)
        {
            return (message) =>
            {
                taskFactory.StartNew(() =>
                {
                    try
                    {
                        messageProcessor(message);
                    }
                    catch (Exception e) // Protect the thread
                    {
                        if (logger != null)
                        {
                            this.logger.Log(TraceEventType.Error, LoggingMessageId.MessageBufferEventId, e.ToString());
                        }
                        else
                        {
                            throw; // Should roll up in the aggregate exception
                        }
                    }
                });
            };
        }

        private Action<T> WrapWithSemaphore(Action<T> messageProcessor)
        {
            return (message) =>
            {
                try
                {
                    semaphoreDisposalEvent.AddCount();

                    try
                    {                        
                        semaphore.Wait(cancellationToken); // TODO: Timeout?
                        messageProcessor(message);
                    }
                    finally
                    {
                        semaphore.Release();                        
                    }  
                }
                finally
                {
                    semaphoreDisposalEvent.Signal();
                }                              
            };
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("MessageBuffer");
            }
        }
    }
}