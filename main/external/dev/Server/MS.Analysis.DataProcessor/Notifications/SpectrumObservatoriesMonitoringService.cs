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
    using System.Linq;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Enums;
    using Microsoft.Spectrum.Storage.Models;
    using Microsoft.Spectrum.Storage.Queue.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;

    internal delegate void HealthStatusChangedEventHandler(object sender, EventArgs args);

    public class SpectrumObservatoriesMonitoringService
    {
        private const string HealthStatusCheck = "HealthStatusCheck";
        private const string StationAvailabilityCheck = "StationAvailabilityCheck";
        private const string FeedbackCount = "FeedbackCount";

        private static SpectrumObservatoriesMonitoringService instance;

        private readonly MeasurementStationTableOperations measurementStationTableOperations;
        private readonly SpectrumDataProcessorMetadataStorage metadataStorage;
        private readonly ILogger logger;

        public SpectrumObservatoriesMonitoringService()
        {
            this.measurementStationTableOperations = GlobalCache.Instance.MeasurementStationOperations;
            this.metadataStorage = GlobalCache.Instance.MasterSpectrumDataProcessorMetadataStorage;
            this.logger = GlobalCache.Instance.Logger;
        }

        public event EventHandler<HealthStatusEventArgs> MeasurementStationDown;

        public static SpectrumObservatoriesMonitoringService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SpectrumObservatoriesMonitoringService();
                    new AdminNotificationService(instance);
                }

                return instance;
            }
        }

        public void ExecuteSpectrumObservatoriesHealthStatusMonitoringSchedule()
        {
            this.logger.Log(TraceEventType.Start, LoggingMessageId.SpectrumObservatoriesMonitoringService, string.Format(CultureInfo.InvariantCulture, "Measurement Stations Health monitoring service started at {0}", DateTime.UtcNow));

            DateTime startTime = DateTime.UtcNow;

            try
            {
                IEnumerable<MeasurementStationInfo> measurementStations = this.measurementStationTableOperations.GetAllMeasurementStationInfoPrivate();
                List<MeasurementStationHealthStatus> stationsHealthStatus = new List<MeasurementStationHealthStatus>();

                DateTimeOffset modifiedSince = startTime.Subtract(SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.SpectrumObservatoriesMonitoringService, HealthStatusCheck, TimeSpan.FromHours(24)));

                foreach (MeasurementStationInfo station in measurementStations)
                {
                    string measurementStationId = station.Identifier.Id.ToString();

                    StationAvailability availability = (StationAvailability)station.StationAvailability;

                    // Do not have to monitor health status for those stations which are decommissioned, down for maintenance or a upcoming stations.
                    if (availability == StationAvailability.Decommissioned || availability == StationAvailability.DownForMaintenance || availability == StationAvailability.Upcoming)
                    {
                        continue;
                    }

                    try
                    {
                        CloudStorageAccount storageAccount = SpectrumDataStorageAccountsTableOperations.Instance.GetCloudStorageAccountByName(station.Identifier.StorageAccountName);

                        MeasurementStationHealthStatus stationHealthStatus = CheckIfMeasurementStationHealthStatusDown(storageAccount, station, modifiedSince);

                        if (stationHealthStatus != null)
                        {
                            stationsHealthStatus.Add(stationHealthStatus);
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = string.Format(CultureInfo.InvariantCulture, "Unable to access Storage account {0} for the Measurement station id {1}, {2}", station.Identifier.StorageAccountName, measurementStationId, ex.ToString());

                        this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, errorMessage);
                    }
                }

                // send an e-mail to the station admins regardless of if there is a station down or not
                IEnumerable<string> stationAdminsList = GlobalCache.Instance.UserManagementTableStorage.GetAllSiteAdmins().Select(user => user.PreferredEmail);

                IEnumerable<Feedback> userFeedback = this.metadataStorage.GetUnReadUserFeedbackCollection(modifiedSince.Date);
                List<Feedbacks> readFeedback = new List<Feedbacks>();

                if (userFeedback.Any())
                {
                    // Limiting feedback count to a known number, so that we will not have to show too much data in notifications mail.
                    int feedbackCount = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.SpectrumObservatoriesMonitoringService, FeedbackCount, 50);
                    userFeedback = userFeedback.Take(feedbackCount);

                    foreach (var item in userFeedback)
                    {
                        Feedbacks feedback = new Feedbacks(item.TimeOfSubmission);

                        feedback.Comment = item.Comment;
                        feedback.Email = item.Email;
                        feedback.FirstName = item.FirstName;
                        feedback.LastName = item.LastName;
                        feedback.Phone = item.Phone;
                        feedback.Subject = item.Subject;
                        feedback.Read = true;

                        readFeedback.Add(feedback);
                    }
                }

                HealthStatusEventArgs healthStatusEvnetArgs = new HealthStatusEventArgs(stationsHealthStatus, stationAdminsList, userFeedback);

                this.OnMeasurementStationDown(healthStatusEvnetArgs);

                this.metadataStorage.MarkFeedbackAsRead(readFeedback);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, ex.ToString());
            }

            this.logger.Log(TraceEventType.Stop, LoggingMessageId.SpectrumObservatoriesMonitoringService, string.Format(CultureInfo.InvariantCulture, "Measurement Stations Health monitoring service ended at {0}", DateTime.UtcNow));
        }

        public void ExecuteSpectrumObservatoriesAvailabilityCheckSchedule()
        {
            this.logger.Log(TraceEventType.Start, LoggingMessageId.SpectrumObservatoriesMonitoringService, string.Format(CultureInfo.InvariantCulture, "Spectrum Observatories availability check schedule started at {0}", DateTime.UtcNow));

            DateTime startTime = DateTime.UtcNow;
            try
            {
                IEnumerable<MeasurementStationInfo> measurementStations = this.measurementStationTableOperations.GetAllMeasurementStationInfoPrivate();

                foreach (MeasurementStationInfo station in measurementStations)
                {
                    try
                    {
                        StationAvailability availability = (StationAvailability)station.StationAvailability;

                        // Do not have to modify availability status for those stations which are decommissioned or upcoming stations.
                        if (availability == StationAvailability.Decommissioned || availability == StationAvailability.Upcoming || availability == StationAvailability.DownForMaintenance)
                        {
                            continue;
                        }

                        CloudStorageAccount storageAccount = SpectrumDataStorageAccountsTableOperations.Instance.GetCloudStorageAccountByName(station.Identifier.StorageAccountName);

                        DateTime modifiedSince = startTime.Subtract(SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.SpectrumObservatoriesMonitoringService, StationAvailabilityCheck, TimeSpan.FromHours(2)));

                        IEnumerable<CloudBlockBlob> blobs = GetAllBlobsModifiedSince(storageAccount, station.Identifier.Id.ToString(), modifiedSince);

                        SpectrumDataProcessorStorage azurePessimisticLockingStorage = DataProcessingHelper.InitializeSpectrumDataProcessorStorage(storageAccount);
                        IEnumerable<ScanFileInformation> scanFileMetadataCollection = azurePessimisticLockingStorage.GetSpectralMetadataInfo(station.Identifier.Id, modifiedSince);

                        // TODO: We will even have to check at more granular level by considering exact number of blobs expected to be there present in the 
                        // system for the set interval.
                        if (blobs != null && blobs.Any() && scanFileMetadataCollection.Any())
                        {
                            availability = StationAvailability.Online;
                        }
                        else
                        {
                            availability = StationAvailability.Offline;
                        }

                        GlobalCache.Instance.MeasurementStationOperations.UpdateStationAvailabilityStatus(station.Identifier.Id, availability);
                    }
                    catch (StorageException storageException)
                    {
                        this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, string.Format(CultureInfo.InvariantCulture, "Station ID: {0}, Exception: {1}", station.Identifier.Id.ToString(), storageException.ToString()));
                    }
                    catch (ArgumentException argumentException)
                    {
                        this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, string.Format(CultureInfo.InvariantCulture, "Station ID: {0}, Exception: {1}", station.Identifier.Id.ToString(), argumentException.ToString()));
                    }
                    catch (Exception ex)
                    {
                        this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, string.Format(CultureInfo.InvariantCulture, "Station ID: {0}, Exception: {1}, This may be expected if the station was registered, but never activated.", station.Identifier.Id.ToString(), ex.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, ex.ToString());
            }

            this.logger.Log(TraceEventType.Stop, LoggingMessageId.SpectrumObservatoriesMonitoringService, string.Format(CultureInfo.InvariantCulture, "Spectrum Observatories availability check ended at {0}", DateTime.UtcNow));
        }

        // TODO: Enable this code when we need this feature to be added to production.
        ////public void ExecuteSpectrumObservatoriesPendingAccessRequestsNotificationSchedule()
        ////{
        ////    this.logger.Log(TraceEventType.Start, LoggingMessageId.SpectrumObservatoriesMonitoringService, string.Format(CultureInfo.InvariantCulture, "Spectrum Observatories pending user access requests notifications schedule started at {0}", DateTime.UtcNow));

        ////    try
        ////    {
        ////        IEnumerable<MeasurementStationInfo> measurementStations = this.measurementStationTableOperations.GetAllMeasurementStationInfoPrivate();

        ////        foreach (MeasurementStationInfo station in measurementStations)
        ////        {
        ////            StationAvailability availability = (StationAvailability)station.StationAvailability;

        ////            // Do not have to look for user pending access request for those stations which are decommissioned or upcoming stations.
        ////            if (availability == StationAvailability.Decommissioned || availability == StationAvailability.Upcoming)
        ////            {
        ////                continue;
        ////            }

        ////            // TODO: Trigger a event which will send a notification to all site admins with a list of pending requests.
        ////        }
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, ex.ToString());
        ////    }

        ////    this.logger.Log(TraceEventType.Stop, LoggingMessageId.SpectrumObservatoriesMonitoringService, string.Format(CultureInfo.InvariantCulture, "Spectrum Observatories pending user access requests notifications schedule ended at {0}", DateTime.UtcNow));
        ////}

        protected virtual void OnMeasurementStationDown(HealthStatusEventArgs e)
        {
            if (this.MeasurementStationDown != null)
            {
                this.MeasurementStationDown(this, e);
            }
        }

        private static IEnumerable<CloudBlockBlob> GetAllBlobsModifiedSince(CloudStorageAccount storageAccount, string measurementStationId, DateTimeOffset modifiedSince)
        {
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(measurementStationId);

            IEnumerable<CloudBlockBlob> blobs = blobContainer.ListBlobs(string.Empty, true, BlobListingDetails.Metadata)
                            .OfType<CloudBlockBlob>()
                            .Where(blob => blob.Properties.LastModified >= modifiedSince);

            return blobs;
        }

        private static MeasurementStationHealthStatus CheckIfMeasurementStationHealthStatusDown(CloudStorageAccount storageAccount, MeasurementStationInfo stationInfo, DateTimeOffset modifiedSince)
        {
            // Obtained all the blob created or modified since last stationHealthStatusCheckTime timespan.
            string measurementStationId = stationInfo.Identifier.Id.ToString();
            IEnumerable<CloudBlockBlob> blobs = GetAllBlobsModifiedSince(storageAccount, measurementStationId, modifiedSince);

            IEnumerable<string> stationAdminsList = GlobalCache.Instance.UserManagementTableStorage.GetAllStationAdmins(stationInfo.Identifier.Id).Select(user => user.PreferredEmail);

            MeasurementStationHealthStatus stationHealthStatus = null;

            // Check whether any issues reported for the station
            IEnumerable<IssueReport> issuesReported = GlobalCache.Instance.PortalTableOperations.GetIssuesReported(stationInfo.Identifier.Id, modifiedSince.Date);

            string statusMessage = string.Empty;

            if (blobs == null || (blobs != null && !blobs.Any()))
            {
                statusMessage = string.Format(CultureInfo.InvariantCulture, "There have been no files uploaded for the Spectrum observatory station:{0} with station key {1} since {2}", stationInfo.Identifier.Name, measurementStationId, modifiedSince.ToString(CultureInfo.InvariantCulture));

                stationHealthStatus = new MeasurementStationHealthStatus(stationInfo.Identifier, stationAdminsList, statusMessage, modifiedSince.DateTime, 0);
                stationHealthStatus.IssuesReported = issuesReported;

                return stationHealthStatus;
            }

            SpectrumDataProcessorStorage spectrumDataProcessorStorage = DataProcessingHelper.InitializeSpectrumDataProcessorStorage(storageAccount);

            IEnumerable<ScanFileProcessingError> scanFileFailures = spectrumDataProcessorStorage.GetAllSpectrumFileProcessingFailures(stationInfo.Identifier.Id, modifiedSince.DateTime);
            IEnumerable<ScanFileInformation> scanFileMetadataCollection = spectrumDataProcessorStorage.GetSpectralMetadataInfo(stationInfo.Identifier.Id, modifiedSince.Date);

            if (scanFileFailures.Any())
            {
                statusMessage = string.Format(CultureInfo.InvariantCulture, "One or more files being processed has failed");
                int filesDueInProcessingQueue = blobs.Count() - scanFileFailures.Count();

                stationHealthStatus = new MeasurementStationHealthStatus(stationInfo.Identifier, stationAdminsList, statusMessage, modifiedSince.DateTime, filesDueInProcessingQueue, scanFileFailures);
                stationHealthStatus.IssuesReported = issuesReported;
            }
            else if (!scanFileMetadataCollection.Any())
            {
                statusMessage = string.Format(CultureInfo.InvariantCulture, string.Format(CultureInfo.InvariantCulture, "No files have been processed since {0} (UTC)", modifiedSince.ToString()));

                stationHealthStatus = new MeasurementStationHealthStatus(stationInfo.Identifier, stationAdminsList, statusMessage, modifiedSince.DateTime, blobs.Count());
                stationHealthStatus.IssuesReported = issuesReported;
            }

            return stationHealthStatus;
        }
    }
}
