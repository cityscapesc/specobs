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

    public class SpectralDataSchema : TableEntity
    {
        public SpectralDataSchema()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectralDataSchema"/> class
        /// </summary>
        /// <param name="timeStart">Time Start</param>
        /// <param name="frequency">Spectral Data Frequency</param>
        public SpectralDataSchema(DateTime timeStart, long frequency)
        {
            string dayTimeStart = timeStart.ToUniversalTime().Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);
            this.RowKey = frequency.ToString(Constants.ElevenDigit, CultureInfo.InvariantCulture);
            this.PartitionKey = dayTimeStart;
        }

        /// <summary> 
        /// Gets the TimeStart of the SpectralDataSchema. 
        /// </summary>
        public DateTime TimeStart
        {
            get
            {
                return new DateTime(long.Parse(this.PartitionKey, CultureInfo.InvariantCulture));
            }
        }

        /// <summary> 
        /// Gets the Frequency of the SpectralDataSchema. 
        /// </summary>
        public long Frequency
        {
            get
            {
                return long.Parse(this.RowKey, CultureInfo.InvariantCulture);
            }
        }

        /// <summary> 
        /// Gets or sets the AveragePower
        /// </summary>
        public double AveragePower { get; set; }

        /// <summary>
        /// Gets or sets the average of the minimum power
        /// </summary>
        public double AverageOfMinimumPower { get; set; }

        /// <summary>
        /// Gets or sets the average of the maximum power
        /// </summary>
        public double AverageOfMaximumPower { get; set; }

        /// <summary> 
        /// Gets or sets the MaxPower of the TableSchema2aBase. 
        /// </summary>
        public double MaxPower { get; set; }

        /// <summary> 
        /// Gets or sets the MinPower of the TableSchema2aBase. 
        /// </summary>
        public double MinPower { get; set; }

        /// <summary> 
        /// Gets or sets the SampleCount of the TableSchema2aBase. 
        /// </summary>
        public long SampleCount { get; set; }

        /// <summary>
        /// Gets or sets the standard deviation of the average
        /// </summary>
        public double StandardDeviationOfAveragePower { get; set; }

        /// <summary>
        /// Gets or sets the standard deviation of the minimum
        /// </summary>
        public double StandardDeviationOfMinimumPower { get; set; }

        /// <summary>
        /// Gets or sets the standard deviation of the maximum
        /// </summary>
        public double StandardDeviationOfMaximumPower { get; set; }
    }
}
