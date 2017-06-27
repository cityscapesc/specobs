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
    /// Class:          RawSpectralDataSchema2a (Entity Type:Public)
    /// Description:    RawSpectralDataSchema2a Entity Class
    /// -----------------------------------------------------------------  
    /// </summary>
    public class RawSpectralDataSchema : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawSpectralDataSchema"/> class
        /// </summary>
        public RawSpectralDataSchema()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawSpectralDataSchema"/> class
        /// </summary>
        /// <param name="timeStart"> Time Start </param>
        /// <param name="frequency">Spectral Data Frequency</param>
        public RawSpectralDataSchema(Guid measurementStationId, DateTime timeStart)
        {
            string startTime = timeStart.ToUniversalTime().Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);
            this.RowKey = startTime;
            this.PartitionKey = measurementStationId.ToString();            
        }

        /// <summary> 
        /// Gets the TimeStart of the RawSpectralDataSchema2a. 
        /// </summary>
        public DateTime TimeStart
        {
            get
            {
                return new DateTime(long.Parse(this.RowKey, CultureInfo.InvariantCulture));
            }
        }

        /// <summary> 
        /// Gets and sets the CompressionType of the RawSpectralDataSchema. 
        /// </summary>
        public int CompressionType { get; set; }

        /// <summary> 
        /// Gets and sets the TypeId of the RawSpectralDataSchema. 
        /// </summary>
        public int TypeId { get; set; }

        /// <summary> 
        /// Gets the BlobUri of the RawSpectralDataSchema. 
        /// </summary>
        public string BlobUri { get; set; }

        public double StartFrequency { get; set; }

        public double EndFrequency { get; set; }
    }
}
