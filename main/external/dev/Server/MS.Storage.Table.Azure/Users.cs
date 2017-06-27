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
    /// Class:          Users (Entity Type:Private)
    /// Description:    Users Entity Class
    /// -----------------------------------------------------------------
    /// </summary>
    public class Users : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Users"/> class
        /// </summary>
        public Users()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Users"/> class
        /// </summary>
        /// <param name="id"> id </param>
        public Users(int id)
        {
            this.RowKey = id.ToString(CultureInfo.InvariantCulture);
            this.PartitionKey = Constants.DummyPartitionKey;
        }

        /// <summary> 
        /// Gets the Id of the Users. 
        /// </summary>
        public int Id
        {
            get
            {
                string[] rowkey = this.RowKey.Split(':');
                return int.Parse(rowkey[1], CultureInfo.InvariantCulture);
            }
        }

        /// <summary> 
        /// Gets or sets the UserName of the Users. 
        /// </summary>
        public string UserName { get; set; }

        /// <summary> 
        /// Gets or sets the FirstName of the Users. 
        /// </summary>
        public string FirstName { get; set; }

        /// <summary> 
        /// Gets or sets the LastName of the Users. 
        /// </summary>
        public string LastName { get; set; }
       
        /// <summary> 
        /// Gets or sets the Location of the Users. 
        /// </summary>
        public string Location { get; set; }

        /// <summary> 
        /// Gets or sets the Region of the Users. 
        /// </summary>
        public string Region { get; set; }

        /// <summary> 
        /// Gets or sets the TimeZoneId of the Users. 
        /// </summary>
        public string TimeZoneId { get; set; }

        /// <summary> 
        /// Gets or sets the PreferredEmail of the Users. 
        /// </summary>
        public string PreferredEmail { get; set; }

        /// <summary> 
        /// Gets or sets the AccountEmail of the Users. 
        /// </summary>
        public string AccountEmail { get; set; }

        /// <summary> 
        /// Gets or sets the CreatedOn of the Users. 
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary> 
        /// Gets or sets the UpdatedTime of the Users. 
        /// </summary>
        public DateTime UpdatedTime { get; set; }

        /// <summary> 
        /// Gets or sets the Link of the Users. 
        /// </summary>
        public string Link { get; set; }

        /// <summary> 
        /// Gets or sets the Gender of the Users. 
        /// </summary>
        public string Gender { get; set; }

        /// <summary> 
        /// Gets or sets the TimeZone of the Users. 
        /// </summary>
        public string TimeZone { get; set; }

        /// <summary> 
        /// Gets or sets the Address1 of the Users. 
        /// </summary>
        public string Address1 { get; set; }

        /// <summary> 
        /// Gets or sets the Address2 of the Users. 
        /// </summary>
        public string Address2 { get; set; }

        /// <summary> 
        /// Gets or sets the Phone of the Users. 
        /// </summary>
        public string Phone { get; set; }

        /// <summary> 
        /// Gets or sets the Country of the Users. 
        /// </summary>
        public string Country { get; set; }

        /// <summary> 
        /// Gets or sets the ZipCode of the Users. 
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary> 
        /// Gets or sets the PhoneCountryCode of the Users. 
        /// </summary>
        public string PhoneCountryCode { get; set; }

        /// <summary>
        /// Gets or sets SubscribeNotifications.
        /// </summary>
        public bool SubscribeNotifications { get; set; }
    }
}
