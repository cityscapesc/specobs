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

namespace Microsoft.Spectrum.AutoUpdate
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Timers;
    using Microsoft.Spectrum.Common;
    using Microsoft.Win32;

    public class AutomaticUpdateAgent : IUpdateAgent
    {
        private const string ImporterRegistryKeyPath = @"Software\Microsoft\CityScape Spectrum Observatory";
        private const string PackageVersion = "SpectrumInstallerVersion";

        // For more info refer: http://msdn.microsoft.com/en-us/library/aa376931(v=vs.85).aspx
        private const int ErrorSuccess = 0;
        private const int ErrorSuccessRebootInitiated = 1641;
        private const int ErrorSuccessRebootRequired = 3010;

        private static object lockObject = new object();

        private readonly IDownloadAgent downloadAgent;
        private readonly ILogger logger;
        private readonly AutoUpdateConfiguration autoUpdateConfiguration;
        private readonly Timer timer;

        public AutomaticUpdateAgent(IDownloadAgent downloadAgent, ILogger logger, IConfigurationSource configurationSource)
        {
            if (downloadAgent == null)
            {
                throw new ArgumentNullException("downloadAgent");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (configurationSource == null)
            {
                throw new ArgumentNullException("configurationSource");
            }

            this.downloadAgent = downloadAgent;
            this.logger = logger;
            this.autoUpdateConfiguration = configurationSource.GetConfiguration();

            this.timer = new Timer();
            this.timer.Interval = this.autoUpdateConfiguration.UpdateInterval;
            this.timer.Elapsed += this.CheckForUpdates;
        }

        public void TurnAutomaticUpdatesOn()
        {
            IList<ValidationResult> validationResult = ValidationHelper.Validate(this.autoUpdateConfiguration);

            if (validationResult.Any())
            {
                throw new AutoUpdateConfigurationException("There are some issues with the specified configuration file.", validationResult);
            }

            this.logger.Log(TraceEventType.Information, LoggingMessageId.AutoUpdateAgent, "Configuration validated successfully");

            this.timer.Start();
            this.logger.Log(TraceEventType.Information, LoggingMessageId.AutoUpdatesOn, "Scheduled automatic updates");
        }

        public void TurnAutomaticUpdatesOff()
        {
            this.timer.Stop();
            this.logger.Log(TraceEventType.Information, LoggingMessageId.AutoUpdatesOff, "Cancelling scheduled automatic updates");
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
            }
        }

        private static string GetImportServiceRegistryKeyValue()
        {
            using (RegistryKey localMachineRegistry = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
            using (RegistryKey subKey = localMachineRegistry.OpenSubKey(ImporterRegistryKeyPath))
            {
                if (subKey != null)
                {
                    return (string)subKey.GetValue(PackageVersion);
                }
            }

            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is an unattended process.  It logs the failure for later analysis.")]
        private void CheckForUpdates(object sender, ElapsedEventArgs e)
        {
            // Make sure only one thread will check for updates and perform installation at a time.
            lock (lockObject)
            {
                try
                {
                    ServiceInstaller serviceInstaller = null;

                    if (!this.NewUpdatesFound(out serviceInstaller))
                    {
                        this.logger.Log(TraceEventType.Information, LoggingMessageId.NoUpdates, "No Updates are available");
                    }
                    else
                    {
                        this.InstallUpdates(serviceInstaller);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Log(TraceEventType.Error, LoggingMessageId.AutoUpdateAgent, ex.ToString());
                }
            }
        }

        private bool NewUpdatesFound(out ServiceInstaller serviceInstaller)
        {
            this.logger.Log(TraceEventType.Information, LoggingMessageId.UpdatesCheck, "Checking for updates...");

            Version currentAssemblyVersion;
            Version cloudAssemblyVersion;
            serviceInstaller = null;

            // This case will arise only when there is no registry key installed, which implies that machine doesn't have the installer installed.
            if (!Version.TryParse(GetImportServiceRegistryKeyValue(), out currentAssemblyVersion))
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.AutoUpdateAgent, string.Format(CultureInfo.InvariantCulture, "Could not find {0} registry entry.", PackageVersion));

                return false;
            }

            using (Stream stream = this.downloadAgent.OpenRead(this.autoUpdateConfiguration.ServiceInstallerConfigUri))
            {
                if (stream != null)
                {
                    serviceInstaller = XmlHelper.Deserialize<ServiceInstaller>(stream);
                }
            }

            if (serviceInstaller == null)
            {
                return false;
            }

            return Version.TryParse(serviceInstaller.Version, out cloudAssemblyVersion) && cloudAssemblyVersion.CompareTo(currentAssemblyVersion) > 0;
        }

        private void InstallUpdates(ServiceInstaller serviceInstaller)
        {
            this.logger.Log(TraceEventType.Information, LoggingMessageId.NewUpdates, "New updates available");

            string downloadUrl = serviceInstaller.DownloadPath;
            string fileName = Path.Combine(this.autoUpdateConfiguration.MsiDownloadPath, downloadUrl.Split('/').Last());

            this.downloadAgent.DownloadFile(new Uri(serviceInstaller.DownloadPath), fileName);

            using (Process updateInstallerProcess = Process.Start(fileName, "/q /norestart"))
            {
                updateInstallerProcess.WaitForExit();
                this.LogInsallationStatus(updateInstallerProcess.ExitCode);
            }
        }

        private void LogInsallationStatus(int exitCode)
        {
            switch (exitCode)
            {
                case ErrorSuccess:
                    this.logger.Log(TraceEventType.Information, LoggingMessageId.InstalledUpdates, "Installed updates successfully.");
                    break;

                case ErrorSuccessRebootInitiated:
                    this.logger.Log(TraceEventType.Information, LoggingMessageId.InstalledUpdates, "Installed updates successfully and the installer has initiated a restart");
                    break;

                case ErrorSuccessRebootRequired:
                    this.logger.Log(TraceEventType.Information, LoggingMessageId.InstalledUpdates, "A restart is required to complete the install");
                    break;

                default:
                    string msg = string.Format(
                        CultureInfo.InvariantCulture,
                        "Installation failed with ExitCode:{0}{1}Refer to http://msdn.microsoft.com/en-us/library/aa376931(v=vs.85).aspx for more information.",
                        exitCode,
                        Environment.NewLine);
                    this.logger.Log(TraceEventType.Error, LoggingMessageId.AutoUpdateAgent, msg);
                    break;
            }
        }
    }
}