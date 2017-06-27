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
    using System.Threading;
    using System.Threading.Tasks;

    public class MessageBufferOptions
    {
        private int maxDegreeOfParallelism;
        private int maxMessagesInBuffer;
        private TaskScheduler taskScheduler;

        public MessageBufferOptions()
        {
            this.MaxDegreeOfParallelism = 0;
            this.MaxMessagesInBuffer = 0;
            this.CancellationToken = CancellationToken.None;
            this.TaskScheduler = TaskScheduler.Default;
            this.Logger = null;
        }

        public int MaxDegreeOfParallelism
        {
            get 
            {
                return this.maxDegreeOfParallelism;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "The value must be either 0 (to indicate unbounded parallelism) or a positive number.");
                }

                this.maxDegreeOfParallelism = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of messages allowed in the buffer before posting begins blocking.
        /// </summary>
        public int MaxMessagesInBuffer
        {
            get
            {
                return this.maxMessagesInBuffer;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "The value must be either 0 (to indicate an unlimited number of messages are allowed in the buffer) or a positive number.");
                }

                this.maxMessagesInBuffer = value;
            }
        }

        public CancellationToken CancellationToken { get;  set; }

        public ILogger Logger { get; set; }

        public TaskScheduler TaskScheduler
        {
            get
            {
                return this.taskScheduler; 
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.taskScheduler = value;
            }
        }
    }
}