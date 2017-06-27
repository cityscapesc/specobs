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
    using Microsoft.Spectrum.IO.MeasurementStationSettings;
    using Microsoft.Spectrum.Storage.Enums;

    public class DeviceDescription
    {
        public DeviceDescription(string antenna, string radioType, string stationType, long startFrequency, long stopFrequency, string hardwareInformation, MeasurementStationConfigurationEndToEnd clientEndToEndConfiguration)
        {
            if (string.IsNullOrWhiteSpace(antenna))
            {
                throw new ArgumentException("Antenna can not be empty", "antenna");
            }

            if (string.IsNullOrWhiteSpace(radioType))
            {
                throw new ArgumentException("Radio Type can not be empty", "radioType");
            }

            if (string.IsNullOrWhiteSpace(stationType))
            {
                throw new ArgumentException("Station Type can not be empty", "stationType");
            }

            // TODO: Uncomment the following validation once the basic testing is done.
            ////if (string.IsNullOrWhiteSpace(xmlConfiguration))
            ////{
            ////    throw new ArgumentException("xmlConfiguration");
            ////}

            this.Antenna = antenna;
            this.RadioType = radioType;
            this.StationType = stationType;
            this.StartFrequency = startFrequency;
            this.StopFrequency = stopFrequency;
            this.HardwareInformation = hardwareInformation;
            this.ClientEndToEndConfiguration = clientEndToEndConfiguration;   
        }

        public string Antenna { get; private set; }

        public string RadioType { get; private set; }

        // TODO: Convert this field into an enum.
        public string StationType { get; private set; }

        public long StartFrequency { get; private set; }

        public long StopFrequency { get; private set; }

        public string HardwareInformation { get; set; }

        public MeasurementStationConfigurationEndToEnd ClientEndToEndConfiguration { get; set; }
    }
}
