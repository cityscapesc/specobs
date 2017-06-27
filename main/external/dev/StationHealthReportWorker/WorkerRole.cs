namespace Microsoft.Spectrum.StationHealthReportWorker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.Spectrum.Common.Enums;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.Queue.Azure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Newtonsoft.Json;
    using System.Globalization;
    using System.Text;
    using Storage.Models;
    using System.IO;
    using System.Reflection;
    using Storage.Table.Azure.Helpers;
    using Storage.Enums;

    public class WorkerRole : RoleEntryPoint
    {
        private const string ResourceManifest = "Microsoft.Spectrum.StationHealthReportWorker.EmailTemplates";
        private const string StationsHealthStatusEmailBodyFile = "StationsHealthStatus.html";

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        private SubscriptionClient highPriSubscriptionClient;
        private SubscriptionClient mediumPriSubscriptionClient;
        private SubscriptionClient lowPriSubscriptonClient;
        private EmailNotificationHelper notificationHelper;

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        ManualResetEvent CompletedEvent = new ManualResetEvent(false);

        private int highPriThreadCount = ConnectionStringsUtility.HighPriorityThreadCount;
        private int mediumPriThreadCount = ConnectionStringsUtility.MediumPriorityThreadCount;
        private int lowPriThreadCount = ConnectionStringsUtility.LowPriorityThreadCount;

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");

            Task[] messageProcessors = new Task[highPriThreadCount + mediumPriThreadCount + lowPriThreadCount];

            for (int i = 0; i < highPriThreadCount; i++)
            {
                messageProcessors[i] = Task.Factory.StartNew(this.ProcessHighPriorityMessages);
            }

            for (int i = highPriThreadCount; i < (highPriThreadCount + mediumPriThreadCount); i++)
            {
                messageProcessors[i] = Task.Factory.StartNew(this.ProcessMediumPriorityMessages);
            }

            for (int i = (highPriThreadCount + mediumPriThreadCount); i < messageProcessors.Length; i++)
            {
                messageProcessors[i] = Task.Factory.StartNew(this.ProcessLowPriorityMessages);
            }

            try
            {
                Task.WaitAll(messageProcessors);
            }
            catch (Exception ex)
            {
                GlobalCache.Instance.Logger.Log(TraceEventType.Error, Common.LoggingMessageId.SpectrumObservatoriesMonitoringService, ex.ToString());
            }

            CompletedEvent.WaitOne();
        }

        private void ProcessMediumPriorityMessages()
        {
            while (!this.cts.IsCancellationRequested)
            {
                HealthReportQueueMessage healthReport = this.ReteriveMessage(this.mediumPriSubscriptionClient);
                this.Notify(healthReport);
            }
        }

        private void ProcessHighPriorityMessages()
        {
            while (!this.cts.IsCancellationRequested)
            {
                HealthReportQueueMessage healthReport = this.ReteriveMessage(this.highPriSubscriptionClient);
                this.Notify(healthReport);
            }
        }

        private void ProcessLowPriorityMessages()
        {
            while (!this.cts.IsCancellationRequested)
            {
                HealthReportQueueMessage healthReport = this.ReteriveMessage(this.lowPriSubscriptonClient);
                this.Notify(healthReport);
            }
        }

        private void Notify(HealthReportQueueMessage healthReport)
        {
            if (healthReport != null)
            {
                try
                {
                    Guid stationKey = Guid.Parse(healthReport.MeasurementStationKey);
                    MeasurementStationInfo stationInfo = GlobalCache.Instance.MeasurementStationManager.GetMeasurementStationPublic(stationKey);

                    if (stationInfo != null)
                    {
                        string emailBody = this.BindValuesToEmailTemplate(healthReport, stationInfo);

                        if ((stationInfo.StationAvailability != (int)StationAvailability.Decommissioned
                            || stationInfo.StationAvailability != (int)StationAvailability.DownForMaintenance)
                            && stationInfo.ReceiveStationNotifications)
                        {
                            this.Notify(stationKey, emailBody, healthReport.Title);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string error = string.Format("Error occured while processing the message :{0}, Exceptions :{1}", healthReport.ToString(), ex.ToString());
                    GlobalCache.Instance.Logger.Log(TraceEventType.Error, Common.LoggingMessageId.SpectrumObservatoriesMonitoringService, error);
                }
            }
        }

        private string BindValuesToEmailTemplate(HealthReportQueueMessage message, MeasurementStationInfo stationInfo)
        {
            string resourceManifestPath = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", ResourceManifest, StationsHealthStatusEmailBodyFile);
            string emailBody = GetHtmlFromEmbeddedResource(resourceManifestPath);

            emailBody = emailBody.Replace("{HealthStatusTitleText}", "City Scape Spectrum Observatory Notifications");
            emailBody = emailBody.Replace("{OccuredAt}", message.OccurredAt.ToString());

            StringBuilder observatoriesHealthStatusTemplate = new StringBuilder();

            observatoriesHealthStatusTemplate.Append("<tr>");
            observatoriesHealthStatusTemplate.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", stationInfo.Identifier.Name));
            observatoriesHealthStatusTemplate.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0},{1}</td>", stationInfo.StationDescription.Description, stationInfo.Address.AddressLine1, stationInfo.Address.AddressLine2));
            observatoriesHealthStatusTemplate.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", message.Title));
            observatoriesHealthStatusTemplate.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", message.Description));
            observatoriesHealthStatusTemplate.Append("</tr>");

            emailBody = emailBody.Replace("{ObservatoriesHeathStatusTemplate}", observatoriesHealthStatusTemplate.ToString());

            return emailBody;
        }

        private void Notify(Guid measurementStationKey, string emailBody, string subject)
        {
            using (MailMessage message = new MailMessage())
            {
                message.IsBodyHtml = true;
                message.Subject = subject;

                IEnumerable<User> stationAdmins = GlobalCache.Instance.UserManagementTableStorage.GetAllStationAdmins(measurementStationKey);
                IEnumerable<User> siteAdmins = GlobalCache.Instance.UserManagementTableStorage.GetAllSiteAdmins();

                if (stationAdmins != null
                    && stationAdmins.Any())
                {
                    foreach (var stationAdmin in stationAdmins)
                    {
                        if (stationAdmin.SubscribeNotifications)
                        {
                            message.To.Add(stationAdmin.PreferredEmail);
                        }
                    }
                }

                if (siteAdmins != null
                    && siteAdmins.Any())
                {
                    foreach (var siteAdmin in siteAdmins)
                    {
                        if (siteAdmin.SubscribeNotifications)
                        {
                            message.To.Add(siteAdmin.PreferredEmail);
                        }
                    }
                }

                if (message.To.Any())
                {
                    this.notificationHelper.SendEmail(emailBody, message);
                }
            }
        }

        private HealthReportQueueMessage ReteriveMessage(SubscriptionClient messageSubsriptionClient)
        {
            BrokeredMessage brokeredMessage = null;

            try
            {
                brokeredMessage = messageSubsriptionClient.Receive();

                if (brokeredMessage == null)
                {
                    return null;
                }

                HealthReportQueueMessage message = JsonConvert.DeserializeObject<HealthReportQueueMessage>(brokeredMessage.GetBody<string>());

                return message;
            }
            catch (Exception ex)
            {
                GlobalCache.Instance.Logger.Log(TraceEventType.Error, Common.LoggingMessageId.SpectrumObservatoriesMonitoringService, string.Format("Unable to read a message from the topic {0}, Exceptions {1}", brokeredMessage.PartitionKey, ex.ToString()));
                return null;
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the queue if it does not exist already

            string connectionString = GlobalCache.Instance.HealthReportServiceBusConnectionString;
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            // Initialize the connection to Service Bus Queue
            MessagingFactory messagingFactory = MessagingFactory.CreateFromConnectionString(connectionString);

            this.highPriSubscriptionClient = messagingFactory.CreateSubscriptionClient(MessagePriority.High.ToString(), ConnectionStringsUtility.HealthReportMessagingSubscription, ReceiveMode.ReceiveAndDelete);
            this.mediumPriSubscriptionClient = messagingFactory.CreateSubscriptionClient(MessagePriority.Medium.ToString(), ConnectionStringsUtility.HealthReportMessagingSubscription, ReceiveMode.ReceiveAndDelete);
            this.lowPriSubscriptonClient = messagingFactory.CreateSubscriptionClient(MessagePriority.Low.ToString(), ConnectionStringsUtility.HealthReportMessagingSubscription, ReceiveMode.ReceiveAndDelete);
            SettingsTableHelper.Instance.Initialize(GlobalCache.Instance.SettingsTable, GlobalCache.Instance.Logger);

            this.notificationHelper = new EmailNotificationHelper(GlobalCache.Instance.Logger);

            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            if (this.cts != null)
            {
                this.cts.Cancel();
            }

            this.highPriSubscriptionClient.Close();
            this.lowPriSubscriptonClient.Close();
            this.mediumPriSubscriptionClient.Close();

            CompletedEvent.Set();
            base.OnStop();
        }

        private static string GetHtmlFromEmbeddedResource(string resourceManifestPath)
        {
            string emailBody = string.Empty;

            using (Stream resourceStream = GetResourceStream(resourceManifestPath))
            {
                StreamReader reader = new StreamReader(resourceStream);
                emailBody = reader.ReadToEnd();
            }

            return emailBody;
        }

        public static Stream GetResourceStream(string resourceManifestFile)
        {
            Stream embededResourceStream = null;

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                embededResourceStream = assembly.GetManifestResourceStream(resourceManifestFile);
            }
            catch
            {
                embededResourceStream = null;
            }

            return embededResourceStream;
        }
    }
}
