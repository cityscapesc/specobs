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

    public class GpsDetails
    {
        public GpsDetails(double latitude, double longitude, double elevation)
        {
            if (latitude < -90 || latitude > 90)
            {
                throw new ArgumentOutOfRangeException("latitude");
            }

            if (longitude < -180 || longitude > 180)
            {
                throw new ArgumentOutOfRangeException("longitude");
            }

            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Elevation = elevation;
        }

        public double Latitude { get; private set; }

        public double Longitude { get; private set; }

        public double Elevation { get; private set; }
    }
}
