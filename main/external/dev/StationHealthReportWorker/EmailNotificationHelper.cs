namespace Microsoft.Spectrum.StationHealthReportWorker
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Mail;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;

    public class EmailNotificationHelper
    {
        private const string SmtpHost = "SmptHost";
        private const string SenderEmailAddress = "FromEmailAddress";
        private const string SenderPassword = "SenderPassword";

        private readonly ILogger logger;

        public EmailNotificationHelper(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger", "Logger instance can't be null");
            }

            this.logger = logger;
        }

        public void SendEmail(string emailBody, MailMessage emailMessage)
        {
            try
            {
                using (SmtpClient smtpCient = new SmtpClient())
                {
                    string host = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.AdminNotificationService, SmtpHost, "smtp.office365.com");
                    string senderEmailAddress = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.AdminNotificationService, SenderEmailAddress, string.Empty);
                    string senderPassword = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.AdminNotificationService, SenderPassword, string.Empty);
                    emailMessage.From = new MailAddress(senderEmailAddress);

                    if (!string.IsNullOrWhiteSpace(host))
                    {
                        smtpCient.Host = host;
                    }
                    else
                    {
                        this.logger.Log(TraceEventType.Error, LoggingMessageId.AdminNotificationService, "Host name for SMTP service not set.");
                    }

                    emailMessage.Body = emailBody;
                    smtpCient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpCient.EnableSsl = true;

                    if (!string.IsNullOrWhiteSpace(senderEmailAddress) && !string.IsNullOrWhiteSpace(senderPassword))
                    {
                        smtpCient.UseDefaultCredentials = false;
                        smtpCient.Credentials = new NetworkCredential(senderEmailAddress, senderPassword);
                    }
                    else
                    {
                        this.logger.Log(TraceEventType.Error, LoggingMessageId.AdminNotificationService, "Credentials for SMTP service not set.");
                    }

                    smtpCient.Send(emailMessage);
                }
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.AdminNotificationService, ex.ToString());
            }
        }
    }
}
