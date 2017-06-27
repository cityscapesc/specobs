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
    /// Class:          MeasurementStationsPrivate (Entity Type:Private)
    /// Description:    MeasurementStationsPrivate Entity Class
    /// ----------------------------------------------------------------- 
    /// </summary>
    public class MeasurementStationsPrivate : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementStationsPrivate"/> class
        /// </summary>
        public MeasurementStationsPrivate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementStationsPrivate"/> class
        /// </summary>
        /// <param name="accessId">Access Id</param>
        public MeasurementStationsPrivate(Guid id)
        {
            this.PartitionKey = Constants.DummyPartitionKey;
            this.RowKey = id.ToString();
        }

        /// <summary> 
        /// Gets the AccessId of the MeasurementStationsPrivate. 
        /// </summary> 
        public Guid Id
        {
            get
            {
                return Guid.Parse(this.RowKey);
            }
        }

        /// <summary> 
        /// Gets or sets the PrimaryContactName of the MeasurementStationsPrivate. 
        /// </summary> 
        public string PrimaryContactName { get; set; }

        /// <summary> 
        /// Gets or sets the PrimaryContactPhone of the MeasurementStationsPrivate. 
        /// </summary> 
        public string PrimaryContactPhone { get; set; }

        /// <summary> 
        /// Gets or sets the PrimaryContactEmail of the MeasurementStationsPrivate. 
        /// </summary> 
        public string PrimaryContactEmail { get; set; }

        /// <summary> 
        /// Gets or sets the PrimaryContactUserId of the MeasurementStationsPrivate. 
        /// </summary> 
        public int PrimaryContactUserId { get; set; }
    }
}
