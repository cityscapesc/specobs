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
    /// Class:          SpectrumFileProcessingFailures (Entity Type:Private)
    /// Description:    SpectrumFileProcessingFailures Entity Class
    /// ----------------------------------------------------------------- 
    /// </summary>
    public class SpectrumFileProcessingFailures : TableEntity
    {
        /// <summary>
        /// Initialized a new instance of the <see cref="SpectrumFileProcessingFailures"/> class
        /// </summary>
        public SpectrumFileProcessingFailures()
        {
        }

        /// <summary>
        /// Initialized a new instance of the <see cref="SpectrumFileProcessingFailures"/> class
        /// </summary>
        /// <param name="measurementStationId">Measurement Station Id.</param>
        /// <param name="timeOfFailure">Time of failure.</param>
        public SpectrumFileProcessingFailures(Guid measurementStationId, DateTime timeOfFailure)
        {
            string failureTime = timeOfFailure.ToUniversalTime().Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);
            this.RowKey = failureTime;
            this.PartitionKey = measurementStationId.ToString();
        }

        /// <summary>
        /// Gets Measurement station id.
        /// </summary>
        public Guid MeasurementStationId
        {
            get
            {
                return Guid.Parse(this.PartitionKey);
            }
        }

        /// <summary>
        /// Gets time of failure.
        /// </summary>
        public DateTime TimeOfFailure
        {
            get
            {
                return new DateTime(long.Parse(this.RowKey, CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Gets or sets Spectrum file blob Uri which is failed to process.
        /// </summary>
        public string AbsoluteFilePath { get; set; }       

        /// <summary>
        /// Gets or sets detailed error description.
        /// </summary>
        public string Error { get; set; }
    }
}
