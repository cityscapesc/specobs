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

    public class Address
    {
        public Address(string location, string addressLine1, string addressLine2, string country)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentException("Location can not be empty", "location");
            }

            if (string.IsNullOrWhiteSpace(addressLine1))
            {
                throw new ArgumentException("Address can not be empty", "addressLine1");
            }            

            if (string.IsNullOrWhiteSpace(country))
            {
                throw new ArgumentException("Country can not be empty", "country");
            }

            this.Location = location;
            this.AddressLine1 = addressLine1;
            this.AddressLine2 = addressLine2;
            this.Country = country;
        }

        public string Location { get; private set; }

        public string AddressLine1 { get; private set; }

        public string AddressLine2 { get; set; }

        public string Country { get; private set; }
    }
}
