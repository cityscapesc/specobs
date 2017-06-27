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
    using System.Globalization;    
    using Microsoft.Spectrum.Common;
    using Microsoft.WindowsAzure.Storage.Table;
    
    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      MS.Storage.AzureTables
    /// Class:          MeasurementStationsPublic (Entity Type:Public)
    /// Description:    MeasurementStationsPublic Entity Class
    /// ----------------------------------------------------------------- 
    /// </summary>
     public class MeasurementStationsPublicHistorical : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementStationsPublic"/> class
        /// </summary>
        public MeasurementStationsPublicHistorical()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementStationsPublic"/> class
        /// </summary>
        /// <param name="accessId">Access Id</param>
        public MeasurementStationsPublicHistorical(Guid id, DateTime updateTime)
        {
            this.RowKey = updateTime.ToUniversalTime().Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture); 
            this.PartitionKey = id.ToString(); 
        }

        /// <summary> 
        /// Gets the AccessId of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public Guid Id
        {
            get
            {
                return Guid.Parse(this.PartitionKey);
            }
        }

        /// <summary> 
        /// Gets the Timestamp of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public DateTime UpdateTime
        {
            get
            {
                return new DateTime(long.Parse(this.RowKey, CultureInfo.InvariantCulture));
            }
        }

        /// <summary> 
        /// Gets or sets the Name of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public string Name { get; set; }

        /// <summary> 
        /// Gets or sets the Latitude of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public double Latitude { get; set; }

        /// <summary> 
        /// Gets or sets the Longitude of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public double Longitude { get; set; }

        /// <summary> 
        /// Gets or sets the Elevation of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public double Elevation { get; set; }

        /// <summary> 
        /// Gets or sets the Description of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public string Description { get; set; }

        /// <summary> 
        /// Gets or sets the Location of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public string Location { get; set; }

        /// <summary> 
        /// Gets or sets the RadioType of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public string RadioType { get; set; }

        /// <summary> 
        /// Gets or sets the Antenna of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public string Antenna { get; set; }

        /// <summary> 
        /// Gets or sets the StartFrequency of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public long StartFrequency { get; set; }

        /// <summary> 
        /// Gets or sets the StopFrequency of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public long StopFrequency { get; set; }

        /// <summary> 
        /// Gets or sets the AddressLine1 of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public string AddressLine1 { get; set; }

        /// <summary> 
        /// Gets or sets the AddressLine2 of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public string AddressLine2 { get; set; }

        /// <summary> 
        /// Gets or sets the Country of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public string Country { get; set; }

        /// <summary> 
        /// Gets or sets the SpectrumDataStorageAccountName of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public string SpectrumDataStorageAccountName { get; set; }

        /// <summary> 
        /// Gets or sets the StationType of the MeasurementStationsPublicHistorical. 
        /// </summary>
        public string StationType { get; set; }

        /// <summary> 
        /// Gets or sets the ClientDeviceConfiguration of the MeasurementStationsPublic. This includes the settings for the rf sensors.
        /// </summary>
        public string ClientDeviceConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the ClientEndToEndConfiguation of the MeasurementStationsPublic. This includes the antenna setup, cabling, connectors, and RF sensors.
        /// </summary>
        public byte[] ClientEndToEndConfiguration { get; set; }
    }
}
