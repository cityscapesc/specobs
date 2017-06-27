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
    using Microsoft.WindowsAzure.Storage.Table;    

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      MS.Storage.AzureTables
    /// Class:          ChartRequests (Entity Type:Private)
    /// Description:    ChartRequests Entity Class
    /// ----------------------------------------------------------------- 
    /// </summary>
    public class ChartRequests : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartRequests"/> class
        /// </summary>
        public ChartRequests()
        {
        }

       /// <summary>
        /// Initializes a new instance of the <see cref="ChartRequests"/> class
       /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="id">Id</param>
        public ChartRequests(Guid userId, Guid id)
        {
            this.PartitionKey = userId.ToString();
            this.RowKey = id.ToString();
        }

        /// <summary> 
        /// Gets the UserId of the ChartRequests. 
        /// </summary>
        public Guid UserId
        {
            get
            {
                return Guid.Parse(this.PartitionKey);
            }
        }

        /// <summary> 
        /// Gets the Id of the ChartRequests. 
        /// </summary>
        public Guid Id
        {
            get
            {
                return Guid.Parse(this.RowKey);
            }
        }

        /// <summary> 
        /// Gets or sets the Name of the ChartRequests. 
        /// </summary>
        public string Name { get; set; }

        /// <summary> 
        /// Gets or sets the MeasurementStationId of the ChartRequests. 
        /// </summary>
        public Guid MeasurementStationId { get; set; }

        /// <summary> 
        /// Gets or sets the ChartCreationDate of the ChartRequests. 
        /// </summary>
        public DateTime ChartCreationDate { get; set; }

        /// <summary> 
        /// Gets or sets the StartFrequency of the ChartRequests. 
        /// </summary>
        public double StartFrequency { get; set; }

        /// <summary> 
        /// Gets or sets the EndFrequency of the ChartRequests. 
        /// </summary>
        public double EndFrequency { get; set; }

        /// <summary> 
        /// Gets or sets the TimeScale of the ChartRequests. 
        /// </summary>
        public string Timescale { get; set; }

        /// <summary> 
        /// Gets or sets the UpdateTime of the ChartRequests. 
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary> 
        /// Gets or sets the ChartType of the ChartRequests. 
        /// </summary>
        public string ChartType { get; set; }
    }
}
