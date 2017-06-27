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

namespace Microsoft.Spectrum.Portal.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Xml.Linq;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;
    using Microsoft.Spectrum.Portal.Models;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Models;
    using Microsoft.Spectrum.Storage.Table.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using AddressEntity = Microsoft.Spectrum.Storage.Models.Address;
    using AddressViewModel = Microsoft.Spectrum.Portal.Models.Address;
    using GpsEntity = Microsoft.Spectrum.Storage.Models.GpsDetails;
    using GpsViewModel = Microsoft.Spectrum.Portal.Models.GpsDetails;
    using System.IO.Compression;
    using System.Text;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Portal.Helpers
    /// Class:          Utility
    /// Description:    Utility class for common operations
    /// ----------------------------------------------------------------- 
    public static class Utility
    {
        /// <summary>
        /// Get all time zones
        /// </summary>
        /// <returns>list of all time zones</returns>
        public static IEnumerable<string> GetTimeZones()
        {
            List<string> timezoneList = new List<string>();

            var timeZones = TimeZoneInfo.GetSystemTimeZones();
            foreach (var timezone in timeZones)
            {
                timezoneList.Add(timezone.DisplayName);
            }

            return timezoneList;
        }

        /// <summary>
        /// reads all the country phone codes from config file
        /// </summary>
        /// <returns>list of phone codes</returns>
        public static IEnumerable<string> GetCountryPhoneCodes()
        {
            List<string> codeDictionary = new List<string>();
            string filePath = string.Empty;

            if (RoleEnvironment.IsAvailable)
            {
                filePath = Environment.GetEnvironmentVariable("RoleRoot") + "\\approot\\bin\\ContryCodes.config";
                if (!File.Exists(filePath))
                {
                    filePath = Environment.GetEnvironmentVariable("RoleRoot") + "\\approot\\ContryCodes.config";
                }
            }
            else if (HttpContext.Current != null && HttpContext.Current.Server != null)
            {
                filePath = HttpContext.Current.Server.MapPath(@"~\bin") + "\\ContryCodes.config";
            }

            XDocument xmlDoc = XDocument.Load(filePath);
            var countryCodes = xmlDoc.Descendants("data-set").Elements("countrycode");

            foreach (var countryCode in countryCodes)
            {
                var country = countryCode.Elements("country").Select(r => r.Value).FirstOrDefault();
                var code = countryCode.Elements("code").Select(r => r.Value).FirstOrDefault();

                codeDictionary.Add(country + "(+" + code + ")");
            }

            return codeDictionary;
        }

        /// <summary>
        /// Get List of Countries
        /// </summary>
        /// <returns>list of countries</returns>
        public static IEnumerable<string> GetCountries()
        {
            List<string> countryList = new List<string>();

            // Iterate the Framework Cultures...
            foreach (CultureInfo ci in CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures))
            {
                RegionInfo ri = null;
                try
                {
                    ri = new RegionInfo(ci.Name);
                }
                catch
                {
                    // If a RegionInfo object could not be created we don't want to use the CultureInfo
                    //    for the country list.
                    continue;
                }

                if (!countryList.Contains(ri.EnglishName))
                {
                    countryList.Add(ri.EnglishName);
                }
            }

            countryList.Sort();
            countryList.MoveItemAtIndexToFront(countryList.IndexOf("United States"));

            return countryList;
        }

        public static AzureTableDbContext GetAzureTableContext(string storageName)
        {
            return new AzureTableDbContext(SpectrumDataStorageAccountsTableOperations.Instance.GetCloudStorageAccountByName(storageName), PortalGlobalCache.GlobalRetryPolicy);
        }

        public static string PartialView(Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);

                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);

                return sw.ToString();
            }
        }

        public static IEnumerable<string> GetStationTypeCollection()
        {
            List<string> stationTypes = new List<string>();

            foreach (StationType stationType in Enum.GetValues(typeof(StationType)))
            {
                stationTypes.Add(stationType.ToString());
            }

            return stationTypes;
        }

        public static IEnumerable<string> GetScanPattern()
        {
            List<string> scanPattern = new List<string>();

            foreach (ScanTypes scanType in Enum.GetValues(typeof(ScanTypes)))
            {
                scanPattern.Add(scanType.ToString());
            }

            return scanPattern;
        }

        public static IEnumerable<string> GetScannerCommunicationChannels()
        {
            List<string> communicationChannels = new List<string>();

            foreach (CommunicationChannel communicationChannel in Enum.GetValues(typeof(CommunicationChannel)))
            {
                communicationChannels.Add(communicationChannel.ToString());
            }

            return communicationChannels;
        }

        public static IEnumerable<string> GetScannerDeviceTypes()
        {
            List<string> deviceTypes = new List<string>();

            foreach (DeviceType deviceType in Enum.GetValues(typeof(DeviceType)))
            {
                deviceTypes.Add(deviceType.ToString());
            }

            return deviceTypes;
        }

        public static MeasurementStationConfigurationEndToEnd ConvertToMeasurementStationConfigurationEndToEnd(StationRegistrationInputs registrationInputs)
        {
            MeasurementStationConfigurationEndToEnd endToEndConfig = new MeasurementStationConfigurationEndToEnd();

            endToEndConfig.AddressLine1 = registrationInputs.Address.AddressLine1;
            endToEndConfig.AddressLine2 = registrationInputs.Address.AddressLine2;
            endToEndConfig.Location = registrationInputs.Address.Location;
            endToEndConfig.Country = registrationInputs.Address.Country;

            endToEndConfig.Name = registrationInputs.StationName;
            endToEndConfig.Description = registrationInputs.Description;

            endToEndConfig.LastModifiedTime = DateTime.UtcNow;
            endToEndConfig.Latitude = registrationInputs.Gps.Latitude;
            endToEndConfig.Longitude = registrationInputs.Gps.Longitude;

            ConvertToRFSensorConfigurationEndToEndCollection(endToEndConfig.RFSensorConfigurations, registrationInputs.RFSensorConfigurationEndToEnd);

            endToEndConfig.RawIqConfiguration = ConvertToRawIqConfiguration(registrationInputs.RawIqDataConfiguration);

            endToEndConfig.AggregationConfiguration = new ClientAggregationConfiguration();
            endToEndConfig.AggregationConfiguration.MinutesOfDataPerScanFile = TimeSpan.FromMinutes(registrationInputs.ClientAggregationConfiguration.MinutesOfDataPerScanFile);
            endToEndConfig.AggregationConfiguration.SecondsOfDataPerSample = TimeSpan.FromSeconds(registrationInputs.ClientAggregationConfiguration.SecondsOfDataPerSample);

            // Following field is default to false always, this field will only be helpful while debugging the Scanner code.
            endToEndConfig.AggregationConfiguration.SingleScan = false;
            endToEndConfig.AggregationConfiguration.OutputData = registrationInputs.ClientAggregationConfiguration.OutputData;

            return endToEndConfig;
        }

        public static RawIqDataConfigurationElement ConvertToRawIqConfiguration(RawIqDataConfigurationElementViewModel rawIqConfigurationViewModel)
        {
            RawIqDataConfigurationElement rawIqConfigurationElement = new RawIqDataConfigurationElement();

            int dutyCycleOnTime = rawIqConfigurationViewModel.DutycycleOnTimeInMilliSec;

            // Ceil factors of 100 if not factors of 100.
            //if (dutyCycleOnTime % 100 != 0)
            //{
            //    dutyCycleOnTime = ((int)(dutyCycleOnTime / 100) + 1) * 100;
            //}

            rawIqConfigurationElement.OutputData = rawIqConfigurationViewModel.OutputData;
            rawIqConfigurationElement.RetentionSeconds = rawIqConfigurationViewModel.RetentionSeconds;
            rawIqConfigurationElement.SecondsOfDataPerFile = rawIqConfigurationViewModel.SecondsOfDataPerFile;
            rawIqConfigurationElement.StartFrequencyHz = MathLibrary.MHzToHz(rawIqConfigurationViewModel.StartFrequencyMHz);
            rawIqConfigurationElement.StopFrequencyHz = MathLibrary.MHzToHz(rawIqConfigurationViewModel.StopFrequencyMHz);
            rawIqConfigurationElement.DutycycleOnTimeInMilliSec = dutyCycleOnTime;
            rawIqConfigurationElement.DutycycleTimeInMilliSec = rawIqConfigurationViewModel.DutycycleTimeInMilliSec;
            rawIqConfigurationElement.OuputPSDDataInDutyCycleOffTime = rawIqConfigurationViewModel.OuputPSDDataInDutyCycleOffTime;

            return rawIqConfigurationElement;
        }

        public static AddressEntity ConvertToAddress(AddressViewModel address)
        {
            return new AddressEntity(address.Location, address.AddressLine1, address.AddressLine2, address.Country);
        }

        public static void ConvertToRFSensorConfigurationEndToEndCollection(List<RFSensorConfigurationEndToEnd> rfsensorConfigurationsEndToEnd, List<RFSensorConfigurationEndToEndViewModel> rfsensorConfigurationViewModel)
        {
            foreach (RFSensorConfigurationEndToEndViewModel rfsensorConfig in rfsensorConfigurationViewModel)
            {
                RFSensorConfigurationEndToEnd rfsensorConfigEndToEnd = new RFSensorConfigurationEndToEnd();
                rfsensorConfigEndToEnd.AntennaPort = rfsensorConfig.AntennaPort;

                foreach (AntennaConfigurationViewModel antenna in rfsensorConfig.Antennas)
                {
                    AntennaConfiguration antennaConfig = new AntennaConfiguration();
                    antennaConfig.AntennaType = antenna.AntennaType;
                    antennaConfig.DegreeDirection = antenna.DegreeDirection;
                    antennaConfig.HeightInFeet = antenna.Height;

                    rfsensorConfigEndToEnd.Antennas.Add(antennaConfig);
                }

                rfsensorConfigEndToEnd.BandwidthHz = rfsensorConfig.BandwidthHz;

                foreach (CableConfigurationViewModel cable in rfsensorConfig.Cables)
                {
                    CableConfiguration cableConfig = new CableConfiguration();
                    cableConfig.CableType = cable.CableType;
                    cableConfig.LengthInFeet = cable.Length;

                    rfsensorConfigEndToEnd.Cables.Add(cableConfig);
                }

                rfsensorConfigEndToEnd.CommunicationsChannel = rfsensorConfig.CommunicationsChannel;

                foreach (ConnectorConfigurationViewModel connector in rfsensorConfig.Connectors)
                {
                    ConnectorConfiguration connectorConfig = new ConnectorConfiguration();
                    connectorConfig.ConnectorType = connector.ConnectorType;

                    rfsensorConfigEndToEnd.Connectors.Add(connectorConfig);
                }

                rfsensorConfigEndToEnd.CurrentStartFrequencyHz = MathLibrary.MHzToHz(rfsensorConfig.CurrentStartFrequencyMHz);
                rfsensorConfigEndToEnd.CurrentStopFrequencyHz = MathLibrary.MHzToHz(rfsensorConfig.CurrentStopFrequencyMHz);
                rfsensorConfigEndToEnd.DescriptiveName = rfsensorConfig.DescriptiveName;
                rfsensorConfigEndToEnd.DeviceAddress = rfsensorConfig.DeviceAddress;
                rfsensorConfigEndToEnd.DeviceType = rfsensorConfig.DeviceType;
                rfsensorConfigEndToEnd.Gain = rfsensorConfig.Gain;
                rfsensorConfigEndToEnd.GpsEnabled = rfsensorConfig.GpsEnabled;
                rfsensorConfigEndToEnd.LockingCommunicationsChannel = rfsensorConfig.LockingCommunicationsChannel;
                rfsensorConfigEndToEnd.MaxPossibleEndFrequencyHz = MathLibrary.MHzToHz(rfsensorConfig.MaxPossibleEndFrequencyMHz);
                rfsensorConfigEndToEnd.MinPossibleStartFrequencyHz = MathLibrary.MHzToHz(rfsensorConfig.MinPossibleStartFrequencyMHz);
                rfsensorConfigEndToEnd.NumberOfSampleBlocksPerScan = rfsensorConfig.NumberOfSampleBlocksPerScan;
                rfsensorConfigEndToEnd.NumberOfSampleBlocksToThrowAway = rfsensorConfig.NumberOfSampleBlocksToThrowAway;
                rfsensorConfigEndToEnd.SamplesPerScan = rfsensorConfig.SamplesPerScan;
                rfsensorConfigEndToEnd.ScanPattern = rfsensorConfig.ScanPattern;
                rfsensorConfigEndToEnd.TuneSleep = rfsensorConfig.TuneSleep;
                rfsensorConfigEndToEnd.AdditionalTuneDelayInMilliSecs = rfsensorConfig.AdditionalTuneDelayInMilliSecs;

                rfsensorConfigurationsEndToEnd.Add(rfsensorConfigEndToEnd);
            }
        }

        public static RFSensorConfigurationEndToEndViewModel GetEmptyRFSensorConfigurationViewModel(bool allowEntryDelete)
        {
            RFSensorConfigurationEndToEndViewModel rfsensorConfig = new RFSensorConfigurationEndToEndViewModel();

            IEnumerable<string> scanPatterns = Utility.GetScanPattern();
            rfsensorConfig.ScanPatterns = new SelectList(scanPatterns);

            IEnumerable<string> deviceTypes = Utility.GetScannerDeviceTypes();
            rfsensorConfig.DeviceTypes = new SelectList(deviceTypes);

            AntennaConfigurationViewModel anntenaConfig = new AntennaConfigurationViewModel() { AllowAntennaEntryDelete = false };
            rfsensorConfig.Antennas = new List<AntennaConfigurationViewModel>() { anntenaConfig };

            CableConfigurationViewModel cableCofig = new CableConfigurationViewModel() { AllowCableEntryDelete = false };
            rfsensorConfig.Cables = new List<CableConfigurationViewModel>() { cableCofig };

            ConnectorConfigurationViewModel connectorConfig = new ConnectorConfigurationViewModel() { AllowConnectorEntryDelete = false };
            rfsensorConfig.Connectors = new List<ConnectorConfigurationViewModel>() { connectorConfig };

            if (string.Compare(rfsensorConfig.DeviceTypes.FirstOrDefault().Text, DeviceType.RFExplorer.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
            {
                rfsensorConfig.CommunicationsChannel = PortalConstants.RFExplorerDefaultCommunicationChannel;
                rfsensorConfig.AntennaPort = PortalConstants.RFExplorerDefaultAntennaPort;
                rfsensorConfig.SamplesPerScan = PortalConstants.RFExplorerSamplesPerScan;
            }
            else if (string.Compare(rfsensorConfig.DeviceTypes.FirstOrDefault().Text, DeviceType.USRP.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
            {
                rfsensorConfig.CommunicationsChannel = PortalConstants.USRPDefaultCommunicationChannel;
                rfsensorConfig.AntennaPort = PortalConstants.USRPDefaultAntennaPort;
                rfsensorConfig.SamplesPerScan = PortalConstants.USRPSamplesPerScan;
            }

            rfsensorConfig.Gain = PortalConstants.RFSensoreDefaultGain;
            rfsensorConfig.TuneSleep = PortalConstants.RFSensoreDefaultTuneSleep;
            rfsensorConfig.AdditionalTuneDelayInMilliSecs = PortalConstants.RFSensoreDefaultTuneSleep;
            rfsensorConfig.BandwidthHz = PortalConstants.RFSensoreDefaultBandwith;
            rfsensorConfig.NumberOfSampleBlocksToThrowAway = PortalConstants.RFSensoreNumberOfSampleBlocksToThrowAway;
            rfsensorConfig.NumberOfSampleBlocksPerScan = PortalConstants.RFSensoreNumberOfSmapleBlocksPerScan;
            rfsensorConfig.AllowRFSensorConfigEntryDelete = allowEntryDelete;

            return rfsensorConfig;
        }

        public static StationContact GetStationContactByProviderUserId(string providerUserId)
        {
            UserManager userManager = PortalGlobalCache.Instance.UserManager;
            User user = userManager.GetUserByProviderUserId(providerUserId);

            return new StationContact
            {
                PrimaryContactName = user.UserName,
                PrimaryContactPhone = user.Phone,
                PrimaryContactEmail = user.AccountEmail,
                PrimaryContactUserId = user.UserId
            };
        }

        public static StationContact GetStationContactByAccountEmail(string accountEmail)
        {
            UserManager userManager = PortalGlobalCache.Instance.UserManager;
            User user = userManager.GetUserByAccountEmail(accountEmail);

            return new StationContact
            {
                PrimaryContactName = user.UserName,
                PrimaryContactPhone = user.Phone,
                PrimaryContactEmail = user.AccountEmail,
                PrimaryContactUserId = user.UserId
            };
        }

        public static StationRegistrationInputs ConvertToStationRegistrationInputs(MeasurementStationInfo measurmentStationInfo, string stationPrimaryContactAccountEmail)
        {
            StationRegistrationInputs stationRegistrationInputs = new StationRegistrationInputs();

            stationRegistrationInputs.StationName = measurmentStationInfo.Identifier.Name;
            stationRegistrationInputs.Description = measurmentStationInfo.StationDescription.Description;
            stationRegistrationInputs.StationType = measurmentStationInfo.DeviceDescription.StationType;
            stationRegistrationInputs.RadioType = measurmentStationInfo.DeviceDescription.RadioType;

            IEnumerable<string> countries = Utility.GetCountries();
            stationRegistrationInputs.Countries = new SelectList(countries, measurmentStationInfo.Address.Country);

            IEnumerable<string> stationTypes = Utility.GetStationTypeCollection();
            stationRegistrationInputs.StationTypes = new SelectList(stationTypes, measurmentStationInfo.DeviceDescription.StationType);

            stationRegistrationInputs.Address = ConvertToAddressViewModel(measurmentStationInfo.Address);

            stationRegistrationInputs.ContactInfo = Utility.GetStationContactByAccountEmail(stationPrimaryContactAccountEmail);

            stationRegistrationInputs.Gps = ConvertToGpsViewModel(measurmentStationInfo.GpsDetails);

            stationRegistrationInputs.ClientAggregationConfiguration = ConvertToClientAggregationConfigViewModel(measurmentStationInfo.DeviceDescription.ClientEndToEndConfiguration.AggregationConfiguration);

            stationRegistrationInputs.RawIqDataConfiguration = ConvertToRawIqConfigurationViewModel(measurmentStationInfo.DeviceDescription.ClientEndToEndConfiguration.RawIqConfiguration);

            stationRegistrationInputs.RFSensorConfigurationEndToEnd = ConvertToRFSensorConfigViewModelCollection(measurmentStationInfo.DeviceDescription.ClientEndToEndConfiguration.RFSensorConfigurations);

            stationRegistrationInputs.ReceiveHealthStatusNotifications = measurmentStationInfo.ReceiveStationNotifications;
            stationRegistrationInputs.ClientHealthStatusCheckIntervalInMin = measurmentStationInfo.ClientHealthStatusCheckIntervalInMin;

            return stationRegistrationInputs;
        }

        public static List<RFSensorConfigurationEndToEndViewModel> ConvertToRFSensorConfigViewModelCollection(List<RFSensorConfigurationEndToEnd> rfsensorConfigCollection)
        {
            List<RFSensorConfigurationEndToEndViewModel> rfsensorConfigCollectionViewModel = new List<RFSensorConfigurationEndToEndViewModel>();

            IEnumerable<string> scanPatterns = Utility.GetScanPattern();
            IEnumerable<string> deviceTypes = Utility.GetScannerDeviceTypes();

            for (int index = 0; index < rfsensorConfigCollection.Count; index++)
            {
                RFSensorConfigurationEndToEndViewModel rfsensorConfigEndToEndViewModel = new RFSensorConfigurationEndToEndViewModel();
                rfsensorConfigEndToEndViewModel.AllowRFSensorConfigEntryDelete = true;

                // Just to disable delete button for first RF SensorConfiguration in the UI.
                if (index == 0)
                {
                    rfsensorConfigEndToEndViewModel.AllowRFSensorConfigEntryDelete = false;
                }

                rfsensorConfigEndToEndViewModel.Antennas = new List<AntennaConfigurationViewModel>();
                rfsensorConfigEndToEndViewModel.Connectors = new List<ConnectorConfigurationViewModel>();
                rfsensorConfigEndToEndViewModel.Cables = new List<CableConfigurationViewModel>();

                rfsensorConfigEndToEndViewModel.AntennaPort = rfsensorConfigCollection[index].AntennaPort;

                for (int i = 0; i < rfsensorConfigCollection[index].Antennas.Count; i++)
                {
                    AntennaConfigurationViewModel antennaConfigViewModel = new AntennaConfigurationViewModel();
                    antennaConfigViewModel.AllowAntennaEntryDelete = true;

                    if (i == 0)
                    {
                        antennaConfigViewModel.AllowAntennaEntryDelete = false;
                    }

                    antennaConfigViewModel.AntennaType = rfsensorConfigCollection[index].Antennas[i].AntennaType;
                    antennaConfigViewModel.DegreeDirection = rfsensorConfigCollection[index].Antennas[i].DegreeDirection;
                    antennaConfigViewModel.Height = rfsensorConfigCollection[index].Antennas[i].HeightInFeet;

                    rfsensorConfigEndToEndViewModel.Antennas.Add(antennaConfigViewModel);
                }

                rfsensorConfigEndToEndViewModel.BandwidthHz = rfsensorConfigCollection[index].BandwidthHz;

                for (int i = 0; i < rfsensorConfigCollection[index].Cables.Count; i++)
                {
                    CableConfigurationViewModel cableConfigViewModel = new CableConfigurationViewModel();
                    cableConfigViewModel.AllowCableEntryDelete = true;

                    // Just to disable delete button for first cable entry in the UI.
                    if (i == 0)
                    {
                        cableConfigViewModel.AllowCableEntryDelete = false;
                    }

                    cableConfigViewModel.CableType = rfsensorConfigCollection[index].Cables[i].CableType;
                    cableConfigViewModel.Length = rfsensorConfigCollection[index].Cables[i].LengthInFeet;

                    rfsensorConfigEndToEndViewModel.Cables.Add(cableConfigViewModel);
                }

                rfsensorConfigEndToEndViewModel.CommunicationsChannel = rfsensorConfigCollection[index].CommunicationsChannel;

                for (int j = 0; j < rfsensorConfigCollection[index].Connectors.Count; j++)
                {
                    ConnectorConfigurationViewModel connectorConfigViewModel = new ConnectorConfigurationViewModel();
                    connectorConfigViewModel.AllowConnectorEntryDelete = true;

                    // Just to disable delete button for first connector entry in the UI.
                    if (j == 0)
                    {
                        connectorConfigViewModel.AllowConnectorEntryDelete = false;
                    }

                    connectorConfigViewModel.ConnectorType = rfsensorConfigCollection[index].Connectors[j].ConnectorType;

                    rfsensorConfigEndToEndViewModel.Connectors.Add(connectorConfigViewModel);
                }

                rfsensorConfigEndToEndViewModel.CurrentStartFrequencyMHz = MathLibrary.HzToMHz(rfsensorConfigCollection[index].CurrentStartFrequencyHz);
                rfsensorConfigEndToEndViewModel.CurrentStopFrequencyMHz = MathLibrary.HzToMHz(rfsensorConfigCollection[index].CurrentStopFrequencyHz);
                rfsensorConfigEndToEndViewModel.DescriptiveName = rfsensorConfigCollection[index].DescriptiveName;
                rfsensorConfigEndToEndViewModel.DeviceAddress = rfsensorConfigCollection[index].DeviceAddress;

                rfsensorConfigEndToEndViewModel.DeviceTypes = new SelectList(deviceTypes, rfsensorConfigCollection[index].DeviceType);
                rfsensorConfigEndToEndViewModel.DeviceType = rfsensorConfigCollection[index].DeviceType;

                rfsensorConfigEndToEndViewModel.Gain = rfsensorConfigCollection[index].Gain;
                rfsensorConfigEndToEndViewModel.GpsEnabled = rfsensorConfigCollection[index].GpsEnabled;
                rfsensorConfigEndToEndViewModel.LockingCommunicationsChannel = rfsensorConfigCollection[index].LockingCommunicationsChannel;
                rfsensorConfigEndToEndViewModel.MaxPossibleEndFrequencyMHz = MathLibrary.HzToMHz(rfsensorConfigCollection[index].MaxPossibleEndFrequencyHz);
                rfsensorConfigEndToEndViewModel.MinPossibleStartFrequencyMHz = MathLibrary.HzToMHz(rfsensorConfigCollection[index].MinPossibleStartFrequencyHz);
                rfsensorConfigEndToEndViewModel.NumberOfSampleBlocksPerScan = rfsensorConfigCollection[index].NumberOfSampleBlocksPerScan;
                rfsensorConfigEndToEndViewModel.NumberOfSampleBlocksToThrowAway = rfsensorConfigCollection[index].NumberOfSampleBlocksToThrowAway;
                rfsensorConfigEndToEndViewModel.SamplesPerScan = rfsensorConfigCollection[index].SamplesPerScan;

                rfsensorConfigEndToEndViewModel.ScanPatterns = new SelectList(scanPatterns, rfsensorConfigCollection[index].ScanPattern);
                rfsensorConfigEndToEndViewModel.ScanPattern = rfsensorConfigCollection[index].ScanPattern;

                rfsensorConfigEndToEndViewModel.TuneSleep = rfsensorConfigCollection[index].TuneSleep;
                rfsensorConfigEndToEndViewModel.AdditionalTuneDelayInMilliSecs = rfsensorConfigCollection[index].AdditionalTuneDelayInMilliSecs;

                rfsensorConfigCollectionViewModel.Add(rfsensorConfigEndToEndViewModel);
            }

            return rfsensorConfigCollectionViewModel;
        }

        public static RawIqDataConfigurationElementViewModel ConvertToRawIqConfigurationViewModel(RawIqDataConfigurationElement rawIqDataConfigurationElement)
        {
            return new RawIqDataConfigurationElementViewModel
            {
                RetentionSeconds = rawIqDataConfigurationElement.RetentionSeconds,
                SecondsOfDataPerFile = rawIqDataConfigurationElement.SecondsOfDataPerFile,
                StartFrequencyMHz = MathLibrary.HzToMHz(rawIqDataConfigurationElement.StartFrequencyHz),
                StopFrequencyMHz = MathLibrary.HzToMHz(rawIqDataConfigurationElement.StopFrequencyHz),
                DutycycleOnTimeInMilliSec = rawIqDataConfigurationElement.DutycycleOnTimeInMilliSec,
                DutycycleTimeInMilliSec = rawIqDataConfigurationElement.DutycycleTimeInMilliSec,
                OutputData = rawIqDataConfigurationElement.OutputData,
                OuputPSDDataInDutyCycleOffTime = rawIqDataConfigurationElement.OuputPSDDataInDutyCycleOffTime
            };
        }

        public static ClientAggregationConfigurationViewModel ConvertToClientAggregationConfigViewModel(ClientAggregationConfiguration clientAggregationConfig)
        {
            return new ClientAggregationConfigurationViewModel
            {
                MinutesOfDataPerScanFile = clientAggregationConfig.MinutesOfDataPerScanFile.TotalMinutes,
                SecondsOfDataPerSample = clientAggregationConfig.SecondsOfDataPerSample.TotalSeconds,
                OutputData = clientAggregationConfig.OutputData
            };
        }

        public static AddressViewModel ConvertToAddressViewModel(AddressEntity address)
        {
            return new AddressViewModel
            {
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                Country = address.Country,
                Location = address.Location
            };
        }

        public static GpsViewModel ConvertToGpsViewModel(GpsEntity gpsDetails)
        {
            return new GpsViewModel
            {
                Latitude = gpsDetails.Latitude,
                Longitude = gpsDetails.Longitude,
                Elevation = gpsDetails.Elevation
            };
        }

        /// <summary>
        /// Compresses the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }

        /// <summary>
        /// Decompresses the string.
        /// </summary>
        /// <param name="compressedText">The compressed text.</param>
        /// <returns></returns>
        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

    }
}