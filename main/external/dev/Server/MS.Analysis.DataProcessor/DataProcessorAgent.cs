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
    using System.Text;
    using System.Threading;
    using System.Xml.Serialization;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Microsoft.Spectrum.Analysis.DataProcessor.Rules;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.Spectrum.IO.RawIqFile;
    using Microsoft.Spectrum.IO.ScanFile;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Enums;
    using Microsoft.Spectrum.Storage.Models;
    using Microsoft.Spectrum.Storage.Queue.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Microsoft.WindowsAzure.Scheduler.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Newtonsoft.Json;

    public class DataProcessorAgent
    {
        private const int TimeoutInMilliSeconds = 1000;

        private static readonly object Mutex = new object();
        private readonly Dictionary<string, ScanFileProcessor> storageAccountScanFileProcessors;

        public DataProcessorAgent()
        {
            // Initialize the settings table storage
            SettingsTableHelper.Instance.Initialize(GlobalCache.Instance.SettingsTable, GlobalCache.Instance.Logger);

            this.storageAccountScanFileProcessors = new Dictionary<string, ScanFileProcessor>();
        }

        public void Run(CancellationToken workerThreadCancellationToken, bool processAsynchronously)
        {
            WorkerQueueMessage message = null;
            string queueMessageString = string.Empty;
            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.TypeNameHandling = TypeNameHandling.All;

            while (!workerThreadCancellationToken.IsCancellationRequested)
            {
                try
                {
                    queueMessageString = GlobalCache.Instance.MainWorkerQueue.GetMessage();

                    // Process, if any message is available in azure queue storage.
                    if (!string.IsNullOrWhiteSpace(queueMessageString))
                    {
                        GlobalCache.Instance.Logger.Log(TraceEventType.Information, LoggingMessageId.DataProcessorAgentId, string.Format(CultureInfo.InvariantCulture, "Processing Message: {0}", queueMessageString));

                        try
                        {
                            message = (WorkerQueueMessage)JsonConvert.DeserializeObject(queueMessageString, typeof(WorkerQueueMessage), jss);
                        }
                        catch (JsonReaderException)
                        {
                            // if we can't parse it straight away, check to see if this is a message coming from the scheduler
                            using (StringReader stringReader = new StringReader(queueMessageString))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(StorageQueueMessage));
                                StorageQueueMessage schedulerQueueMessage = (StorageQueueMessage)serializer.Deserialize(stringReader);
                                message = (WorkerQueueMessage)JsonConvert.DeserializeObject(schedulerQueueMessage.Message, typeof(WorkerQueueMessage), jss);
                            }
                        }

                        if (message != null)
                        {
                            if (message.MessageType == (int)MessageType.RetentionPolicy)
                            {
                                // Go through each spectrum data storage account and container and perform the retention policy
                                RetentionPolicy.Instance.ExecuteContainerStorageRetentionPolicy();
                            }
                            else if (message.MessageType == (int)MessageType.SpectrumObservatoriesAvailability)
                            {
                                SpectrumObservatoriesMonitoringService.Instance.ExecuteSpectrumObservatoriesAvailabilityCheckSchedule();
                            }
                            else if (message.MessageType == (int)MessageType.SpectrumObservatoriesHealthMonitoring)
                            {
                                SpectrumObservatoriesMonitoringService.Instance.ExecuteSpectrumObservatoriesHealthStatusMonitoringSchedule();
                            }
                            else if (message.MessageType == (int)MessageType.FileProcessing)
                            {
                                try
                                {
                                    string blobName = message.BlobUri.Split('/').LastOrDefault();
                                    string measurmentStationKey = message.MeasurementStationKey;

                                    CloudStorageAccount stationStorageAccount = GetStationStorageAccount(measurmentStationKey);
                                    RetrySpectrumBlobStorage blobStorage = DataProcessingHelper.GetStationBlobStorage(stationStorageAccount, measurmentStationKey);

                                    string storageAccountName = stationStorageAccount.Credentials.AccountName;

                                    if (!message.UploadSuccess)
                                    {
                                        // Delete the file and log the error
                                        blobStorage.DeleteIfExists(blobName);
                                    }
                                    else
                                    {
                                        SpectrumDataProcessorStorage spectrumDataProcessorStorage = null;

                                        try
                                        {
                                            processAsynchronously = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.DataProcessorCategory, DataProcessingHelper.AsyncProcessingKey, processAsynchronously);

                                            if (message.BlobUri.EndsWith(RawIqFile.Extension, StringComparison.OrdinalIgnoreCase))
                                            {
                                                // Process the Raw IQ data file
                                                spectrumDataProcessorStorage = DataProcessingHelper.InitializeSpectrumDataProcessorStorage(stationStorageAccount);
                                                ProcessRawIqFile(message, blobStorage, spectrumDataProcessorStorage);
                                            }
                                            else if (message.BlobUri.EndsWith(ScanFile.Extension, StringComparison.OrdinalIgnoreCase))
                                            {
                                                // Process the scan file
                                                spectrumDataProcessorStorage = DataProcessingHelper.InitializeSpectrumDataProcessorStorage(stationStorageAccount);

                                                this.AddScanFileProcessorIfNotExists(storageAccountName, spectrumDataProcessorStorage, DataProcessingHelper.InitializePessimisticSpectrumDataProcessorStorage(stationStorageAccount));

                                                ScanFileProcessor scanFileProcessor = this.storageAccountScanFileProcessors[storageAccountName];
                                                ProcessScanFile(measurmentStationKey, blobStorage, scanFileProcessor, message.BlobUri, processAsynchronously);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            string errorMessage = string.Format(CultureInfo.InvariantCulture, "Unable to process file {0} for the Measurement Station Id :{1} and Storage Account {2} | Error Details :{3}", message.BlobUri, measurmentStationKey, storageAccountName, ex.ToString());
                                            GlobalCache.Instance.Logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, errorMessage);

                                            if (spectrumDataProcessorStorage != null)
                                            {
                                                SaveFailedFileDetails(spectrumDataProcessorStorage, measurmentStationKey, message.BlobUri, ex.ToString(), DateTime.UtcNow);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Just in case we get a bad message, we can't trust what is coming in and we don't want to crash
                                    GlobalCache.Instance.Logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, string.Format(CultureInfo.InvariantCulture, "Bad File Processing Message: {0}, Exception: {1}", queueMessageString, ex.ToString()));
                                }
                            }
                            else if (message.MessageType == (int)MessageType.TablesSharedAccessSignatureUpdate)
                            {
                                // Update all out table with all of the shared access signatures that we have created
                                // if a table name is not in this table, then we will create a shared access signature
                                // for this table with a read-only policy
                                UpdateTableSharedAccess.Instance.ExecuteUpdateTableSharedAccess();
                            }

                            // TODO: Enable this code when we need this feature to be added to production.
                            ////else if (message.MessageType == (int)MessageType.UserPendingAccessRequestsNotification)
                            ////{
                            ////    SpectrumObservatoriesMonitoringService.Instance.ExecuteSpectrumObservatoriesPendingAccessRequestsNotificationSchedule();
                            ////}
                        }
                        else
                        {
                            GlobalCache.Instance.Logger.Log(TraceEventType.Information, LoggingMessageId.DataProcessorAgentId, new InvalidOperationException("Queue message is not of request payload type.").ToString());
                        }
                    }

                    Thread.Sleep(TimeoutInMilliSeconds);
                }
                catch (Exception ex)
                {
                    // just in case, we catch anything we might have missed so that we keep the role up and running
                    GlobalCache.Instance.Logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, "Queue Message: " + queueMessageString + "Exception Info: " + ex.ToString());
                }
            }
        }

        private static IEnumerable<AggregationRule> GetAggregationRules(SpectrumDataProcessorStorage spectrumDataProcessorStorage)
        {
            List<AggregationRule> aggregationRules = new List<AggregationRule>();

            aggregationRules.Add(new RawToHourlyRule(spectrumDataProcessorStorage));
            aggregationRules.Add(new RawToDailyRule(spectrumDataProcessorStorage));
            aggregationRules.Add(new RawToWeeklyRule(spectrumDataProcessorStorage));
            aggregationRules.Add(new RawToMonthlyRule(spectrumDataProcessorStorage));

            return aggregationRules;
        }

        private static void ProcessScanFile(string measurementStationKey, RetrySpectrumBlobStorage blobStorage, ScanFileProcessor scanFileProcessor, string blobUri, bool processAsynchronously)
        {
            ScanFile scanFile = null;
            string blobName = blobUri.Split('/').LastOrDefault();

            using (Stream scanFileStream = blobStorage.OpenRead(blobName))
            {
                ScanFileReader sfr = new ScanFileReader(scanFileStream);
                scanFile = sfr.Read();
            }

            if (scanFile == null)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, "Invalid Scan-file: Unable able to read the Scan-file data from the blob storage:{0}", blobUri);
                throw new InvalidDataException(errorMessage);
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (processAsynchronously)
            {
                // Asynchronous & Concurrent Scan file processing.
                scanFileProcessor.ProcessAsync(scanFile, measurementStationKey).Wait();
            }
            else
            {
                // Synchronous Scan file processing.
                scanFileProcessor.Process(scanFile, measurementStationKey);
            }

            scanFileProcessor.UpdateScanFileInformation(Guid.Parse(measurementStationKey), scanFile.Config, blobUri, FileType.ScanFile);

            stopwatch.Stop();

            string information = string.Format(CultureInfo.InvariantCulture, "Overall time to process file {0} with {1} processing is {2} seconds", blobUri, processAsynchronously ? "Asynchronous" : "Synchronous", stopwatch.Elapsed.TotalSeconds);
            GlobalCache.Instance.Logger.Log(TraceEventType.Information, LoggingMessageId.AggregatingDataProcessorEventId, information);
        }

        private static void ProcessRawIqFile(WorkerQueueMessage queueMessage, RetrySpectrumBlobStorage blobStorage, SpectrumDataProcessorStorage spectrumDataProcessorStorage)
        {
            Guid measurementStationId = Guid.Parse(queueMessage.MeasurementStationKey);

            MeasurementStationInfo measurementStation = GlobalCache.Instance.MeasurementStationManager.GetMeasurementStationPublic(measurementStationId);

            if (measurementStation == null)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, "Invalid Measurement StationKey:{0}", queueMessage.MeasurementStationKey);
                throw new ArgumentException(errorMessage, "measurementStationKey");
            }

            string blobName = queueMessage.BlobUri.Split('/').LastOrDefault();

            RawIqFile rawIqFile = null;

            // TODO: Should avoid reading RawIqFile, as we need only metadata information about the file. At present, only way to get timeStart of the RawIqFile
            // is from ConfigDataBlock. We should find a way to bypass this read operation.
            using (Stream rawIqStream = blobStorage.OpenRead(blobName))
            {
                RawIqFileReader rawIqFileReader = new RawIqFileReader(rawIqStream);
                rawIqFile = rawIqFileReader.Read();
            }

            if (rawIqFile == null)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, "Invalid RawIqFile: Unable able to read the RawIqFile data from the blob storage:{0}", queueMessage.BlobUri);

                throw new InvalidDataException(errorMessage);
            }

            if (!measurementStation.RawIQDataAvailabilityStartDate.HasValue)
            {
                IEnumerable<RawSpectralDataInfo> spectralDataSchema = spectrumDataProcessorStorage.GetRawSpectralDataSchema(measurementStationId, FileType.RawIqFile);
                measurementStation.RawIQDataAvailabilityStartDate = rawIqFile.Config.Timestamp;

                if (spectralDataSchema != null
                    && spectralDataSchema.Any())
                {
                    measurementStation.RawIQDataAvailabilityStartDate = spectralDataSchema.FirstOrDefault().TimeStart;
                }
            }

            GlobalCache.Instance.MeasurementStationManager.UpdateRawIQDataAvailabilityTimestamp(measurementStationId, measurementStation.RawIQDataAvailabilityStartDate.Value, rawIqFile.Config.Timestamp);

            DateTime timeStart = rawIqFile.Config.Timestamp;

            ScanFileInformation rawIqFileInformation = new ScanFileInformation(measurementStationId, timeStart, (int)FileCompressionType.Deflate, FileType.RawIqFile, queueMessage.BlobUri, rawIqFile.Config.EndToEndConfiguration.RawIqConfiguration.StartFrequencyHz, rawIqFile.Config.EndToEndConfiguration.RawIqConfiguration.StopFrequencyHz);

            spectrumDataProcessorStorage.InsertOrUpdateScanFileInformation(rawIqFileInformation);
        }

        private static CloudStorageAccount GetStationStorageAccount(string measurementStationKey)
        {
            Guid measurementStationId = Guid.Parse(measurementStationKey);
            string accountName = GlobalCache.Instance.MeasurementStationManager.GetMeasurementStationPublic(measurementStationId).Identifier.StorageAccountName;

            return SpectrumDataStorageAccountsTableOperations.Instance.GetCloudStorageAccountByName(accountName);
        }

        private static void SaveFailedFileDetails(SpectrumDataProcessorStorage spectrumDataProcessorStorage, string measurementStationId, string blobUri, string errorDetails, DateTime timeOfFailure)
        {
            try
            {
                ScanFileProcessingError failure = new ScanFileProcessingError(Guid.Parse(measurementStationId), blobUri, errorDetails, timeOfFailure);

                spectrumDataProcessorStorage.PersistScanFileProcessingError(failure);
            }
            catch (Exception ex)
            {
                GlobalCache.Instance.Logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
            }
        }

        private void AddScanFileProcessorIfNotExists(string cloudStorageAccountName, SpectrumDataProcessorStorage spectrumDataProcessorStorage, PessimisticLockingSpectrumDataProcessorStorage pessimesticLockingSpectrumDataProcessorStorage)
        {
            lock (Mutex)
            {
                if (!this.storageAccountScanFileProcessors.ContainsKey(cloudStorageAccountName))
                {
                    IEnumerable<AggregationRule> aggregationRules = GetAggregationRules(spectrumDataProcessorStorage);
                    ScanFileProcessor scanFileProcessor = new ScanFileProcessor(spectrumDataProcessorStorage, aggregationRules, GlobalCache.Instance.Logger);

                    this.storageAccountScanFileProcessors.Add(cloudStorageAccountName, scanFileProcessor);
                }
            }
        }
    }
}
