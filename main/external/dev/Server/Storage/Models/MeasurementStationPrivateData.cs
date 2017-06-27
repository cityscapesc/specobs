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

namespace Microsoft.Spectrum.Storage.Models
{
    using System;

    public class MeasurementStationPrivateData
    {
        public MeasurementStationPrivateData(string primaryContactName, string primaryContactPhone, string primaryContactEmail, int primaryContactUserId)
        {
            if (string.IsNullOrWhiteSpace(primaryContactName))
            {
                throw new ArgumentException("primary contact name must not be blank", "primaryContactName");
            }

            if (string.IsNullOrWhiteSpace(primaryContactPhone))
            {
                throw new ArgumentException("primary contact phone must not be blank", "primaryContactPhone");
            }

            if (string.IsNullOrWhiteSpace(primaryContactEmail))
            {
                throw new ArgumentException("primary contact email must not be blank", "primaryContactEmail");
            }

            this.PrimaryContactName = primaryContactName;
            this.PrimaryContactPhone = primaryContactPhone;
            this.PrimaryContactEmail = primaryContactEmail;
            this.PrimaryContactUserId = primaryContactUserId;            
        }

        public string PrimaryContactName { get; private set; }

        public string PrimaryContactPhone { get; private set; }

        public string PrimaryContactEmail { get; private set; }

        public int PrimaryContactUserId { get; private set; }
    }
}
