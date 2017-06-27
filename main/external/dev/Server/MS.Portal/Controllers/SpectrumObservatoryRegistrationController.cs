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

namespace Microsoft.Spectrum.Portal.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;
    using Microsoft.Spectrum.Portal.Helpers;
    using Microsoft.Spectrum.Portal.Models;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Storage.Models;

    public class SpectrumObservatoryRegistrationController : Controller
    {
        [AllowAnonymous]
        public PartialViewResult BeginRegistration()
        {
            return this.PartialView("BeginRegistration");
        }

        public ActionResult RegistrationSteps()
        {
            StationRegistrationInputs registrationSteps = new StationRegistrationInputs();

            registrationSteps.ContactInfo = Utility.GetStationContactByProviderUserId(this.User.Identity.Name);

            registrationSteps.Address = new Models.Address();

            registrationSteps.ClientAggregationConfiguration = new ClientAggregationConfigurationViewModel
            {
                MinutesOfDataPerScanFile = PortalConstants.ClientAggregationMinutesOfDataPerScanFile,
                SecondsOfDataPerSample = PortalConstants.ClientAggregationSecondsOfDataPerScanFile
            };

            registrationSteps.Gps = new Portal.Models.GpsDetails();

            registrationSteps.RawIqDataConfiguration = new RawIqDataConfigurationElementViewModel
            {
                SecondsOfDataPerFile = PortalConstants.RawIQSecondsOfDataPerFile,
                RetentionSeconds = PortalConstants.RawIQRetentionSeconds,
                OutputData = true
            };

            // For the first entry disable delete entry option.
            RFSensorConfigurationEndToEndViewModel rfsensorConfig = Utility.GetEmptyRFSensorConfigurationViewModel(false);

            registrationSteps.RFSensorConfigurationEndToEnd = new List<RFSensorConfigurationEndToEndViewModel>() { rfsensorConfig };

            IEnumerable<string> countries = PortalGlobalCache.Instance.Countries;
            registrationSteps.Countries = new SelectList(countries);

            IEnumerable<string> stationTypes = Utility.GetStationTypeCollection();
            registrationSteps.StationTypes = new SelectList(stationTypes);

            return this.View("RegistrationSteps", registrationSteps);
        }

        public ActionResult Register(StationRegistrationInputs inputs)
        {
            if (!this.ModelState.IsValid)
            {
                IEnumerable<string> scanPatterns = Utility.GetScanPattern();
                IEnumerable<string> deviceTypes = Utility.GetScannerDeviceTypes();
                IEnumerable<string> countries = Utility.GetCountries();
                IEnumerable<string> stationTypes = Utility.GetStationTypeCollection();

                inputs.Countries = new SelectList(countries);
                inputs.StationTypes = new SelectList(stationTypes);

                if (inputs.RFSensorConfigurationEndToEnd == null)
                {
                    inputs.RFSensorConfigurationEndToEnd = new List<RFSensorConfigurationEndToEndViewModel>();
                }

                foreach (RFSensorConfigurationEndToEndViewModel sensorConfig in inputs.RFSensorConfigurationEndToEnd)
                {
                    sensorConfig.ScanPatterns = new SelectList(scanPatterns);
                    sensorConfig.DeviceTypes = new SelectList(deviceTypes);
                }

                string jsonViewData = Utility.PartialView(this, "Register", inputs);

                return this.Json(new { ViewData = jsonViewData, ServerValidationErrors = true, ErrorMessage = string.Empty });
            }

            IMeasurementStationTableOperations measurementStationTableOperations = new MeasurementStationTableOperations(PortalGlobalCache.Instance.MasterAzureTableDbContext);
            ISpectrumDataProcessorMetadataStorage spectrumDataProcessorMetadataStorage = new SpectrumDataProcessorMetadataStorage(PortalGlobalCache.Instance.MasterAzureTableDbContext, PortalGlobalCache.Instance.Logger);

            RawIQPolicy rawIQPolicy = GetRawIQPolicy((long)MathLibrary.MHzToHz(inputs.RawIqDataConfiguration.StartFrequencyMHz), (long)MathLibrary.MHzToHz(inputs.RawIqDataConfiguration.StopFrequencyMHz));

            if (rawIQPolicy != null)
            {
                // The default Dutycycle on time is the upper bound value set in RawIQPolicy, if the user enter value is zero or negative or greater than upper bound set.
                if (inputs.RawIqDataConfiguration.DutycycleOnTimeInMilliSec <= 0
                    || inputs.RawIqDataConfiguration.DutycycleOnTimeInMilliSec > rawIQPolicy.DutycycleOnTimeUpperBoundInMilliSec)
                {
                    inputs.RawIqDataConfiguration.DutycycleOnTimeInMilliSec = rawIQPolicy.DutycycleOnTimeUpperBoundInMilliSec;
                }

                // The default Dutyclycle time is the lower bound value set for dutycycle time in RawIQPolicy, if the entered value is less than lower bound. If the enter value is greater than file duration by default dutycycle time will be reset to lowerbound.
                if ((inputs.RawIqDataConfiguration.DutycycleTimeInMilliSec < rawIQPolicy.DutycycleTimeLowerBoundInMilliSec)
                    || (inputs.RawIqDataConfiguration.DutycycleTimeInMilliSec > rawIQPolicy.DutycycleTimeUpperBoundInMilliSec))
                {
                    inputs.RawIqDataConfiguration.DutycycleTimeInMilliSec = rawIQPolicy.DutycycleTimeLowerBoundInMilliSec;
                }             

                inputs.RawIqDataConfiguration.SecondsOfDataPerFile = rawIQPolicy.FileDurationInSec;
                inputs.RawIqDataConfiguration.RetentionSeconds = rawIQPolicy.RetentionTimeInSec;
            }

            MeasurementStationManager stationManager = new MeasurementStationManager(measurementStationTableOperations, SpectrumDataStorageAccountsTableOperations.Instance, PortalGlobalCache.Instance.Logger);

            MeasurementStationConfigurationEndToEnd mesurementStationConfigEndToEnd = Utility.ConvertToMeasurementStationConfigurationEndToEnd(inputs);

            long startFrequency = (long)mesurementStationConfigEndToEnd.RFSensorConfigurations.Min(sensor => sensor.CurrentStartFrequencyHz);
            long stopFrequency = (long)mesurementStationConfigEndToEnd.RFSensorConfigurations.Max(sensor => sensor.CurrentStopFrequencyHz);

            try
            {
                Guid measurementStationId = PortalGlobalCache.Instance.StationManger.CreateNewStation(
                    inputs.Address.Location,
                    inputs.Address.AddressLine1,
                    inputs.Address.AddressLine2,
                    inputs.Address.Country,
                    inputs.RFSensorConfigurationEndToEnd.FirstOrDefault().Antennas.First().AntennaType,
                    inputs.RadioType,
                    inputs.StationType,
                    startFrequency,
                    stopFrequency,
                    mesurementStationConfigEndToEnd,
                    inputs.Gps.Latitude,
                    inputs.Gps.Longitude,
                    inputs.Gps.Elevation,
                    inputs.StationName,
                    inputs.Description,
                    inputs.ContactInfo.PrimaryContactName,
                    inputs.ContactInfo.PrimaryContactPhone,
                    inputs.ContactInfo.PrimaryContactEmail,
                    inputs.ContactInfo.PrimaryContactUserId,
                    inputs.ReceiveHealthStatusNotifications,
                    inputs.ClientHealthStatusCheckIntervalInMin);

                StationRegistrationViewModel registrationViewModel = ConvertToStationRegistrationViewModel(measurementStationId, inputs.StationName);

                string jsonViewData = Utility.PartialView(this, "HostConfiguration", registrationViewModel);

                return this.Json(new { ViewData = jsonViewData, ServerValidationErrors = false, ErrorMessage = string.Empty });
            }
            catch (Exception ex)
            {
                PortalGlobalCache.Instance.Logger.Log(TraceEventType.Error, LoggingMessageId.PortalUnhandledError, ex.ToString());

                return this.Json(new { ViewData = string.Empty, ServerValidationErrors = false, ErrorMessage = "CityScape Spectrum Observatory registration failed: Please contact Site Administrator for more information" });
            }
        }

        public JsonResult LoadRawIQScanPolicy(StationRegistrationInputs inputs)
        {
            var rawIQConfig = inputs.RawIqDataConfiguration;
            RawIQPolicy rawIQPolicy = GetRawIQPolicy((long)MathLibrary.MHzToHz(rawIQConfig.StartFrequencyMHz), (long)MathLibrary.MHzToHz(rawIQConfig.StopFrequencyMHz));

            if (rawIQConfig != null)
            {
                // The default Dutycycle on time is the upper bound value set in RawIQPolicy, if the user enter value is zero or negative or greater than upper bound set.
                if (rawIQConfig.DutycycleOnTimeInMilliSec <= 0
                    || rawIQConfig.DutycycleOnTimeInMilliSec > rawIQPolicy.DutycycleOnTimeUpperBoundInMilliSec)
                {
                    rawIQConfig.DutycycleOnTimeInMilliSec = rawIQPolicy.DutycycleOnTimeUpperBoundInMilliSec;
                }

                // The default Dutyclycle time is the lower bound value set for dutycycle time in RawIQPolicy, if the entered value is less than lower bound. If the enter value is greater than file duration by default dutycycle time will be reset to lowerbound.              
                if ((inputs.RawIqDataConfiguration.DutycycleTimeInMilliSec < rawIQPolicy.DutycycleTimeLowerBoundInMilliSec)
                    || (inputs.RawIqDataConfiguration.DutycycleTimeInMilliSec > rawIQPolicy.DutycycleTimeUpperBoundInMilliSec))
                {
                    inputs.RawIqDataConfiguration.DutycycleTimeInMilliSec = rawIQPolicy.DutycycleTimeLowerBoundInMilliSec;
                }               

                //rawIQConfig.DutycycleOnTimeInMilliSec = rawIQPolicy.DutycycleOnTimeUpperBoundInMilliSec;
                //rawIQConfig.DutycycleTimeInMilliSec = rawIQPolicy.DutycycleTimeLowerBoundInMilliSec;

                rawIQConfig.SecondsOfDataPerFile = rawIQPolicy.FileDurationInSec;
                rawIQConfig.RetentionSeconds = rawIQPolicy.RetentionTimeInSec;
                rawIQConfig.RawIQScanPolicyCategory = rawIQPolicy.Category;
                rawIQConfig.RawIQPolicyDetails = rawIQPolicy.PolicyDetails;
            }
            else
            {
                return this.Json(new { ViewData = string.Empty, ServerValidationErrors = false, ErrorMessage = "Couldn't able to load RawIQPolicy for the given frequency range" });
            }

            string jsonResult = Utility.PartialView(this, "EditorTemplates/RawIQScanPolicySettings", inputs);

            return this.Json(new { ViewData = jsonResult, ServerValidationErrors = false, ErrorMessage = string.Empty });

        }

        public ActionResult DownloadInstaller()
        {
            string installerSettingKey = "Installer";
            string filePath = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.DeviceSetupCategory, installerSettingKey, string.Empty);
            string fileName = string.Empty;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                PortalGlobalCache.Instance.Logger.Log(TraceEventType.Error, LoggingMessageId.PortalUnhandledError, "No file path exist for MSI download");

                throw new InvalidOperationException(string.Format("No file exist for the setting Category {0} Key {1}", SettingsTableHelper.DeviceSetupCategory, installerSettingKey));
            }

            fileName = filePath.Split('/').Last();
            byte[] data = null;

            using (WebClient webClient = new WebClient())
            {
                data = webClient.DownloadData(new Uri(filePath));
            }

            return this.File(data, "application/octet-stream", fileName);
        }

        public FileResult DownloadSetupManual()
        {
            string deviceSetupSettingsKey = "SetupManual";

            string filePath = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.DeviceSetupCategory, deviceSetupSettingsKey, string.Empty);

            string fileName = string.Empty;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                PortalGlobalCache.Instance.Logger.Log(TraceEventType.Error, LoggingMessageId.PortalUnhandledError, "No file path exist for SetupGuide");

                throw new InvalidOperationException(string.Format("No file exist for the setting Category {0} Key {1}", SettingsTableHelper.DeviceSetupCategory, deviceSetupSettingsKey));
            }

            fileName = filePath.Split('/').Last();

            byte[] data = null;

            using (WebClient webClient = new WebClient())
            {
                data = webClient.DownloadData(new Uri(filePath));
            }

            return this.File(data, "application/pdf", fileName);
        }

        public PartialViewResult AddCable(string htmlPrefixName)
        {
            this.ViewData["PrefixName"] = htmlPrefixName;
            CableConfigurationViewModel cableCofig = new CableConfigurationViewModel();
            cableCofig.AllowCableEntryDelete = true;

            return this.PartialView("EditorTemplates/CableConfiguration", cableCofig);
        }

        public PartialViewResult AddAntenna(string htmlPrefixName)
        {
            this.ViewData["PrefixName"] = htmlPrefixName;
            AntennaConfigurationViewModel antennaConfig = new AntennaConfigurationViewModel();
            antennaConfig.AllowAntennaEntryDelete = true;

            return this.PartialView("EditorTemplates/AntennaConfiguration", antennaConfig);
        }

        public PartialViewResult AddConnector(string htmlPrefixName)
        {
            this.ViewData["PrefixName"] = htmlPrefixName;
            ConnectorConfigurationViewModel cableCofig = new ConnectorConfigurationViewModel();
            cableCofig.AllowConnectorEntryDelete = true;

            return this.PartialView("EditorTemplates/ConnectorConfiguration", cableCofig);
        }

        public PartialViewResult AddRFSensor(string htmlPrefixName)
        {
            this.ViewData["PrefixName"] = htmlPrefixName;

            // Allow delete option for the newly added RF Sesnor configurations.
            RFSensorConfigurationEndToEndViewModel rfsensorConfig = Utility.GetEmptyRFSensorConfigurationViewModel(true);

            return this.PartialView("EditorTemplates/RFSensorConfiguration", rfsensorConfig);
        }

        private static StationRegistrationViewModel ConvertToStationRegistrationViewModel(Guid measurementStationId, string spectrumObservatoryName)
        {
            return new StationRegistrationViewModel
            {
                MeasurementStationId = measurementStationId.ToString(),
                SpectrumObservatoryName = spectrumObservatoryName
            };
        }

        private static RawIQPolicy GetRawIQPolicy(long startFrequency, long stopFrequency)
        {
            IMeasurementStationTableOperations measurementStationTableOperations = new MeasurementStationTableOperations(PortalGlobalCache.Instance.MasterAzureTableDbContext);
            IEnumerable<RawIQPolicy> rawIQPolicyBands = measurementStationTableOperations.GetOverlappingIQBands(startFrequency, stopFrequency);

            if (rawIQPolicyBands != null
                && rawIQPolicyBands.Any())
            {
                int topBandPriority = rawIQPolicyBands.Min(policy => policy.BandPriority);
                RawIQPolicy applicablePolicy = rawIQPolicyBands.FirstOrDefault(policy => policy.BandPriority == topBandPriority);

                return applicablePolicy;
            }

            return null;
        }
    }
}