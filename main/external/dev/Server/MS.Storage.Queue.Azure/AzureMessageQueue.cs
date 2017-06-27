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

namespace Microsoft.Spectrum.Storage.Queue.Azure
{
    using System;
    using System.Linq;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Common.Enums;

    public class AzureMessageQueue : IMessageQueue
    {
        private const int MaxMessageSize = 65536; // 64 KB

        private CloudStorageAccount storageAccount = null;
        private CloudQueueClient queueClient = null;
        private CloudQueue queue = null;

        public AzureMessageQueue(CloudStorageAccount storageAccount)
        {
            if (storageAccount == null)
            {
                throw new ArgumentNullException("storageAccount");
            }

            this.storageAccount = storageAccount;
            this.queueClient = this.storageAccount.CreateCloudQueueClient();
        }

        public AzureMessageQueue(string queueAddress, CloudStorageAccount storageAccount, bool isQueueResetRequired)
        {
            if (string.IsNullOrWhiteSpace(queueAddress))
            {
                throw new ArgumentNullException("queueAddress");
            }

            if (storageAccount == null)
            {
                throw new ArgumentNullException("storageAccount");
            }

            this.storageAccount = storageAccount;

            if (!AzureQueueMessageHelper.IsValidQueueAddressName(queueAddress))
            {
                throw new ArgumentException("Invalid parameter queueAddress.");
            }
            else
            {
                this.queueClient = this.storageAccount.CreateCloudQueueClient();
                this.queue = this.queueClient.GetQueueReference(queueAddress);

                // Clears the queue if it already exists and if queue reset is required.
                if ((this.queue.CreateIfNotExists() == false) && isQueueResetRequired)
                {
                    this.queue.Clear();
                }
            }
        }

        public int ApproximateMessageCount
        {
            get
            {
                if (this.queue == null)
                {
                    throw new InvalidOperationException("Queue is not initialized");
                }

                return (int)this.queue.ApproximateMessageCount;
            }
        }

        public string GetLastMessageTimestamp
        {
            get
            {
                int queueCount = (int)this.queue.ApproximateMessageCount;
                string timeStamp = this.queue.GetMessages(queueCount).ElementAt(queueCount - 1).InsertionTime.ToString();

                return timeStamp;
            }
        }

        public void SetReferenceToQueue(string queueAddress, bool isQueueResetRequired)
        {
            if (this.queueClient == null)
            {
                throw new InvalidOperationException("queueClient can not be null");
            }

            if (!AzureQueueMessageHelper.IsValidQueueAddressName(queueAddress))
            {
                throw new ArgumentException("Invalid parameter queueAddress.", "queueAddress");
            }
            else
            {
                this.queue = this.queueClient.GetQueueReference(queueAddress);

                // Clears the queue if it already exists and if queue reset is required.
                if ((this.queue.CreateIfNotExists() == false) && isQueueResetRequired)
                {
                    this.queue.Clear();
                }
            }
        }

        public void PutMessage(string message, int messagePriority = (int)MessagePriority.High)
        {
            if (message == null || message.Length == 0)
            {
                throw new ArgumentException("Message is null or empty.", "message");
            }

            if (message.Length > MaxMessageSize)
            {
                throw new ArgumentOutOfRangeException("message", "Message size exceeds 64Kb.");
            }

            CloudQueueMessage queueMessage = new CloudQueueMessage(message);
            this.queue.AddMessage(queueMessage);
        }

        public string GetMessage()
        {
            CloudQueueMessage queueMessage = this.queue.GetMessage();
            if (queueMessage == null)
            {
                return null;
            }

            string outMessage = queueMessage.AsString;
            this.queue.DeleteMessage(queueMessage);
            return outMessage;
        }
    }
}
