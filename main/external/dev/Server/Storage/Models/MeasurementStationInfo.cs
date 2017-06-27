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

namespace Microsoft.Spectrum.Storage.Models
{
    using System;
    using System.Globalization;
    using Microsoft.Spectrum.Storage.Enums;

    public class MeasurementStationInfo
    {
        public MeasurementStationInfo(Address address, DeviceDescription deviceDescription, GpsDetails gpsDetails, MeasurementStationIdentifier measurementStationIdentifier, MeasurementStationDescription measurementStationDescription, MeasurementStationPrivateData measurementStationPrivateData, StationAvailability stationAvailability, bool receiveStationNotifications = true, int clientHealthStatusCheckIntervalInMin = 10)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address", "address can not be null");
            }

            if (deviceDescription == null)
            {
                throw new ArgumentNullException("deviceDescription", "deviceDescription can not be null");
            }

            if (gpsDetails == null)
            {
                throw new ArgumentNullException("gpsDetails", "gpsDetails can not be null");
            }

            if (measurementStationIdentifier == null)
            {
                throw new ArgumentNullException("measurementStationIdentifier", "measurementStationIdentifier can not be null");
            }

            if (measurementStationDescription == null)
            {
                throw new ArgumentNullException("measurementStationDescription", "measurementStationDescription can not be null");
            }

            this.Address = address;

            this.DeviceDescription = deviceDescription;
            this.GpsDetails = gpsDetails;
            this.Identifier = measurementStationIdentifier;
            this.PrivateData = measurementStationPrivateData;
            this.StationAvailability = Convert.ToInt16(stationAvailability, CultureInfo.InvariantCulture);
            this.StationDescription = measurementStationDescription;
            this.ReceiveStationNotifications = receiveStationNotifications;
            this.ClientHealthStatusCheckIntervalInMin = clientHealthStatusCheckIntervalInMin;
        }

        public Address Address { get; set; }

        public DeviceDescription DeviceDescription { get; set; }

        public GpsDetails GpsDetails { get; set; }

        public MeasurementStationIdentifier Identifier { get; set; }

        public MeasurementStationDescription StationDescription { get; set; }

        public MeasurementStationPrivateData PrivateData { get; set; }

        public int StationAvailability { get; set; }

        public DateTime? PSDDataAvailabilityStartDate { get; set; }

        public DateTime? PSDDataAvailabilityEndDate { get; set; }

        public DateTime? RawIQDataAvailabilityStartDate { get; set; }

        public DateTime? RawIQDataAvailabilityEndDate { get; set; }

        public bool ReceiveStationNotifications { get; set; }

        public int ClientHealthStatusCheckIntervalInMin { get; set; }
    }
}
