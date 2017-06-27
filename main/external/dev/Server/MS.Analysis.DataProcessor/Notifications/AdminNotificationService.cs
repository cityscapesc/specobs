// Copyright (c) Microsoft Corporation
//
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use file except in compliance 
// with the License.  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER
// EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE,
// FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.

namespace Microsoft.Spectrum.Analysis.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Reflection;
    using System.Text;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.Models;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using UserRoleType = Microsoft.Spectrum.Storage.Enums.UserRoles;

    public class AdminNotificationService
    {
        private const string SmtpHost = "SmptHost";
        private const string SenderEmailAddress = "FromEmailAddress";
        private const string SenderPassword = "SenderPassword";
        private const string ResourceManifest = "Microsoft.Spectrum.Analysis.DataProcessor.EmailBodyTemplates";
        private const string StationsHealthStatusEmailBodyFile = "StationsHealthStatus.html";
        private const string UnprocessedFilesEmailBodyTemplate = "UnprocessedFiles.html";
        private const string UserFeedbackEmailBodyTemplate = "UserFeedbackList.html";
        private const string IssuesReportedEmailBodyTemplate = "StationIssuesList.html";

        private readonly ILogger logger;

        public AdminNotificationService(SpectrumObservatoriesMonitoringService stationMonitoringService)
        {
            if (stationMonitoringService == null)
            {
                throw new ArgumentNullException("stationMonitoringService", "MeasurementStation monitoring service can not be null");
            }

            this.logger = GlobalCache.Instance.Logger;
            stationMonitoringService.MeasurementStationDown += this.OnMeasurementStationDown;
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

        protected void OnMeasurementStationDown(object sender, HealthStatusEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e", "HealthStatusEvent parameter can not be null");
            }

            string fromAlias = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.AdminNotificationService, SenderEmailAddress, string.Empty);

            // Notify Site administrators.
            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = "Measurement Stations Health Status";

                mailMessage.From = new MailAddress(fromAlias);

                if (e.SiteAdmins != null && e.SiteAdmins.Any())
                {
                    foreach (string siteAdmin in e.SiteAdmins)
                    {
                        mailMessage.To.Add(new MailAddress(siteAdmin));
                    }

                    string htmlEmailBody = GetStationHealthStatusEmailBodyForSiteAdmins(e.StationsHealthStatus, e.UserFeedbackCollection);
                    this.SendEmail(htmlEmailBody, mailMessage);
                    this.logger.Log(TraceEventType.Information, LoggingMessageId.AdminNotificationService, "Sent heath status mail to all station admins and site admins.");
                }
                else
                {
                    this.logger.Log(TraceEventType.Error, LoggingMessageId.AdminNotificationService, "There are no Site Administers found in the System.");
                }
            }

            // Notify Station administrators.
            if (e.StationsHealthStatus != null && e.StationsHealthStatus.Any())
            {
                Dictionary<string, List<MeasurementStationHealthStatus>> stationAdminStationHealthStatus = new Dictionary<string, List<MeasurementStationHealthStatus>>();

                // Aggregating MeasurementStation Health Status data for each individual Station Administrators. So, each Station administrator will receive 
                // only one consolidated email which contains details about all the stations for which he has station administrator access.
                foreach (MeasurementStationHealthStatus stationHealthStatus in e.StationsHealthStatus)
                {
                    if (stationHealthStatus.AdminEmailAddress != null && stationHealthStatus.AdminEmailAddress.Any())
                    {
                        foreach (string stationAdmin in stationHealthStatus.AdminEmailAddress)
                        {
                            if (!stationAdminStationHealthStatus.ContainsKey(stationAdmin))
                            {
                                stationAdminStationHealthStatus.Add(stationAdmin, new List<MeasurementStationHealthStatus>());
                            }

                            stationAdminStationHealthStatus[stationAdmin].Add(stationHealthStatus);
                        }
                    }
                    else
                    {
                        this.logger.Log(TraceEventType.Error, LoggingMessageId.AdminNotificationService, string.Format(CultureInfo.InvariantCulture, "There are no Station Administrators found for the Spectrum Observatory Station Id {0} in the System", stationHealthStatus.StationIdentifier.Id.ToString()));
                    }
                }

                foreach (var stationAdminsNotification in stationAdminStationHealthStatus)
                {
                    using (MailMessage mailMessage = new MailMessage())
                    {
                        mailMessage.IsBodyHtml = true;
                        mailMessage.Subject = "Measurement Stations Health Status";

                        mailMessage.From = new MailAddress(fromAlias);
                        mailMessage.To.Add(stationAdminsNotification.Key);

                        string htmlEmailBody = GetStationHealthStatusEmailBodyForStationAdmin(stationAdminsNotification.Value);
                        this.SendEmail(htmlEmailBody, mailMessage);
                    }
                }
            }
        }

        private static string GetStationHealthStatusEmailBodyForStationAdmin(IEnumerable<MeasurementStationHealthStatus> stationHealthStatus)
        {
            return GetStationsHealthStatusEmailBodyTemplate(stationHealthStatus, UserRoleType.StationAdmin, null);
        }

        private static string GetStationHealthStatusEmailBodyForSiteAdmins(IEnumerable<MeasurementStationHealthStatus> stationsHealthStatus, IEnumerable<Feedback> userFeedbackCollection)
        {
            return GetStationsHealthStatusEmailBodyTemplate(stationsHealthStatus, UserRoleType.SiteAdmin, userFeedbackCollection);
        }

        private static string GetStationsHealthStatusEmailBodyTemplate(IEnumerable<MeasurementStationHealthStatus> stationsHealthStatus, UserRoleType userRole, IEnumerable<Feedback> userFeedbackCollection)
        {
            string resourceManifestPath = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", ResourceManifest, StationsHealthStatusEmailBodyFile);
            string emailBody = GetHtmlFromEmbeddedResource(resourceManifestPath);

            string issuesReportedResourceManifestPath = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", ResourceManifest, IssuesReportedEmailBodyTemplate);
            string issuesReportedTemplate = GetHtmlFromEmbeddedResource(issuesReportedResourceManifestPath);

            emailBody = emailBody.Replace("{HealthStatusTitleText}", "City Scape Spectrum Observatory Daily Notifications");
            emailBody = emailBody.Replace("{Duration}", "Last 24 Hours");

            StringBuilder observatoriesHealthStatusTemplate = new StringBuilder();
            StringBuilder reportedIssues = new StringBuilder();

            // Logic to fill in Measurement stations health status section place holders.
            foreach (MeasurementStationHealthStatus observatoryHealthStatus in stationsHealthStatus)
            {
                observatoriesHealthStatusTemplate.Append("<tr>");

                observatoriesHealthStatusTemplate.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", observatoryHealthStatus.StationIdentifier.Name + " " + observatoryHealthStatus.StationIdentifier.Id));
                observatoriesHealthStatusTemplate.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", observatoryHealthStatus.StatusMessage));

                string errorFilesResourceManifestPath = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", ResourceManifest, UnprocessedFilesEmailBodyTemplate);
                string errorFileTemplate = GetHtmlFromEmbeddedResource(errorFilesResourceManifestPath);

                // Logic to fill in details about file processing failures place holders
                if (observatoryHealthStatus.FileProcessingErrors != null && observatoryHealthStatus.FileProcessingErrors.Any())
                {
                    StringBuilder errorFileDetails = new StringBuilder();

                    foreach (ScanFileProcessingError errorFile in observatoryHealthStatus.FileProcessingErrors)
                    {
                        errorFileDetails.Append("<tr>");
                        errorFileDetails.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", errorFile.AbsoluteFilePath));
                        errorFileDetails.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", errorFile.Error));
                        errorFileDetails.Append("</tr>");
                    }

                    errorFileTemplate = errorFileTemplate.Replace("{UnprocessedFiles}", errorFileDetails.ToString());
                }
                else
                {
                    errorFileTemplate = errorFileTemplate.Replace("{UnprocessedFiles}", "None");
                }

                observatoriesHealthStatusTemplate.Append(errorFileTemplate);

                // Logic to fill in Numbers of files due in processing queue place holders only for Site Administrators.
                if (userRole == UserRoleType.SiteAdmin)
                {
                    observatoriesHealthStatusTemplate.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", observatoryHealthStatus.FilesDueInProcessingQueue));
                }

                observatoriesHealthStatusTemplate.Append("</tr>");

                if (observatoryHealthStatus.IssuesReported != null && observatoryHealthStatus.IssuesReported.Any())
                {
                    reportedIssues.Append(AdminNotificationService.GetIssuesReportedtemplate(observatoryHealthStatus.IssuesReported, observatoryHealthStatus.StationIdentifier.Name));
                }
            }

            if (reportedIssues.Length == 0)
            {
                emailBody = emailBody.Replace("{IssuesReportTemplate}", string.Empty);
            }
            else
            {
                issuesReportedTemplate = issuesReportedTemplate.Replace("{IssuesList}", reportedIssues.ToString());
                emailBody = emailBody.Replace("{IssuesReportTemplate}", issuesReportedTemplate);
            }

            emailBody = emailBody.Replace("{ObservatoriesHeathStatusTemplate}", observatoriesHealthStatusTemplate.ToString());

            if (userRole == UserRoleType.SiteAdmin)
            {
                // Logic to enable Number of files due in processing queue column for site administrators.
                emailBody = emailBody.Replace("{DueFilesCountInWorkerQueueTitle}", string.Format(CultureInfo.InvariantCulture, "<th>{0}</th>", " No. Of Files due in Processing Queue"));

                // Logic to fill in User Feedback list place holders.
                if (userFeedbackCollection.Any())
                {
                    StringBuilder userFeedback = new StringBuilder();

                    string userFeedbackResourceManifestPath = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", ResourceManifest, UserFeedbackEmailBodyTemplate);
                    string userFeedbackTemplate = GetHtmlFromEmbeddedResource(userFeedbackResourceManifestPath);

                    foreach (Feedback feedback in userFeedbackCollection)
                    {
                        userFeedback.Append("<tr>");
                        userFeedback.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", feedback.Email));
                        userFeedback.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", feedback.Subject));
                        userFeedback.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", feedback.Comment));
                        userFeedback.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", feedback.Phone));
                        userFeedback.Append("</tr>");
                    }

                    userFeedbackTemplate = userFeedbackTemplate.Replace("{FeedbackList}", userFeedback.ToString());
                    emailBody = emailBody.Replace("{UserFeedbackTemplate}", userFeedbackTemplate);
                }
            }
            else
            {
                // Logic to disable Number of file due in Processing queue and user feedback list for Station Administrators.
                emailBody = emailBody.Replace("{DueFilesCountInWorkerQueueTitle}", string.Empty);
                emailBody = emailBody.Replace("{UserFeedbackTemplate}", string.Empty);
            }

            return emailBody;
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

        private static string GetIssuesReportedtemplate(IEnumerable<IssueReport> issuesReported, string stationName)
        {
            StringBuilder issuesReportedBuilder = new StringBuilder();

            foreach (IssueReport problem in issuesReported)
            {
                issuesReportedBuilder.Append("<tr>");
                issuesReportedBuilder.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", problem.StationId.ToString()));
                issuesReportedBuilder.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", stationName));
                issuesReportedBuilder.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", problem.Email));
                issuesReportedBuilder.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", problem.Subject));
                issuesReportedBuilder.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", problem.IssueDescription));
                issuesReportedBuilder.Append(string.Format(CultureInfo.InvariantCulture, "<td>{0}</td>", problem.Phone));
                issuesReportedBuilder.Append("</tr>");
            }

            return issuesReportedBuilder.ToString();
        }

        private void SendEmail(string emailBody, MailMessage emailMessage)
        {
            try
            {
                using (SmtpClient smtpCient = new SmtpClient())
                {
                    string host = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.AdminNotificationService, SmtpHost, "smtp.office365.com");
                    string senderEmailAddress = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.AdminNotificationService, SenderEmailAddress, string.Empty);
                    string senderPassword = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.AdminNotificationService, SenderPassword, string.Empty);

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
