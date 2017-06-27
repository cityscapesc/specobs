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

namespace Microsoft.Spectrum.Storage.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;
    using Microsoft.Spectrum.Storage.Models;

    public class MeasurementStationManager
    {
        private readonly IMeasurementStationTableOperations stationTableOperations;
        private readonly ISpectrumDataStorageAccountsTableOperations storageAccountsTableOperations;
        private readonly ILogger logger;

        public MeasurementStationManager(IMeasurementStationTableOperations measurementStationTableOperations, ISpectrumDataStorageAccountsTableOperations spectrumDataStorageAccountsTableOperations, ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (spectrumDataStorageAccountsTableOperations == null)
            {
                throw new ArgumentNullException("spectrumDataStorageAccountsTableOperations");
            }

            this.stationTableOperations = measurementStationTableOperations;
            this.storageAccountsTableOperations = spectrumDataStorageAccountsTableOperations;
            this.logger = logger;
        }

        public MeasurementStationInfo GetMeasurementStationPublic(Guid measurementStationId)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId");
            }

            try
            {
                MeasurementStationInfo measurementStationInfo = this.stationTableOperations.GetMeasurementStationInfoPublic(measurementStationId);

                return measurementStationInfo;
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        public void UpdateStationData(MeasurementStationInfo latestMeasurementStationInfo)
        {
            if (latestMeasurementStationInfo == null)
            {
                throw new ArgumentNullException("latestMeasurementStationInfo");
            }

            try
            {
                this.stationTableOperations.UpdateStationData(latestMeasurementStationInfo);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        public void UpdatePSDDataAvailabilityTimestamp(Guid measurementStationId, DateTime startTime, DateTime endTime)
        {
            try
            {
                this.stationTableOperations.UpdatePSDDataAvailabilityTimestamp(measurementStationId, startTime, endTime);
            }
            catch(Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        public void UpdateRawIQDataAvailabilityTimestamp(Guid measurementStationId, DateTime startTime, DateTime endTime)
        {
            try
            {
                this.stationTableOperations.UpdateRawIQDataAvailabilityTimestamp(measurementStationId, startTime, endTime);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        public void UpdateStationHardwareInformation(Guid measurementStationId, string hardwareInformation)
        {
            if (hardwareInformation == null)
            {
                throw new ArgumentNullException("hardwareInformation");
            }

            try
            {
                this.stationTableOperations.UpdateStationHardwareInformation(measurementStationId, hardwareInformation);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        public Guid CreateNewStation(
            string location,
            string addressLine1,
            string addressLine2,
            string country,
            string antenna,
            string radioType,
            string stationType,
            long startFrequency,
            long stopFrequency,
            MeasurementStationConfigurationEndToEnd measurementStationEndToEndConfiguration,
            double latitude,
            double longitude,
            double elevation,
            string stationName,
            string description,
            string primaryContactName,
            string primaryContactPhone,
            string primaryContactEmail,
            int primaryContactUserId,
            bool receiveHealthStatusNotifications,
            int clientHealthStatusCheckIntervalInMin)
        {
            try
            {
                // Assign the measurement station to one of the storage accounts
                string storageAccountName = this.storageAccountsTableOperations.AddStationToAccount();

                // Create a new ID for the measurement station and make sure to update it in the end to end configuration
                Guid measurementStationId = Guid.NewGuid();
                measurementStationEndToEndConfiguration.MeasurementStationId = measurementStationId.ToString();

                // Add the station to the measurement stations table with a new ID, the correct storage account name and mark it as upcoming
                MeasurementStationInfo measurementStationInfo = new MeasurementStationInfo(
                        new Address(
                            location,
                            addressLine1,
                            addressLine2,
                            country),
                        new DeviceDescription(
                            antenna,
                            radioType,
                            stationType,
                            startFrequency,
                            stopFrequency,
                            string.Empty,
                            measurementStationEndToEndConfiguration),
                        new GpsDetails(
                            latitude,
                            longitude,
                            elevation),
                        new MeasurementStationIdentifier(
                            measurementStationId,
                            stationName,
                            storageAccountName,
                            DateTime.UtcNow),
                        new MeasurementStationDescription(
                            stationName,
                            description),
                        new MeasurementStationPrivateData(
                            primaryContactName,
                            primaryContactPhone,
                            primaryContactEmail,
                            primaryContactUserId),
                        Enums.StationAvailability.Upcoming,
                        receiveHealthStatusNotifications,
                        clientHealthStatusCheckIntervalInMin);

                this.stationTableOperations.AddMeasurementStation(measurementStationInfo);

                // On successful station registration, return the MeasurementStation Id with which registration is completed.
                return measurementStationId;
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.StationRegistrationFailure, ex.ToString());
                throw;
            }
        }
    }
}
