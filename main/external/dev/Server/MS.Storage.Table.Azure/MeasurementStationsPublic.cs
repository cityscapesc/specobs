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

namespace Microsoft.Spectrum.Storage.Table.Azure
{
    using System;
    using Microsoft.Spectrum.Common;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      MS.Storage.AzureTables
    /// Class:          MeasurementStationsPublic (Entity Type:Public)
    /// Description:    MeasurementStationsPublic Entity Class
    /// ----------------------------------------------------------------- 
    /// </summary>
    public class MeasurementStationsPublic : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementStationsPublic"/> class
        /// </summary>
        public MeasurementStationsPublic()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementStationsPublic"/> class
        /// </summary>
        /// <param name="accessId">Access Id</param>
        public MeasurementStationsPublic(Guid id)
        {
            this.RowKey = id.ToString();
            this.PartitionKey = Constants.DummyPartitionKey;
        }

        /// <summary> 
        /// Gets the AccessId of the MeasurementStationsPublic. 
        /// </summary>
        public Guid Id
        {
            get
            {
                return Guid.Parse(this.RowKey);
            }
        }

        /// <summary> 
        /// Gets or sets the Name of the MeasurementStationsPublic. 
        /// </summary>
        public string Name { get; set; }

        /// <summary> 
        /// Gets or sets the Latitude of the MeasurementStationsPublic. 
        /// </summary>
        public double Latitude { get; set; }

        /// <summary> 
        /// Gets or sets the Longitude of the MeasurementStationsPublic. 
        /// </summary>
        public double Longitude { get; set; }

        /// <summary> 
        /// Gets or sets the Elevation of the MeasurementStationsPublic. 
        /// </summary>
        public double Elevation { get; set; }

        /// <summary> 
        /// Gets or sets the Description of the MeasurementStationsPublic. 
        /// </summary>
        public string Description { get; set; }

        /// <summary> 
        /// Gets or sets the Location of the MeasurementStationsPublic. 
        /// </summary>
        public string Location { get; set; }

        /// <summary> 
        /// Gets or sets the RadioType of the MeasurementStationsPublic. 
        /// </summary>
        public string RadioType { get; set; }

        /// <summary> 
        /// Gets or sets the Antenna of the MeasurementStationsPublic. 
        /// </summary>
        public string Antenna { get; set; }

        /// <summary> 
        /// Gets or sets the StartFrequency of the MeasurementStationsPublic. 
        /// </summary>
        public long StartFrequency { get; set; }

        /// <summary> 
        /// Gets or sets the StopFrequency of the MeasurementStationsPublic. 
        /// </summary>
        public long StopFrequency { get; set; }

        /// <summary> 
        /// Gets or sets the AddressLine1 of the MeasurementStationsPublic. 
        /// </summary>
        public string AddressLine1 { get; set; }

        /// <summary> 
        /// Gets or sets the AddressLine2 of the MeasurementStationsPublic. 
        /// </summary>
        public string AddressLine2 { get; set; }

        /// <summary> 
        /// Gets or sets the Country of the MeasurementStationsPublic. 
        /// </summary>
        public string Country { get; set; }

        /// <summary> 
        /// Gets or sets the SpectrumDataStorageAccountId of the MeasurementStationsPublic. 
        /// </summary>
        public string SpectrumDataStorageAccountName { get; set; }

        /// <summary> 
        /// Gets or sets the StationType of the MeasurementStationsPublic. 
        /// </summary>
        public string StationType { get; set; }

        /// <summary> 
        /// Gets or sets the ClientDeviceConfiguration of the MeasurementStationsPublic. This includes the settings for the rf sensors.
        /// </summary>
        public string HardwareInformation { get; set; }

        /// <summary>
        /// Gets or set the very first PSD process file timestamp.
        /// </summary>
        public DateTime? PSDDataAvailabilityStartDate { get; set; }

        /// <summary>
        /// Gets or set the very first RawIQ process file timestamp.
        /// </summary>
        public DateTime? RawIQDataAvailabilityStartDate { get; set; }

        /// <summary>
        /// Gets or sets latest PSD file processed timestamp.
        /// </summary>
        public DateTime? PSDDataAvailabilityEndDate { get; set; }

        /// <summary>
        /// Gets or sets latest RawIQ file processed timestamp.
        /// </summary>
        public DateTime? RawIQDataAvailabilityEndDate { get; set; }

        /// <summary>
        /// Gets or sets the ClientEndToEndConfiguation of the MeasurementStationsPublic. This includes the antenna setup, cabling, connectors, and RF sensors.
        /// </summary>
        public byte[] ClientEndToEndConfiguration { get; set; }

        /// <summary>
        /// Gets or sets StationAvailability for a MeasurementStationsPublic
        /// </summary>
        public int StationAvailability { get; set; }

        /// <summary>
        /// Gets or sets ReceiveStationNotifications for a MeasurementStationsPublic
        /// </summary>
        public bool ReceiveStationNotifications { get; set; }

        /// <summary>
        /// Gets or sets Health Status check interval for the remote machine connected to USRP device.
        /// </summary>
        public int ClientHealthStatusCheckIntervalInMin { get; set; }
    }
}
