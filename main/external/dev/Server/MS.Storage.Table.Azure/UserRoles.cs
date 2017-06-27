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
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      MS.Storage.AzureTables
    /// Class:          UserRoles (Entity Type:Private)
    /// Description:    UserRoles Entity Class
    /// ----------------------------------------------------------------- 
    /// </summary>
    public class UserRoles : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserRoles"/> class
        /// </summary>
        public UserRoles()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRoles"/> class
        /// </summary>
        /// <param name="userId"> user Id </param>
        public UserRoles(string measurementStationId, int userId)
        {
            this.RowKey = userId.ToString(CultureInfo.InvariantCulture);
            this.PartitionKey = measurementStationId.ToString();
        }

        /// <summary> 
        /// Gets the UserId of the UserRoles. 
        /// </summary>
        public int UserId
        {
            get
            {
                return int.Parse(this.RowKey, CultureInfo.InvariantCulture);
            }
        }

        /// <summary> 
        /// Gets or sets the MeasurementStationId of the UserRoles. 
        /// </summary>
        public string MeasurementStationId
        {
            get
            {
                return this.PartitionKey;
            }
        }

        /// <summary> 
        /// Gets or sets the Role of the UserRoles. 
        /// </summary>
        public string Role { get; set; }
    }
}
