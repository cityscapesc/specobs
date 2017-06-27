namespace Microsoft.Spectrum.Storage.Queue.Azure
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using Common.Enums;
    using ServiceBus;

    public class AzureServiceBusQueue : IMessageQueue
    {
        private string connectionString;
        private readonly NamespaceManager namespaceManager;

        public AzureServiceBusQueue(string connectionString, string[] topics, string subscriptionName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("AzureServiceBus connection string can't be null or empty", "connectionString");
            }

            if (topics == null)
            {
                throw new ArgumentNullException("topics", "Topics list can't be null");
            }

            if (topics.Length == 0)
            {
                throw new ArgumentException("topics", "Topics can't be empty");
            }

            if (string.IsNullOrWhiteSpace(subscriptionName))
            {
                throw new ArgumentException("subscription", "Subscription name can't be null or empty");
            }

            this.connectionString = connectionString;
            this.namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            this.Init(topics, subscriptionName);
        }

        private void Init(string[] topics, string subscription)
        {
            foreach (var topic in topics)
            {
                try
                {
                    TopicDescription topicDescription = this.namespaceManager.CreateTopic(topic);
                    SubscriptionDescription myAgentSubscription = this.namespaceManager.CreateSubscription(topic, subscription);
                }
                catch (MessagingEntityAlreadyExistsException)
                {
                    // Nothing to do.
                }
            }
        }

        public string GetMessage()
        {
            throw new NotImplementedException();
        }

        public void PutMessage(string message, int messagePriority = (int)MessagePriority.High)
        {
            try
            {
                TopicClient topicClient = TopicClient.CreateFromConnectionString(this.connectionString, ((MessagePriority)messagePriority).ToString());

                BrokeredMessage brokeredMessage = new BrokeredMessage(message);
                brokeredMessage.MessageId = DateTime.UtcNow.Ticks.ToString("d19");

                brokeredMessage.Properties["Priority"] = messagePriority;
                topicClient.Send(new BrokeredMessage(message));
            }
            catch
            {
                throw;
            }
        }
    }
}
