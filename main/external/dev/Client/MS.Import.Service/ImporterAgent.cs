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

namespace Microsoft.Spectrum.Import.Service
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Transactions;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;
    using Microsoft.Spectrum.MeasurementStation.Client;
    using Microsoft.Spectrum.Storage.Blob;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using WindowsAzure.Storage.Auth;

    internal class ImporterAgent : IDisposable
    {
        private readonly ILogger logger;
        private readonly IConfigurationSource configurationSource;

        private HealthReporter healthReport;
        private DirectoryWatcherConfiguration configuration;
        private MeasurementStationConfigurationEndToEnd measurementStationConfiguration = null;
        private SettingsConfigurationSection settingsConfiguration;
        private bool continueMonitoring = true;
        private Task importerThread;
        private Task healthReportThread;

        public ImporterAgent(ILogger logger, IConfigurationSource configurationSource)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (configurationSource == null)
            {
                throw new ArgumentNullException("configurationSource");
            }

            this.logger = logger;
            this.configurationSource = configurationSource;

            this.settingsConfiguration = (SettingsConfigurationSection)ConfigurationManager.GetSection("SettingsConfiguration");
            if (File.Exists(this.settingsConfiguration.MeasurementStationConfigurationFileFullPath))
            {
                try
                {
                    using (Stream input = File.OpenRead(this.settingsConfiguration.MeasurementStationConfigurationFileFullPath))
                    {
                        this.measurementStationConfiguration = MeasurementStationConfigurationEndToEnd.Read(input);
                    }
                }
                catch
                {
                    // There is an issue with the configuration file, so delete it and we can rewrite another one
                    File.Delete(this.settingsConfiguration.MeasurementStationConfigurationFileFullPath);
                    this.measurementStationConfiguration = null;
                }
            }
        }

        #region IImporterAgent Implemenation

        public void StartMonitoring()
        {
            // Read the configuration and make sure that it is correct. We should fail fast if there are problems.
            this.configuration = this.configurationSource.GetConfiguration();
            IList<ValidationResult> validationResults = ValidationHelper.Validate(this.configuration);
            if (validationResults.Any())
            {
                throw new ImporterConfigurationException("There was a problem with the specified configuration file.", validationResults);
            }

            this.logger.Log(TraceEventType.Information, LoggingMessageId.ImporterAgent, "Configuration validated successfully.");

            this.logger.Log(TraceEventType.Information, LoggingMessageId.ImporterAgent, "Importer Starting");

            this.healthReport = new HealthReporter(configuration, settingsConfiguration, logger);

            Action action = null;
            action = this.UploadWatchFiles;

            Action healthReportAction = null;
            healthReportAction = healthReport.AutoHealthReporterThread;

            this.importerThread = Task.Factory.StartNew(action);
            this.healthReportThread = Task.Factory.StartNew(healthReportAction);
        }

        public void StopMonitoring()
        {
            try
            {
                this.continueMonitoring = false;
                this.importerThread.Wait();
                this.healthReport.ShutDown();
                this.healthReportThread.Wait();
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.ImporterAgent, ex.ToString());
            }

            this.logger.Log(TraceEventType.Information, LoggingMessageId.ImporterAgent, "Importer Stopped");
        }

        #endregion

        private void UploadWatchFiles()
        {
            while (this.continueMonitoring)
            {
                try
                {
                    using (MeasurementStationServiceChannelFactory channelFactory = new MeasurementStationServiceChannelFactory())
                    {
                        IMeasurementStationServiceChannel channel = channelFactory.CreateChannel(this.configuration.MeasurementStationServiceUri);

                        int stationAvailability = channel.GetStationAvailability(this.configuration.StationAccessId);

                        // NOTE: Following is to prevent data uploading for the decommissioned station.
                        if (Microsoft.Spectrum.Storage.Enums.StationAvailability.Decommissioned == (Microsoft.Spectrum.Storage.Enums.StationAvailability)stationAvailability)
                        {
                            string message = string.Format("Station {0} has been decomissioned. With the decomissioned status no data files will be pushed to Cloud.", this.configuration.StationAccessId);
                            this.logger.Log(TraceEventType.Information, LoggingMessageId.ImporterAgent, message);

                            continue;
                        }

                        // Get the list of pathnames for all existing files that need to be processed
                        string[] existingWatchFiles = Directory.GetFiles(this.configuration.WatchDirectory, this.configuration.WatchDirectoryFileExtension, SearchOption.TopDirectoryOnly);
                        bool gotUpdatedSettings;

                        foreach (string watchFile in existingWatchFiles)
                        {
                            bool fileWritten = false;
                            try
                            {
                                // This call will throw an IOException if the file is current being written to
                                using (var file = File.Open(watchFile, FileMode.Open, FileAccess.Read, FileShare.None))
                                {
                                    fileWritten = true;
                                }
                            }
                            catch (IOException)
                            {
                                // We couldn't write to this file, so it must still be open by someone else
                            }

                            if (!fileWritten)
                            {
                                // skip this file until file it is completely available
                                continue;
                            }

                            Stream scanFileStream = null;
                            bool uploadSuccess = true;
                            bool notifySuccess = false;
                            string blobUri = string.Empty;
                            string error = string.Empty;

                            try
                            {
                                string filename = Path.GetFileName(watchFile);
                                scanFileStream = File.OpenRead(watchFile);

                                // Check to see if there is any changes to the settings
                                string storageAccountName = string.Empty;
                                string storageAccessKey = string.Empty;
                                byte[] measurementStationConfigurationUpdate = null;
                                gotUpdatedSettings = false;

                                while (!gotUpdatedSettings)
                                {
                                    try
                                    {
                                        channel.GetUpdatedSettings(this.configuration.StationAccessId, out storageAccountName, out storageAccessKey, out measurementStationConfigurationUpdate);
                                        gotUpdatedSettings = true;
                                    }
                                    catch (WebException ex)
                                    {
                                        this.logger.Log(TraceEventType.Error, LoggingMessageId.ImporterAgent, ex.ToString());
                                    }
                                }

                                MeasurementStationConfigurationEndToEnd settingsUpdated;
                                using (MemoryStream stream = new MemoryStream(measurementStationConfigurationUpdate))
                                {
                                    settingsUpdated = MeasurementStationConfigurationEndToEnd.Read(stream);
                                }

                                // if the configuration has been updated, then update the setting file
                                if (this.measurementStationConfiguration == null ||
                                    this.measurementStationConfiguration.LastModifiedTime < settingsUpdated.LastModifiedTime ||
                                    !File.Exists(this.settingsConfiguration.MeasurementStationConfigurationFileFullPath))
                                {
                                    this.measurementStationConfiguration = settingsUpdated;

                                    // Write out to a file so that the scanner can get the updated settings as well
                                    using (Stream output = File.OpenWrite(this.settingsConfiguration.MeasurementStationConfigurationFileFullPath))
                                    {
                                        this.measurementStationConfiguration.Write(output);

                                        if (this.healthReport != null)
                                        {
                                            this.healthReport.UsrpScannerConfigurationChanged(this.measurementStationConfiguration);
                                        }
                                    }
                                }

                                AzureSpectrumBlobStorage cloudStorage = new AzureSpectrumBlobStorage(null, null, storageAccountName + storageAccessKey);
                                blobUri = cloudStorage.UploadFile(scanFileStream, filename, this.configuration.UploadRetryCount, this.configuration.ServerUploadTimeout, this.configuration.RetryDeltaBackoff);
                            }
                            catch (Exception e)
                            {
                                uploadSuccess = false;
                                this.logger.Log(TraceEventType.Error, LoggingMessageId.ImporterAgent, e.ToString());
                            }
                            finally
                            {
                                if (scanFileStream != null)
                                {
                                    scanFileStream.Dispose();
                                }
                            }

                            Exception mostRecentEx = null;
                            for (int i = 0; (i < this.configuration.UploadRetryCount) && (notifySuccess == false); i++)
                            {
                                // Do the notification on success or failure                    
                                mostRecentEx = null;

                                try
                                {
                                    channel.ScanFileUploaded(this.configuration.StationAccessId, blobUri, uploadSuccess);
                                    notifySuccess = true;
                                }
                                catch (ChannelTerminatedException cte)
                                {
                                    mostRecentEx = cte;
                                    System.Threading.Thread.Sleep(TimeSpan.FromMinutes(this.configuration.RetryDeltaBackoff));
                                }
                                catch (EndpointNotFoundException enfe)
                                {
                                    mostRecentEx = enfe;
                                    System.Threading.Thread.Sleep(TimeSpan.FromMinutes(this.configuration.RetryDeltaBackoff));
                                }
                                catch (ServerTooBusyException stbe)
                                {
                                    mostRecentEx = stbe;
                                    System.Threading.Thread.Sleep(TimeSpan.FromMinutes(this.configuration.RetryDeltaBackoff));
                                }
                                catch (Exception ex)
                                {
                                    mostRecentEx = ex;
                                    System.Threading.Thread.Sleep(TimeSpan.FromMinutes(this.configuration.RetryDeltaBackoff));
                                }
                            }

                            if (mostRecentEx != null)
                            {
                                this.logger.Log(TraceEventType.Error, LoggingMessageId.ImporterAgent, mostRecentEx.ToString());
                            }

                            if (uploadSuccess && notifySuccess)
                            {
                                this.OnFileInJobCompleted(watchFile, blobUri);
                            }
                            else
                            {
                                this.OnFileInJobFailed(watchFile, error);
                            }
                        }

                        if (channel != null)
                        {
                            channel.CloseOrAbort();
                        }
                    }

                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    // make sure that an exception doesn't cause us to stop uploading files, we need to keep trying
                    this.logger.Log(TraceEventType.Error, LoggingMessageId.ImporterAgent, ex.ToString());
                }
            }
        }

        private void OnFileInJobFailed(string failedFilePath, string error)
        {
            if (!string.IsNullOrWhiteSpace(failedFilePath) && File.Exists(failedFilePath))
            {
                string errorPath = FileHelper.GetUniqueFileName(this.configuration.InvalidFilesDirectory, failedFilePath);
                this.logger.Log(TraceEventType.Error, LoggingMessageId.MessageBufferEventId, string.Format(CultureInfo.InvariantCulture, "Error occurred while uploading the file {0} {1} Exception Details: {2}", errorPath, Environment.NewLine, error));
                using (TransactionScope scope = new TransactionScope())
                {
                    TransactedFileHelper.MoveFileTransacted(failedFilePath, errorPath);
                    scope.Complete();
                }
            }
        }

        private void OnFileInJobCompleted(string scanFilePath, string uploadedFilePath)
        {
            if (!string.IsNullOrWhiteSpace(uploadedFilePath) && File.Exists(scanFilePath))
            {
                try
                {
                    File.Delete(scanFilePath);
                }
                catch (IOException exception)
                {
                    this.logger.Log(TraceEventType.Error, LoggingMessageId.ImporterAgent, exception.ToString());
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (this.healthReport != null)
                {
                    this.healthReport.Dispose();
                }
            }
        }
    }
}
