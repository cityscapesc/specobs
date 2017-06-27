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

namespace Microsoft.Spectrum.IO.ScanFile
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;  
    using ProtoBuf;      

    [DebuggerDisplay("ConfigDataBlock: {TimeStamp}")]
    [ProtoContract]
    public class ConfigDataBlock : DataBlock
    {
        public ConfigDataBlock(string hardwareInformation, MeasurementStationConfigurationEndToEnd endToEndConfiguration)
        {
            this.HardwareInformation = hardwareInformation;
            this.EndToEndConfiguration = endToEndConfiguration;
            this.Timestamp = DateTime.UtcNow;
        }

        internal ConfigDataBlock()
        {
        }

        [ProtoMember(1)]
        public override DateTime Timestamp { get; set; }

        [ProtoMember(2)]
        public string HardwareInformation { get; set; }

        [ProtoMember(3)]
        public MeasurementStationConfigurationEndToEnd EndToEndConfiguration { get; set; }
    }
}