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

namespace Microsoft.Spectrum.Storage
{
    using System;

    public class User
    {
        public User(int userId, string userName, string firstName, string lastName, string location, string region, string timeZoneId, string preferredEmail, string accountEmail, string createdOn, string updatedTime, string link, string gender, string address1, string address2, string phone, string country, string zipCode, string phoneCountryCode,bool subscribeNotifications)
        {
            this.UserId = userId;
            this.UserName = userName;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Location = location;
            this.Region = region;
            this.PreferredEmail = preferredEmail;
            this.AccountEmail = accountEmail;
            this.TimeZoneId = timeZoneId;
            this.CreatedOn = createdOn;
            this.UpdatedTime = updatedTime;
            this.Address1 = address1;
            this.Address2 = address2;
            this.Country = country;
            this.ZipCode = zipCode;
            this.PhoneCountryCode = phoneCountryCode;
            this.Phone = phone;
            this.Link = link;
            this.Gender = gender;
            this.SubscribeNotifications = subscribeNotifications;
        }

        /// <summary>
        /// User id returned after live id login.
        /// </summary>
        public int UserId { get; private set; }

        public string UserName { get; private set; }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public string Location { get; private set; }

        public string Region { get; private set; }

        public string TimeZoneId { get; private set; }

        public string CreatedOn { get; private set; }

        public string PreferredEmail { get; private set; }

        public string AccountEmail { get; private set; }

        public string UpdatedTime { get; private set; }

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
