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
    using System.Linq;
    using System.Web.Mvc;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;
    using Microsoft.Spectrum.Portal.Helpers;
    using Microsoft.Spectrum.Portal.Models;
    using Microsoft.Spectrum.Portal.Security;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Enums;
    using Microsoft.Spectrum.Storage.Models;
    using AddressEntity = Microsoft.Spectrum.Storage.Models.Address;
    using GpsEntity = Microsoft.Spectrum.Storage.Models.GpsDetails;
    using Storage.Table.Azure.DataAccessLayer;

    /// <summary>
    /// Manage Controller
    /// </summary>        
    public class ManageController : Controller
    {
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>ActionResult</returns>
        public ActionResult Index()
        {
            return this.View(this.GetStationsPageWise(0));
        }

        /// <summary>
        /// Manages the user.
        /// </summary>
        /// <returns>PartialViewResult</returns>
        public PartialViewResult ManageUser(string stationId, string stationName)
        {
            this.ViewBag.StationId = stationId;
            this.ViewBag.StationName = stationName;
            return this.PartialView("ManageUserPartial", PortalGlobalCache.Instance.UserManager.GetAllStationAdmins(new System.Guid(stationId)));
        }

        /// <summary>
        /// Adds the administrator.
        /// </summary>
        /// <returns>PartialViewResult</returns>
        public PartialViewResult GetAddAdminView(string stationId)
        {
            AddAdminViewModel model = new AddAdminViewModel
            {
                StationId = stationId
            };

            return this.PartialView("AddAdminPartial", model);
        }

        [HttpPost]
        public PartialViewResult AddAdmin(AddAdminViewModel model, string buttonType)
        {
            if (buttonType == "Add")
            {
                if (ModelState.IsValid)
                {
                    string resultMessage = string.Empty;
                    bool result = PortalGlobalCache.Instance.UserManager.AddUserToStation(model.Email, new System.Guid(model.StationId), out resultMessage);

                    if (!result)
                    {
                        model.ErrorMessage = resultMessage;
                        return this.PartialView("AddAdminPartial", model);
                    }
                }
                else
                {
                    return this.PartialView("AddAdminPartial", model);
                }
            }

            this.ViewBag.StationId = model.StationId;
            this.ViewBag.StationName = PortalGlobalCache.Instance.MeasurementStationTableOperation.GetMeasurementStationInfoPublic(new System.Guid(model.StationId)).StationDescription.StationName;
            return this.PartialView("ManageUserPartial", PortalGlobalCache.Instance.UserManager.GetAllStationAdmins(new System.Guid(model.StationId)));
        }

        public PartialViewResult DeleteAdmin(string stationId, int userId)
        {
            PortalGlobalCache.Instance.UserManagementTableOperations.RemoveAdmin(userId, new System.Guid(stationId));

            return this.ManageUser(stationId, PortalGlobalCache.Instance.MeasurementStationTableOperation.GetMeasurementStationInfoPublic(new System.Guid(stationId)).StationDescription.StationName);
        }

        public PartialViewResult DecomissionStation(string stationId, int pageIndex)
        {
            PortalGlobalCache.Instance.MeasurementStationTableOperation.UpdateStationAvailabilityStatus(new System.Guid(stationId), Storage.Enums.StationAvailability.Decommissioned);

            return this.PartialView("ManageSpectrumObservatoriesPartial", this.GetStationsPageWise(pageIndex));
        }

        public PartialViewResult BringStationOnline(string stationId, int pageIndex)
        {
            PortalGlobalCache.Instance.MeasurementStationTableOperation.UpdateStationAvailabilityStatus(new System.Guid(stationId), Storage.Enums.StationAvailability.Online);

            return this.PartialView("ManageSpectrumObservatoriesPartial", this.GetStationsPageWise(pageIndex));
        }

        public PartialViewResult Next(int pageIndex)
        {
            return this.PartialView("ManageSpectrumObservatoriesPartial", this.GetStationsPageWise(pageIndex + 1));
        }

        public PartialViewResult Previous(int pageIndex)
        {
            return this.PartialView("ManageSpectrumObservatoriesPartial", this.GetStationsPageWise(pageIndex - 1));
        }

        /// <summary>
        /// Backs to manage.
        /// </summary>
        /// <returns>PartialViewResult</returns>
        public PartialViewResult BackToManage()
        {
            return this.PartialView("ManageSpectrumObservatoriesPartial", this.GetStationsPageWise(0));
        }

        [HttpGet]
        public PartialViewResult EditStationDetails(Guid measurementStationId)
        {
            IMeasurementStationTableOperations measurementStationOperations = PortalGlobalCache.Instance.MeasurementStationTableOperation;
            MeasurementStationInfo stationInfo = measurementStationOperations.GetMeasurementStationInfoPrivate(measurementStationId);

            this.ViewData["measurementStationId"] = measurementStationId;
            StationRegistrationInputs registrationDetails = Utility.ConvertToStationRegistrationInputs(stationInfo, stationInfo.PrivateData.PrimaryContactEmail);

            return this.PartialView("EditSpectrumObservatoryDetailsPartial", registrationDetails);
        }

        [HttpPost]
        public ActionResult EditStationDetails(StationRegistrationInputs inputs, Guid measurementStationId)
        {
            if (!this.ModelState.IsValid)
            {
                IEnumerable<string> communicationChannels = Utility.GetScannerCommunicationChannels();
                IEnumerable<string> scanPatterns = Utility.GetScanPattern();
                IEnumerable<string> deviceTypes = Utility.GetScannerDeviceTypes();
                IEnumerable<string> countries = Utility.GetCountries();
                IEnumerable<string> stationTypes = Utility.GetStationTypeCollection();

                inputs.Countries = new SelectList(countries);
                inputs.StationTypes = new SelectList(stationTypes);

                this.ViewData["measurementStationId"] = measurementStationId;

                if (inputs.RFSensorConfigurationEndToEnd == null)
                {
                    inputs.RFSensorConfigurationEndToEnd = new List<RFSensorConfigurationEndToEndViewModel>();
                }

                foreach (RFSensorConfigurationEndToEndViewModel sensorConfig in inputs.RFSensorConfigurationEndToEnd)
                {
                    sensorConfig.ScanPatterns = new SelectList(scanPatterns);
                    sensorConfig.DeviceTypes = new SelectList(deviceTypes);
                }

                string jsonViewData = Utility.PartialView(this, "EditSpectrumObservatoryDetailsPartial", inputs);

                return this.Json(new { ViewData = jsonViewData, ServerValidationErrors = true, ErrorMessage = string.Empty });
            }

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

            IMeasurementStationTableOperations measurementStationOperations = PortalGlobalCache.Instance.MeasurementStationTableOperation;
            MeasurementStationInfo outdatedStationDetails = measurementStationOperations.GetMeasurementStationInfoPublic(measurementStationId);

            try
            {
                MeasurementStationInfo currentStationDetails = ConvertToMeasurementStationInfo(inputs, measurementStationId, outdatedStationDetails.Identifier.StorageAccountName);

                measurementStationOperations.UpdateStationData(currentStationDetails);

                IEnumerable<MeasurementStationInfo> measurementStations = this.GetStationsPageWise(0);
                string jsonViewData = Utility.PartialView(this, "ManageSpectrumObservatoriesPartial", measurementStations);

                return this.Json(new { ViewData = jsonViewData, ServerValidationErrors = false, ErrorMessage = string.Empty });
            }
            catch (Exception ex)
            {
                PortalGlobalCache.Instance.Logger.Log(TraceEventType.Error, LoggingMessageId.PortalUnhandledError, ex.ToString());

                return this.Json(new { ViewData = string.Empty, ServerValidationErrors = false, ErrorMessage = "CityScape Spectrum Observatory update operation failed: Please contact Site Administrator for more information" });
            }
        }

        private static MeasurementStationInfo ConvertToMeasurementStationInfo(StationRegistrationInputs inputs, Guid measurementStationId, string storageAccountName)
        {
            AddressEntity address = Utility.ConvertToAddress(inputs.Address);
            MeasurementStationConfigurationEndToEnd mesurementStationConfigEndToEnd = Utility.ConvertToMeasurementStationConfigurationEndToEnd(inputs);

            long startFrequency = (long)mesurementStationConfigEndToEnd.RFSensorConfigurations.Min(sensor => sensor.CurrentStartFrequencyHz);
            long stopFrequency = (long)mesurementStationConfigEndToEnd.RFSensorConfigurations.Max(sensor => sensor.CurrentStopFrequencyHz);

            DeviceDescription deviceScription = new DeviceDescription(
                            inputs.RFSensorConfigurationEndToEnd.FirstOrDefault().Antennas.First().AntennaType,
                            inputs.RadioType,
                            inputs.StationType,
                            startFrequency,
                            stopFrequency,
                            string.Empty,
                            mesurementStationConfigEndToEnd);

            GpsEntity gpsDetails = new GpsEntity(inputs.Gps.Latitude, inputs.Gps.Longitude, inputs.Gps.Elevation);

            MeasurementStationDescription stationDescription = new MeasurementStationDescription(inputs.StationName, inputs.Description);

            MeasurementStationIdentifier stationIdentifier = new MeasurementStationIdentifier(
                 measurementStationId,
                 inputs.StationName,
                 storageAccountName,
                 DateTime.UtcNow);

            MeasurementStationInfo measurementStationInfo = new MeasurementStationInfo(
                      address,
                      deviceScription,
                      gpsDetails,
                      stationIdentifier,
                      stationDescription,
                      null,
                      StationAvailability.Online,
                      inputs.ReceiveHealthStatusNotifications,
                      inputs.ClientHealthStatusCheckIntervalInMin);

            return measurementStationInfo;
        }

        private IEnumerable<MeasurementStationInfo> GetMeasurementStationsForCurrentUser()
        {
            List<MeasurementStationInfo> measurementStations = new List<MeasurementStationInfo>();

            UserPrincipal userPrincipal = (UserPrincipal)this.User;
            if (userPrincipal.Role == Storage.Enums.UserRoles.SiteAdmin)
            {
                return PortalGlobalCache.Instance.MeasurementStationTableOperation.GetAllMeasurementStationInfoPrivate();
            }
            else if (userPrincipal.Role == Storage.Enums.UserRoles.StationAdmin)
            {
                User user = PortalGlobalCache.Instance.UserManager.GetUserByProviderUserId(userPrincipal.ProviderUserId);
                string[] accessableStationIds = PortalGlobalCache.Instance.UserManager.GetAcessableStationIds(user.UserId);

                if (accessableStationIds.Length > 0)
                {
                    foreach (string stationId in accessableStationIds)
                    {
                        measurementStations.Add(PortalGlobalCache.Instance.MeasurementStationTableOperation.GetMeasurementStationInfoPrivate(new System.Guid(stationId)));
                    }
                }
            }

            return measurementStations;
        }

        private IEnumerable<MeasurementStationInfo> GetStationsPageWise(int pageIndex)
        {
            IEnumerable<MeasurementStationInfo> stations = this.GetMeasurementStationsForCurrentUser();

            this.ViewBag.TotalCount = stations.Count();
            this.ViewBag.PageIndex = pageIndex;

            return this.GetMeasurementStationsForCurrentUser().Skip(pageIndex * Constants.PageSize).Take(Constants.PageSize);
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