namespace Microsoft.Spectrum.Import.Service
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.ServiceProcess;
    using System.Threading;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Common.Enums;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;
    using Microsoft.Spectrum.MeasurementStation.Client;
    using System.Diagnostics.Eventing.Reader;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Text;

    internal enum FileType
    {
        ScanFile,

        RaqIqFile
    }

    internal class HealthReporter : IDisposable
    {
        private const int ExitSuccess = 0;

        private const string UsrpDeviceErrorTitle = "USRP device is offline";
        private const string ScannerErrorTitle = "USRP Scanner is offline";
        private const string ImportServiceErrorTitle = "Import Service Error";

        private const string UrspFindDevicesCommand = "uhd_find_devices.exe";
        private const string ScannerServiceName = "SpectrumScannerService";

        private const int DefaultHealthStatusCheckIntervalInMin = 10;

        private readonly CancellationTokenSource cts;
        private SettingsConfigurationSection settingsConfiguration;
        private readonly DirectoryWatcherConfiguration directoryWatcherConfiguration;
        private MeasurementStationConfigurationEndToEnd measurementStationConfiguration = null;
        private readonly ILogger logger;
        private readonly EventLogWatcher eventLogWatcher;


        public HealthReporter(DirectoryWatcherConfiguration directoryWatcherConfiguration, SettingsConfigurationSection settingsConfiguration, ILogger logger)
        {
            if (directoryWatcherConfiguration == null)
            {
                throw new ArgumentNullException("directoryWatcherConfiguration", "DirectoryWatcherConfiguration instance can't be null");
            }

            if (settingsConfiguration == null)
            {
                throw new ArgumentNullException("settingsConfiguration", "SettingsConfiguration instance can't be null");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger", "Logger instance can't be null");
            }

            this.directoryWatcherConfiguration = directoryWatcherConfiguration;
            this.logger = logger;
            this.settingsConfiguration = settingsConfiguration;
            this.eventLogWatcher = new EventLogWatcher(new EventLogQuery("SpectrumScanningService", PathType.LogName));
            this.eventLogWatcher.EventRecordWritten += EventLogWatcher_EventRecordWritten;
            this.cts = new CancellationTokenSource();
        }

        private void EventLogWatcher_EventRecordWritten(object sender, EventRecordWrittenEventArgs e)
        {
            if ((int)TraceEventType.Error != e.EventRecord.Level)
            {
                return;
            }

            using (MeasurementStationServiceChannelFactory channelFactory = new MeasurementStationServiceChannelFactory())
            {
                IMeasurementStationServiceChannel channel = channelFactory.CreateChannel(this.directoryWatcherConfiguration.MeasurementStationServiceUri);
                string errorDescription = string.Format("Scanner encountered with errors , Exception details:{0}", e.EventException.ToString());

                channel.ReportHealthStatus(this.directoryWatcherConfiguration.StationAccessId, (int)StationHealthStatus.USRPDown, (int)MessagePriority.High, ScannerErrorTitle, errorDescription, DateTime.UtcNow);
                this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, errorDescription);
            }
        }

        public void AutoHealthReporterThread()
        {
            DateTime currentTime = DateTime.Now;
            DateTime usrpPingTime = currentTime;

            bool anyError = false;

            while (!cts.IsCancellationRequested)
            {
                using (MeasurementStationServiceChannelFactory channelFactory = new MeasurementStationServiceChannelFactory())
                {
                    IMeasurementStationServiceChannel channel = channelFactory.CreateChannel(this.directoryWatcherConfiguration.MeasurementStationServiceUri);

                    int intervalOfCheckInMin = channel.GetHealthStatusCheckInterval(this.directoryWatcherConfiguration.StationAccessId);

                    if (intervalOfCheckInMin < 1)
                    {
                        intervalOfCheckInMin = DefaultHealthStatusCheckIntervalInMin;
                    }

                    anyError = !this.ReadUsrpConfiguration(channel);

                    if (DateTime.Now >= usrpPingTime && !anyError)
                    {
                        anyError = this.CheckForUSRPDown(channel);
                        usrpPingTime = usrpPingTime.AddMinutes(intervalOfCheckInMin);

                        if (!anyError
                                && (this.CheckForScanFile(channel) || this.CheckForRaqIqFile(channel)))
                        {
                            anyError = this.CheckForImportServiceUploadFailures(channel);
                        }
                    }

                    // Sleep for configured interval minutes
                    Thread.Sleep(intervalOfCheckInMin * 60 * 1000);
                }
            }
        }

        public void UsrpScannerConfigurationChanged(MeasurementStationConfigurationEndToEnd newStationConfigurationSettings)
        {
            using (MeasurementStationServiceChannelFactory channelFactory = new MeasurementStationServiceChannelFactory())
            {
                IMeasurementStationServiceChannel channel = channelFactory.CreateChannel(this.directoryWatcherConfiguration.MeasurementStationServiceUri);

                var oldConfig = this.measurementStationConfiguration;
                string oldConfigJson = JsonConvert.SerializeObject(oldConfig);
                string newConfigJson = JsonConvert.SerializeObject(newStationConfigurationSettings);

                this.measurementStationConfiguration = newStationConfigurationSettings;

                if (!string.IsNullOrWhiteSpace(oldConfigJson)
                    && !string.IsNullOrWhiteSpace(newConfigJson))
                {
                    string difference = this.GetConfigDifferences(oldConfigJson, newConfigJson);

                    if (!string.IsNullOrWhiteSpace(difference))
                    {
                        string description = string.Format("New Configuration settings on host machine:{0}", difference);
                        channel.ReportHealthStatus(this.directoryWatcherConfiguration.StationAccessId, (int)StationHealthStatus.UsrpScannerConfigurationChanged, (int)MessagePriority.High, "USRP Scanner configuration settings changed", description, DateTime.UtcNow);

                        this.logger.Log(TraceEventType.Information, LoggingMessageId.SpectrumObservatoriesMonitoringService, description);
                    }
                }
            }
        }

        public string GetConfigDifferences(string oldConfig, string currentConfig)
        {
            StringBuilder changes = new StringBuilder();

            try
            {
                JObject sourceJObject = JsonConvert.DeserializeObject<JObject>(oldConfig);
                JObject targetJObject = JsonConvert.DeserializeObject<JObject>(currentConfig);


                if (!JToken.DeepEquals(sourceJObject, targetJObject))
                {
                    foreach (KeyValuePair<string, JToken> sourceProperty in sourceJObject)
                    {
                        JProperty targetProp = targetJObject.Property(sourceProperty.Key);

                        if (!JToken.DeepEquals(sourceProperty.Value, targetProp.Value))
                        {
                            //changes.Append(string.Format("{0} property value is changed", sourceProperty.Key));
                            changes.Append(string.Format("{0} - Current Value :{1}", sourceProperty.Key, targetProp.Value));
                            changes.AppendLine("<br>");
                            changes.Append(string.Format("Old Value :{0}", sourceProperty.Value));
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                string description = string.Format("Error occurred while performing configuration comparison, Exception:{0}", ex.ToString());
                this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, description);

                if (!string.IsNullOrWhiteSpace(oldConfig)
                    && !string.IsNullOrWhiteSpace(currentConfig)
                    && string.Compare(oldConfig, currentConfig, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return currentConfig;
                }
            }

            return changes.ToString();
        }

        //public void CompareJTokenProperties(JToken sourceProperty, JToken targetProperty, StringBuilder differences)
        //{
        //    if (!JToken.DeepEquals(sourceProperty, targetProperty))
        //    {
        //        differences.Append(string.Format("{0}:", sourceProperty.Path));

        //        CompareJTokenProperties(sourceProperty., targetProperty.Children(), differences);

        //        differences.Append(string.Format("Current Value : {0}, Old Value : {1}", targetProperty, sourceProperty));
        //    }
        //}

        public void PingRequestThread()
        {
            while (!cts.IsCancellationRequested)
            {

            }
        }

        public void ShutDown()
        {
            this.cts.Cancel();
            this.eventLogWatcher.EventRecordWritten -= EventLogWatcher_EventRecordWritten;
            this.logger.Log(System.Diagnostics.TraceEventType.Information, LoggingMessageId.SpectrumObservatoriesMonitoringService, "Stopping HealthReport Service");
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
                if (cts != null)
                {
                    cts.Dispose();
                }
            }
        }

        private bool ReadUsrpConfiguration(IMeasurementStationServiceChannel channel)
        {
            bool readSuccess = true;

            if (this.settingsConfiguration != null
                && File.Exists(this.settingsConfiguration.MeasurementStationConfigurationFileFullPath))
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
                    string errorDescription = string.Format(CultureInfo.InvariantCulture, "Unable to read Scanner configuration file. it seems that scanner configuration corrupted:{0}", this.directoryWatcherConfiguration.StationAccessId);
                    channel.ReportHealthStatus(this.directoryWatcherConfiguration.StationAccessId, (int)StationHealthStatus.USRPDown, (int)MessagePriority.High, ScannerErrorTitle, errorDescription, DateTime.UtcNow);

                    this.measurementStationConfiguration = null;
                    readSuccess = false;
                }
            }
            else
            {
                readSuccess = false;

                string errorDescription = string.Format(CultureInfo.InvariantCulture, "USRP scanner configuration file is missing on the host machine");
                channel.ReportHealthStatus(this.directoryWatcherConfiguration.StationAccessId, (int)StationHealthStatus.USRPDown, (int)MessagePriority.High, ScannerErrorTitle, errorDescription, DateTime.UtcNow);
            }

            return readSuccess;
        }

        private bool CheckForUSRPDown(IMeasurementStationServiceChannel channel)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + UrspFindDevicesCommand);
            processInfo.CreateNoWindow = false;
            processInfo.UseShellExecute = false;
            processInfo.WindowStyle = ProcessWindowStyle.Normal;

            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            bool usrpRunning = true;

            try
            {
                Process uhdProcess = Process.Start(processInfo);

                string output = uhdProcess.StandardOutput.ReadToEnd();
                string errors = uhdProcess.StandardError.ReadToEnd();

                uhdProcess.WaitForExit();
                int exitCode = uhdProcess.ExitCode;

                if (exitCode != 0
                    || (!string.IsNullOrWhiteSpace(errors)))
                {
                    usrpRunning = false;
                    string description = string.Format(CultureInfo.InvariantCulture, "Not able to reach USRP device from host machine and seem offline");

                    if (errors != null)
                    {
                        description = string.Format(CultureInfo.InvariantCulture, "{0}, Error returned as :{1}", description, errors);
                    }

                    channel.ReportHealthStatus(this.directoryWatcherConfiguration.StationAccessId, (int)StationHealthStatus.USRPDown, (int)MessagePriority.High, UsrpDeviceErrorTitle, description, DateTime.UtcNow);
                    this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, description);
                }
            }
            catch (Exception ex)
            {
                usrpRunning = false;
                string description = string.Format(CultureInfo.InvariantCulture, "Not able to reach USRP device from host machine and seem offline, Exceptions;{0}", ex.ToString());
                channel.ReportHealthStatus(this.directoryWatcherConfiguration.StationAccessId, (int)StationHealthStatus.USRPDown, (int)MessagePriority.High, UsrpDeviceErrorTitle, description, DateTime.UtcNow);

                this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, description);
            }

            return !usrpRunning;
        }

        private bool CheckForImportServiceUploadFailures(IMeasurementStationServiceChannel channel)
        {
            bool existLongLivedScanFiles = false;
            bool existLongLivedRaqwIqFiles = false;
            string[] scanFiles = Directory.GetFiles(this.directoryWatcherConfiguration.WatchDirectory, "*.dsox", SearchOption.TopDirectoryOnly);
            string[] rawIqFiles = Directory.GetFiles(this.directoryWatcherConfiguration.WatchDirectory, "*.dsor", SearchOption.TopDirectoryOnly);

            // Found more than 2 files found not uploaded or un processed
            if (scanFiles != null
                && scanFiles.Length > 2)
            {
                for (int i = 0; i < scanFiles.Length; i++)
                {
                    FileInfo file = new FileInfo(scanFiles[i]);

                    if (DateTime.Now.Subtract(file.CreationTime).TotalHours > 2)
                    {
                        existLongLivedScanFiles = true;
                        break;
                    }
                }
            }

            if (rawIqFiles != null
                && rawIqFiles.Length > 2)
            {
                RawIqDataConfigurationElement rawIqConfiguration = this.measurementStationConfiguration.RawIqConfiguration;

                for (int i = 0; i < rawIqFiles.Length; i++)
                {
                    FileInfo file = new FileInfo(scanFiles[i]);

                    if (DateTime.Now.Subtract(file.CreationTime).TotalSeconds >= rawIqConfiguration.SecondsOfDataPerFile * 2)
                    {
                        existLongLivedRaqwIqFiles = true;
                        break;
                    }
                }
            }

            if (existLongLivedScanFiles
                || existLongLivedRaqwIqFiles)
            {
                string description = "Import Service is not uploading files";

                if (existLongLivedScanFiles)
                {
                    description = string.Format("{0}, there are {1} scan files", description, scanFiles.Length);
                }

                if (existLongLivedRaqwIqFiles)
                {
                    description = string.Format("{0}, there are {1} RawIq files", description, rawIqFiles.Length);
                }

                description = string.Format(CultureInfo.InvariantCulture, "{0} files found unprocessed on the host machine at the time of check, {1} UTC", description, DateTime.UtcNow);
                channel.ReportHealthStatus(this.directoryWatcherConfiguration.StationAccessId, (int)StationHealthStatus.ScannerServiceDown, (int)MessagePriority.High, ImportServiceErrorTitle, description, DateTime.UtcNow);

                this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, description);
            }

            return existLongLivedScanFiles;
        }

        private bool CheckForScanServiceDownCheck(IMeasurementStationServiceChannel channel)
        {
            bool scannerServiceIsDown = false;
            using (ServiceController scannerService = new ServiceController(ScannerServiceName))
            {
                if (scannerService.Status != ServiceControllerStatus.Running)
                {
                    string description = string.Format(CultureInfo.InvariantCulture, "USRP scanner on the host machine is either stopped or faulted.  No scan files found on the host.  USRP device is running as expected.");
                    channel.ReportHealthStatus(this.directoryWatcherConfiguration.StationAccessId, (int)StationHealthStatus.ScannerServiceDown, (int)MessagePriority.High, ScannerErrorTitle, description, DateTime.UtcNow);

                    this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, description);
                    scannerServiceIsDown = true;
                }
            }

            return scannerServiceIsDown;
        }

        private bool CheckForScanFile(IMeasurementStationServiceChannel channel)
        {
            if (this.CheckForScanServiceDownCheck(channel))
            {
                return true;
            }

            ClientAggregationConfiguration aggregationConfiguration = this.measurementStationConfiguration.AggregationConfiguration;
            DateTime roundedTimeStamp = new DateTime((DateTime.UtcNow.Ticks / aggregationConfiguration.MinutesOfDataPerScanFile.Ticks) * aggregationConfiguration.MinutesOfDataPerScanFile.Ticks);

            string sortableDateTime = this.GetSortableDateTime(roundedTimeStamp, FileType.ScanFile);
            string prevSortableDateTime = this.GetSortableDateTime(roundedTimeStamp.AddMinutes(-aggregationConfiguration.MinutesOfDataPerScanFile.TotalMinutes), FileType.ScanFile);
            string nextSortableDateTime = this.GetSortableDateTime(roundedTimeStamp.AddMinutes(aggregationConfiguration.MinutesOfDataPerScanFile.TotalMinutes), FileType.ScanFile);

            // Current rounded timestamp
            string filePathTmep = Path.Combine(this.directoryWatcherConfiguration.WatchDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.tmp", sortableDateTime));
            string filePathDsox = Path.Combine(this.directoryWatcherConfiguration.WatchDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.dsox", sortableDateTime));


            // Previous rounded timestamp
            string prevFilePathTmep = Path.Combine(this.directoryWatcherConfiguration.WatchDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.tmp", prevSortableDateTime));
            string prevFilePathDsox = Path.Combine(this.directoryWatcherConfiguration.WatchDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.dsox", prevSortableDateTime));

            // Next rounded timestamp
            string nextFilePathTmep = Path.Combine(this.directoryWatcherConfiguration.WatchDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.tmp", nextSortableDateTime));
            string nextFilePathDsox = Path.Combine(this.directoryWatcherConfiguration.WatchDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.dsox", nextSortableDateTime));

            // If there isn't a matching Scan file for the current round off time (+/-) 1 then inferring that something wrong with the scanner that it is not producing scan files.
            bool foundScanFiles = (File.Exists(filePathTmep) || File.Exists(filePathDsox))
                                  || (File.Exists(prevFilePathTmep) || File.Exists(prevFilePathDsox))
                                  || File.Exists(nextFilePathTmep) || File.Exists(nextFilePathDsox);

            if (!foundScanFiles)
            {
                string description = string.Format(CultureInfo.InvariantCulture, "USRP scanner on the host machine is either stopped or faulted.  No scan files found on the host.  USRP device is running as expected.");
                channel.ReportHealthStatus(this.directoryWatcherConfiguration.StationAccessId, (int)StationHealthStatus.ScannerServiceDown, (int)MessagePriority.High, ScannerErrorTitle, description, DateTime.UtcNow);

                string logDescription = string.Format("{0}, ScanFile current rounded time:{1}", description, sortableDateTime);
                this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, logDescription);
            }

            return !foundScanFiles;
        }

        private string GetSortableDateTime(DateTime roundedTimeStamp, FileType fileType)
        {
            string sortableDateTime = roundedTimeStamp.ToString("s", CultureInfo.InvariantCulture);
            sortableDateTime = sortableDateTime.Replace(":", null);

            if (fileType == FileType.RaqIqFile)
            {
                return sortableDateTime;
            }

            sortableDateTime = sortableDateTime.Remove(sortableDateTime.Length - 2); // Remove seconds      

            return sortableDateTime;
        }


        private bool CheckForRaqIqFile(IMeasurementStationServiceChannel channel)
        {
            if (this.CheckForScanServiceDownCheck(channel))
            {
                return true;
            }

            RawIqDataConfigurationElement rawIqConfigurationElement = this.measurementStationConfiguration.RawIqConfiguration;
            TimeSpan secondsOfDataPerFile = TimeSpan.FromSeconds(rawIqConfigurationElement.SecondsOfDataPerFile);

            DateTime roundedTimeStamp = new DateTime((DateTime.UtcNow.Ticks / secondsOfDataPerFile.Ticks) * secondsOfDataPerFile.Ticks);

            string sortableDateTime = this.GetSortableDateTime(roundedTimeStamp, FileType.RaqIqFile);
            string prevSortableDateTime = this.GetSortableDateTime(roundedTimeStamp.AddSeconds(-rawIqConfigurationElement.SecondsOfDataPerFile), FileType.RaqIqFile);
            string nextSortableDateTime = this.GetSortableDateTime(roundedTimeStamp.AddSeconds(rawIqConfigurationElement.SecondsOfDataPerFile), FileType.RaqIqFile);

            // Current rounded time RawIq file
            string filePathTmp = Path.Combine(directoryWatcherConfiguration.WatchDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.tmp", sortableDateTime));
            string filePathDsor = Path.Combine(directoryWatcherConfiguration.WatchDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.dsor", sortableDateTime));

            // Previous rounded time RawIq file
            string prevFilePathTmp = Path.Combine(directoryWatcherConfiguration.WatchDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.tmp", sortableDateTime));
            string prevFilePathDsor = Path.Combine(directoryWatcherConfiguration.WatchDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.dsor", sortableDateTime));

            // Next rounded time RawIq file
            string nextFilePathTmp = Path.Combine(directoryWatcherConfiguration.WatchDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.tmp", sortableDateTime));
            string nextFilePathDsor = Path.Combine(directoryWatcherConfiguration.WatchDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.dsor", sortableDateTime));

            // If there isn't a matching RaqIq file for the current round off time (+/-) 1 then inferring that something wrong with the scanner that it is not producing RawIq files.
            bool foundRawIqfile = (File.Exists(filePathTmp) || File.Exists(filePathDsor))
                                  || (File.Exists(prevFilePathTmp) || File.Exists(prevFilePathDsor))
                                  || (File.Exists(nextFilePathTmp) || File.Exists(nextFilePathDsor));

            if (!foundRawIqfile)
            {
                string description = string.Format(CultureInfo.InvariantCulture, "USRP scanner on the host machine is either stopped or faulted.No RawIq files found on the host machine.USRP device is running as expected.");
                channel.ReportHealthStatus(this.directoryWatcherConfiguration.StationAccessId, (int)StationHealthStatus.ScannerServiceDown, (int)MessagePriority.High, ScannerErrorTitle, description, DateTime.UtcNow);

                string logDescription = string.Format("{0}, RawIqFile current rounded time :{1}", description, roundedTimeStamp);
                this.logger.Log(TraceEventType.Error, LoggingMessageId.SpectrumObservatoriesMonitoringService, logDescription);
            }

            return !foundRawIqfile;
        }

        //private bool CheckForErrorLogs(IMeasurementStationServiceChannel channel)
        //{
        //    string[] logFiles = Directory.GetFiles(this.directoryWatcherConfiguration.WatchDirectory, "*.log", SearchOption.TopDirectoryOnly);

        //    if (logFiles != null)
        //    {
        //        return logFiles.Any();
        //    }

        //    return false;
        //}
    }
}
