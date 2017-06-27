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

namespace Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Enums;
    using Microsoft.Spectrum.Storage.Models;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using ProtoBuf;
    using WindowsAzure.Storage.Table;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage.Table.Azure
    /// Class:          MeasurementStationTableOperations
    /// Description:    class containing operations to deal with user related tables
    /// ----------------------------------------------------------------- 
    public class MeasurementStationTableOperations : IMeasurementStationTableOperations
    {
        private readonly RetryAzureTableOperations<MeasurementStationsPrivate> measurementStationPrivateTableOperations;

        private readonly RetryAzureTableOperations<MeasurementStationsPublic> measurementStationPublicTableOperations;

        private readonly RetryAzureTableOperations<MeasurementStationsPublicHistorical> measurementStationHistorialTableOperations;

        private readonly IUserManagementTableOperations userManagementTableOperations;

        private readonly RetryAzureTableOperations<RawIQScanPolicy> rawIQScanPolicyTableOperations;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementStationTableOperations"/> class
        /// </summary>
        /// <param name="dataContext">data context containing table references</param>
        public MeasurementStationTableOperations(AzureTableDbContext dataContext)
        {
            if (dataContext == null)
            {
                throw new ArgumentNullException("dataContext", "The azure table db context can not be null");
            }

            this.measurementStationPrivateTableOperations = dataContext.MeasurementStationPrivateOperations;
            this.measurementStationPrivateTableOperations.GetTableReference(AzureTableHelper.MeasurementStationsPrivateTable);

            this.measurementStationPublicTableOperations = dataContext.PublicMeasurementStationsOperations;
            this.measurementStationPublicTableOperations.GetTableReference(AzureTableHelper.MeasurementStationsPublicTable);

            this.measurementStationHistorialTableOperations = dataContext.PublicMeasurementStationHistoricalDataOperations;
            this.measurementStationHistorialTableOperations.GetTableReference(AzureTableHelper.MeasurementStationsPublicHistoricalTable);

            this.rawIQScanPolicyTableOperations = dataContext.RawIQScanPolicyTableOperations;
            this.rawIQScanPolicyTableOperations.GetTableReference(AzureTableHelper.RawIQScanPolicyTable);

            this.userManagementTableOperations = new UserManagementTableOperations(dataContext);
        }

        public IEnumerable<MeasurementStationInfo> GetAllMeasurementStationInfoPublic()
        {
            List<MeasurementStationInfo> returnStations = new List<MeasurementStationInfo>();

            IEnumerable<MeasurementStationsPublic> stations = this.measurementStationPublicTableOperations.GetByKeys<MeasurementStationsPublic>(Constants.DummyPartitionKey);

            foreach (MeasurementStationsPublic station in stations)
            {
                returnStations.Add(MeasurementStationTableOperations.GetMeasurementStationInfoPublic(station));
            }

            return returnStations;
        }

        public IEnumerable<MeasurementStationInfo> GetAllMeasurementStationInfoPrivate()
        {
            List<MeasurementStationInfo> returnStations = new List<MeasurementStationInfo>();

            IEnumerable<MeasurementStationsPrivate> stations = this.measurementStationPrivateTableOperations.GetByKeys<MeasurementStationsPrivate>(Constants.DummyPartitionKey);

            foreach (MeasurementStationsPrivate station in stations)
            {
                returnStations.Add(MeasurementStationTableOperations.GetMeasurementStationInfoPrivate(this.GetMeasurementStationPublic(station.Id), station));
            }

            return returnStations;
        }

        public MeasurementStationInfo GetMeasurementStationInfoPublic(Guid measurementStationId)
        {
            MeasurementStationInfo measurementStationInfo = MeasurementStationTableOperations.GetMeasurementStationInfoPublic(this.GetMeasurementStationPublic(measurementStationId));

            return measurementStationInfo;
        }

        public MeasurementStationInfo GetMeasurementStationInfoPrivate(Guid measurementStationId)
        {
            return MeasurementStationTableOperations.GetMeasurementStationInfoPrivate(this.GetMeasurementStationPublic(measurementStationId), this.GetMeasurementStationPrivate(measurementStationId));
        }

        public void AddMeasurementStation(MeasurementStationInfo station)
        {
            if (station == null)
            {
                throw new ArgumentException("station can not be null", "station");
            }

            if (station.PrivateData == null)
            {
                throw new ArgumentNullException("station.PrivateData", "To add a station, we need private data as well as public data.");
            }

            MeasurementStationsPublic publicStation;

            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize<MeasurementStationConfigurationEndToEnd>(stream, station.DeviceDescription.ClientEndToEndConfiguration);

                publicStation = new MeasurementStationsPublic(station.Identifier.Id)
                {
                    Name = station.Identifier.Name,
                    Latitude = station.GpsDetails.Latitude,
                    Longitude = station.GpsDetails.Longitude,
                    Elevation = station.GpsDetails.Elevation,
                    Description = station.StationDescription.Description,
                    Location = station.Address.Location,
                    RadioType = station.DeviceDescription.RadioType,
                    Antenna = station.DeviceDescription.Antenna,
                    StartFrequency = station.DeviceDescription.StartFrequency,
                    StopFrequency = station.DeviceDescription.StopFrequency,
                    AddressLine1 = station.Address.AddressLine1,
                    AddressLine2 = station.Address.AddressLine2,
                    Country = station.Address.Country,
                    SpectrumDataStorageAccountName = station.Identifier.StorageAccountName,
                    StationType = station.DeviceDescription.StationType,
                    HardwareInformation = station.DeviceDescription.HardwareInformation,
                    ClientEndToEndConfiguration = stream.ToArray(),
                    StationAvailability = station.StationAvailability,
                    ReceiveStationNotifications = station.ReceiveStationNotifications
                };
            }

            MeasurementStationsPrivate privateStation = new MeasurementStationsPrivate(station.Identifier.Id)
            {
                PrimaryContactName = station.PrivateData.PrimaryContactName,
                PrimaryContactPhone = station.PrivateData.PrimaryContactEmail,
                PrimaryContactEmail = station.PrivateData.PrimaryContactEmail,
                PrimaryContactUserId = station.PrivateData.PrimaryContactUserId,
            };

            // insert into both the public and private tables
            this.measurementStationPublicTableOperations.InsertOrReplaceEntity(publicStation, true);
            this.measurementStationPrivateTableOperations.InsertOrReplaceEntity(privateStation, true);

            // Elevating the current user role to Station Administrator for the station he/she has registered.
            UserRole userRole = new UserRole(privateStation.PrimaryContactUserId, UserRoles.StationAdmin.ToString(), station.Identifier.Id.ToString());
            this.userManagementTableOperations.InsertOrUpdate(userRole);
        }

        public void UpdateStationData(MeasurementStationInfo latestMeasurementStationInfo)
        {
            if (latestMeasurementStationInfo == null)
            {
                throw new ArgumentNullException("latestMeasurementStationInfo", "latestMeasurementStationInfo can not be null");
            }

            MeasurementStationsPublicHistorical stationHistoricalDataEntity = MeasurementStationTableOperations.GetStationHistoricalDataEntity(latestMeasurementStationInfo);
            this.measurementStationHistorialTableOperations.InsertOrMergeEntity(stationHistoricalDataEntity, true);

            // Updating Public Measurement Station table.
            MeasurementStationsPublic measurementStationPublicEntity = MeasurementStationTableOperations.ConvertToMeasurementStationPublic(latestMeasurementStationInfo);
            this.measurementStationPublicTableOperations.InsertOrMergeEntity(measurementStationPublicEntity, true);

            // Updating Private Measurement Station table.
            if (latestMeasurementStationInfo.PrivateData != null)
            {
                MeasurementStationsPrivate measurementStationPrivateEntity = MeasurementStationTableOperations.ConvertToMeasurementStationPrivate(latestMeasurementStationInfo);
                this.measurementStationPrivateTableOperations.InsertOrMergeEntity(measurementStationPrivateEntity, true);
            }
        }

        public void UpdateStationHardwareInformation(Guid measurementStationId, string hardwareInformation)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "measurmentStationId parameter can not be null");
            }

            string measurementStationIdentifier = measurementStationId.ToString();

            MeasurementStationsPublic measurementStation = this.measurementStationPublicTableOperations.GetByKeys<MeasurementStationsPublic>(Constants.DummyPartitionKey, measurementStationIdentifier).SingleOrDefault();

            if (measurementStation == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Measurement Station id {0} doesn't exist in the system", measurementStationId.ToString()), "measurementStationId");
            }

            measurementStation.HardwareInformation = hardwareInformation;

            this.measurementStationPublicTableOperations.InsertOrReplaceEntity(measurementStation, true);
        }

        public void UpdateStationAvailabilityStatus(Guid measurementStationId, StationAvailability availabilityStatus)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "measurmentStationId parameter can not be null");
            }

            string measurementStationIdentifier = measurementStationId.ToString();

            MeasurementStationsPublic measurementStation = this.measurementStationPublicTableOperations.GetByKeys<MeasurementStationsPublic>(Constants.DummyPartitionKey, measurementStationIdentifier).SingleOrDefault();

            if (measurementStation == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Measurement Station id {0} doesn't exist in the system", measurementStationId.ToString()), "measurementStationId");
            }

            measurementStation.StationAvailability = (int)availabilityStatus;

            this.measurementStationPublicTableOperations.InsertOrReplaceEntity(measurementStation, true);
        }

        public void UpdatePSDDataAvailabilityTimestamp(Guid measurementStationId, DateTime startTime, DateTime endTime)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "measurmentStationId parameter can not be null");
            }

            string measurementStationIdentifier = measurementStationId.ToString();

            MeasurementStationsPublic measurementStation = this.measurementStationPublicTableOperations.GetByKeys<MeasurementStationsPublic>(Constants.DummyPartitionKey, measurementStationIdentifier).SingleOrDefault();

            if (measurementStation == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Measurement Station id {0} doesn't exist in the system", measurementStationId.ToString()), "measurementStationId");
            }

            measurementStation.PSDDataAvailabilityStartDate = startTime;
            measurementStation.PSDDataAvailabilityEndDate = endTime;

            this.measurementStationPublicTableOperations.InsertOrReplaceEntity(measurementStation, true);
        }

        public void UpdateRawIQDataAvailabilityTimestamp(Guid measurementStationId, DateTime startTime, DateTime endTime)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "measurmentStationId parameter can not be null");
            }

            string measurementStationIdentifier = measurementStationId.ToString();

            MeasurementStationsPublic measurementStation = this.measurementStationPublicTableOperations.GetByKeys<MeasurementStationsPublic>(Constants.DummyPartitionKey, measurementStationIdentifier).SingleOrDefault();

            if (measurementStation == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Measurement Station id {0} doesn't exist in the system", measurementStationId.ToString()), "measurementStationId");
            }

            measurementStation.RawIQDataAvailabilityStartDate = startTime;
            measurementStation.RawIQDataAvailabilityEndDate = endTime;

            this.measurementStationPublicTableOperations.InsertOrReplaceEntity(measurementStation, true);
        }

        public IEnumerable<RawIQPolicy> GetOverlappingIQBands(long rawIQStartFrequency, long rawIQStopFrequency)
        {
            if (rawIQStartFrequency >= rawIQStopFrequency)
            {
                throw new ArgumentOutOfRangeException("RawIQStart frequency should be less than RawIQStop frequency");
            }

            string startFrequencyText = rawIQStartFrequency.ToString(Constants.ElevenDigit).PadLeft(11, '0');
            string stopFrequencyText = rawIQStopFrequency.ToString(Constants.ElevenDigit).PadLeft(11, '0');

            string startIntersectionPartitionQuery = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, startFrequencyText);
            string startIntersectionRowQuery = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, startFrequencyText);

            string intersectionOverMiddleBandsPartitionQuery = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, startFrequencyText);
            string intersectionOverMiddleBandsRowQuery = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, stopFrequencyText);

            string stopIntersectionPartitionQuery = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, stopFrequencyText);
            string stopIntersectionRowQuery = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, stopFrequencyText);

            string condition1 = TableQuery.CombineFilters(startIntersectionPartitionQuery, TableOperators.And, startIntersectionRowQuery);
            string condition2 = TableQuery.CombineFilters(intersectionOverMiddleBandsPartitionQuery, TableOperators.And, intersectionOverMiddleBandsRowQuery);
            string condition3 = TableQuery.CombineFilters(stopIntersectionPartitionQuery, TableOperators.And, stopIntersectionRowQuery);

            string finalQuery = TableQuery.CombineFilters(condition1, TableOperators.Or, condition2);
            finalQuery = TableQuery.CombineFilters(finalQuery, TableOperators.Or, condition3);

            IEnumerable<RawIQScanPolicy> intersectingBandsPolicy = this.rawIQScanPolicyTableOperations.ExecuteQueryWithContinuation<RawIQScanPolicy>(finalQuery);
            List<RawIQPolicy> intersectionRawIQScanPolicies = new List<RawIQPolicy>();

            if (intersectingBandsPolicy != null
                && intersectingBandsPolicy.Any())
            {
                foreach (RawIQScanPolicy rawIQScanPolicy in intersectingBandsPolicy)
                {
                    RawIQPolicy rawIQPolicy = new RawIQPolicy(
                        rawIQScanPolicy.Category,
                        rawIQScanPolicy.BandPriority,
                        rawIQScanPolicy.StartFrequency,
                        rawIQScanPolicy.StopFrequency,
                        rawIQScanPolicy.DutycycleOnTimeInMilliSec,
                        rawIQScanPolicy.DutycycleTimeInMilliSec,
                        rawIQScanPolicy.FileDurationInSec,
                        rawIQScanPolicy.RetentionTimeInSec,
                        rawIQScanPolicy.PolicyDetails
                        );
                    intersectionRawIQScanPolicies.Add(rawIQPolicy);
                }
            }
            else
            {
                string defaultPartitionQuery = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Constants.DefaultRawIQFrequencyPartition);
                string defaultRowQuery = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, Constants.DefaultRawIQCategoryRow);

                finalQuery = TableQuery.CombineFilters(defaultPartitionQuery, TableOperators.And, defaultRowQuery);
                IEnumerable<RawIQScanPolicy> defaultBands = this.rawIQScanPolicyTableOperations.ExecuteQueryWithContinuation<RawIQScanPolicy>(finalQuery);

                if (defaultBands != null
                    && defaultBands.Any())
                {
                    RawIQScanPolicy defaultBand = defaultBands.FirstOrDefault();

                    RawIQPolicy rawIQPolicy = new RawIQPolicy(
                       defaultBand.Category,
                       defaultBand.BandPriority,
                       defaultBand.StartFrequency,
                       defaultBand.StopFrequency,
                       defaultBand.DutycycleOnTimeInMilliSec,
                       defaultBand.DutycycleTimeInMilliSec,
                       defaultBand.FileDurationInSec,
                       defaultBand.RetentionTimeInSec,
                       defaultBand.PolicyDetails
                       );

                    intersectionRawIQScanPolicies.Add(rawIQPolicy);
                }
            }

            return intersectionRawIQScanPolicies;
        }

        public double GetFFTBinWidth(Guid measurementStationId, DateTime startTime, DateTime endTime)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "measurmentStationId parameter can not be null");
            }

            double fftBinwidth = 0;

            string measurementStationIdentifier = measurementStationId.ToString();

            string partitionQuery = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, measurementStationIdentifier);
            string onOrBeforeStartTimeRowQuery = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, startTime.Ticks.ToString("d19").PadLeft(19, '0'));
            string settingsOnOrBeforeStartTime = TableQuery.CombineFilters(partitionQuery, TableOperators.And, onOrBeforeStartTimeRowQuery);

            MeasurementStationsPublicHistorical settingBefore = this.measurementStationHistorialTableOperations.ExecuteQueryWithContinuation<MeasurementStationsPublicHistorical>(settingsOnOrBeforeStartTime).LastOrDefault();

            string afterStartTimeRowQuery = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, startTime.Ticks.ToString("d19").PadLeft(19, '0'));
            string beforeEndTimeRowQuery = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, endTime.Ticks.ToString("d19").PadLeft(19, '0'));
            string settingsAfterStartTimeBeforeEndTime = TableQuery.CombineFilters(afterStartTimeRowQuery, TableOperators.And, beforeEndTimeRowQuery);

            string finalQuery = TableQuery.CombineFilters(partitionQuery, TableOperators.And, settingsAfterStartTimeBeforeEndTime);

            MeasurementStationsPublicHistorical settingsAfter = this.measurementStationHistorialTableOperations.ExecuteQueryWithContinuation<MeasurementStationsPublicHistorical>(finalQuery).LastOrDefault();

            if (settingsAfter != null)
            {
                fftBinwidth = this.GetFFTBinWidth(settingsAfter.ClientEndToEndConfiguration);
            }
            else if (settingBefore != null)
            {
                fftBinwidth = this.GetFFTBinWidth(settingBefore.ClientEndToEndConfiguration);
            }

            return fftBinwidth;
        }

        private double GetFFTBinWidth(byte[] clientEndToEndConfiguration)
        {
            double fftBinwidth = 0;

            using (MemoryStream stream = new MemoryStream(clientEndToEndConfiguration))
            {
                MeasurementStationConfigurationEndToEnd measurementStationConfigurationEndToEnd = Serializer.Deserialize<MeasurementStationConfigurationEndToEnd>(stream);
                var rfSensorConfig = measurementStationConfigurationEndToEnd.RFSensorConfigurations.FirstOrDefault();

                if (rfSensorConfig != null)
                {
                    fftBinwidth = rfSensorConfig.BandwidthHz / rfSensorConfig.SamplesPerScan;
                }
            }

            return fftBinwidth;
        }

        private static MeasurementStationsPublicHistorical GetStationHistoricalDataEntity(MeasurementStationInfo measurementStation)
        {
            MeasurementStationsPublicHistorical historicalDataEntity;

            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize<MeasurementStationConfigurationEndToEnd>(stream, measurementStation.DeviceDescription.ClientEndToEndConfiguration);

                historicalDataEntity = new MeasurementStationsPublicHistorical(measurementStation.Identifier.Id, DateTime.UtcNow)
                {
                    AddressLine1 = measurementStation.Address.AddressLine1,
                    AddressLine2 = measurementStation.Address.AddressLine2,
                    Antenna = measurementStation.DeviceDescription.Antenna,
                    Country = measurementStation.Address.Country,
                    Description = measurementStation.StationDescription.Description,
                    Elevation = measurementStation.GpsDetails.Elevation,
                    Latitude = measurementStation.GpsDetails.Latitude,
                    Location = measurementStation.Address.Location,
                    Longitude = measurementStation.GpsDetails.Longitude,
                    Name = measurementStation.Identifier.Name,
                    RadioType = measurementStation.DeviceDescription.RadioType,
                    SpectrumDataStorageAccountName = measurementStation.Identifier.StorageAccountName,
                    StartFrequency = measurementStation.DeviceDescription.StartFrequency,
                    StationType = measurementStation.DeviceDescription.StationType,
                    StopFrequency = measurementStation.DeviceDescription.StopFrequency,
                    ClientDeviceConfiguration = measurementStation.DeviceDescription.HardwareInformation,
                    ClientEndToEndConfiguration = stream.ToArray()
                };
            }

            return historicalDataEntity;
        }

        private static MeasurementStationsPublic ConvertToMeasurementStationPublic(MeasurementStationInfo measurementStation)
        {
            MeasurementStationsPublic measurementStationEntity;

            using (MemoryStream stream = new MemoryStream())
            {
                Serializer.Serialize<MeasurementStationConfigurationEndToEnd>(stream, measurementStation.DeviceDescription.ClientEndToEndConfiguration);

                measurementStationEntity = new MeasurementStationsPublic(measurementStation.Identifier.Id)
                {
                    AddressLine1 = measurementStation.Address.AddressLine1,
                    AddressLine2 = measurementStation.Address.AddressLine2,
                    Antenna = measurementStation.DeviceDescription.Antenna,
                    Country = measurementStation.Address.Country,
                    Description = measurementStation.StationDescription.Description,
                    Elevation = measurementStation.GpsDetails.Elevation,
                    Latitude = measurementStation.GpsDetails.Latitude,
                    Location = measurementStation.Address.Location,
                    Longitude = measurementStation.GpsDetails.Longitude,
                    Name = measurementStation.Identifier.Name,
                    RadioType = measurementStation.DeviceDescription.RadioType,
                    SpectrumDataStorageAccountName = measurementStation.Identifier.StorageAccountName,
                    StartFrequency = measurementStation.DeviceDescription.StartFrequency,
                    StationType = measurementStation.DeviceDescription.StationType,
                    StopFrequency = measurementStation.DeviceDescription.StopFrequency,
                    HardwareInformation = measurementStation.DeviceDescription.HardwareInformation,
                    ClientEndToEndConfiguration = stream.ToArray(),
                    ReceiveStationNotifications = measurementStation.ReceiveStationNotifications,
                    ClientHealthStatusCheckIntervalInMin = measurementStation.ClientHealthStatusCheckIntervalInMin,
                    PSDDataAvailabilityEndDate = measurementStation.PSDDataAvailabilityEndDate,
                    RawIQDataAvailabilityEndDate = measurementStation.RawIQDataAvailabilityEndDate,
                    PSDDataAvailabilityStartDate = measurementStation.PSDDataAvailabilityStartDate,
                    RawIQDataAvailabilityStartDate = measurementStation.RawIQDataAvailabilityStartDate
                };
            }

            return measurementStationEntity;
        }

        /// <summary>
        /// GetMeasurementStationPrivateEntity returns the private data associated with the measurement station. This data is only accessible to 
        /// the operators of the portal.
        /// </summary>
        /// <param name="measurementStation"></param>
        /// <returns>MeasurementStationsPrivate</returns>
        private static MeasurementStationsPrivate ConvertToMeasurementStationPrivate(MeasurementStationInfo measurementStation)
        {
            if (measurementStation != null)
            {
                if (measurementStation.PrivateData != null)
                {
                    MeasurementStationsPrivate measurementStationPrivateEntity = new MeasurementStationsPrivate(measurementStation.Identifier.Id)
                    {
                        PrimaryContactName = measurementStation.PrivateData.PrimaryContactName,
                        PrimaryContactPhone = measurementStation.PrivateData.PrimaryContactPhone,
                        PrimaryContactEmail = measurementStation.PrivateData.PrimaryContactEmail,
                        PrimaryContactUserId = measurementStation.PrivateData.PrimaryContactUserId
                    };

                    return measurementStationPrivateEntity;
                }
            }

            return null;
        }

        private static MeasurementStationInfo GetMeasurementStationInfoPublic(MeasurementStationsPublic measurementStationPublic)
        {
            MeasurementStationIdentifier stationIdentifier = new MeasurementStationIdentifier(measurementStationPublic.Id, measurementStationPublic.Name, measurementStationPublic.SpectrumDataStorageAccountName, measurementStationPublic.Timestamp.DateTime);
            MeasurementStationDescription stationDescription = new MeasurementStationDescription(measurementStationPublic.Name, measurementStationPublic.Description);

            Address stationAddress = new Address(measurementStationPublic.Location, measurementStationPublic.AddressLine1, measurementStationPublic.AddressLine2, measurementStationPublic.Country)
            {
                AddressLine2 = measurementStationPublic.AddressLine2
            };

            DeviceDescription deviceDescription;

            using (MemoryStream stream = new MemoryStream(measurementStationPublic.ClientEndToEndConfiguration))
            {
                MeasurementStationConfigurationEndToEnd measurementStationConfigurationEndToEnd = Serializer.Deserialize<MeasurementStationConfigurationEndToEnd>(stream);
                deviceDescription = new DeviceDescription(measurementStationPublic.Antenna, measurementStationPublic.RadioType, measurementStationPublic.StationType, measurementStationPublic.StartFrequency, measurementStationPublic.StopFrequency, measurementStationPublic.HardwareInformation, measurementStationConfigurationEndToEnd);
            }

            GpsDetails gpsDetails = new GpsDetails(measurementStationPublic.Latitude, measurementStationPublic.Longitude, measurementStationPublic.Elevation);

            var stationInfo = new MeasurementStationInfo(stationAddress, deviceDescription, gpsDetails, stationIdentifier, stationDescription, null, (Enums.StationAvailability)measurementStationPublic.StationAvailability, measurementStationPublic.ReceiveStationNotifications, measurementStationPublic.ClientHealthStatusCheckIntervalInMin);

            stationInfo.PSDDataAvailabilityStartDate = measurementStationPublic.PSDDataAvailabilityStartDate;
            stationInfo.PSDDataAvailabilityEndDate = measurementStationPublic.PSDDataAvailabilityEndDate;
            stationInfo.RawIQDataAvailabilityStartDate = measurementStationPublic.RawIQDataAvailabilityStartDate;
            stationInfo.RawIQDataAvailabilityEndDate = measurementStationPublic.RawIQDataAvailabilityEndDate;

            return stationInfo;
        }

        private static MeasurementStationInfo GetMeasurementStationInfoPrivate(MeasurementStationsPublic measurementStationPublic, MeasurementStationsPrivate measurementStationPrivate)
        {
            if (measurementStationPublic.Id != measurementStationPrivate.Id)
            {
                throw new ArgumentException("measurement station public and private id's don't match", "measurementStationPublic");
            }

            MeasurementStationInfo info = MeasurementStationTableOperations.GetMeasurementStationInfoPublic(measurementStationPublic);

            // add in the private information here, this is the difference between the public and private interfaces
            info.PrivateData = new MeasurementStationPrivateData(measurementStationPrivate.PrimaryContactName, measurementStationPrivate.PrimaryContactPhone, measurementStationPrivate.PrimaryContactEmail, measurementStationPrivate.PrimaryContactUserId);

            return info;
        }

        private MeasurementStationsPublic GetMeasurementStationPublic(Guid measurementStationId)
        {
            if (measurementStationId != null)
            {
                string rowKey = measurementStationId.ToString();
                MeasurementStationsPublic measurementStation = this.measurementStationPublicTableOperations.GetByKeys<MeasurementStationsPublic>(Constants.DummyPartitionKey, rowKey).SingleOrDefault();

                return measurementStation;
            }

            return null;
        }

        private MeasurementStationsPrivate GetMeasurementStationPrivate(Guid measurementStationId)
        {
            if (measurementStationId != null)
            {
                string rowKey = measurementStationId.ToString();
                MeasurementStationsPrivate measurementStation = this.measurementStationPrivateTableOperations.GetByKeys<MeasurementStationsPrivate>(Constants.DummyPartitionKey, rowKey).SingleOrDefault();

                return measurementStation;
            }

            return null;
        }
    }
}
