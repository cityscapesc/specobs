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

namespace Microsoft.Spectrum.IO.MeasurementStationSettings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ProtoBuf;

    [ProtoContract]
    public class ClientAggregationConfiguration
    {
        public ClientAggregationConfiguration()
        {
        }

        [ProtoMember(1)]
        public bool SingleScan { get; set; }

        public TimeSpan MinutesOfDataPerScanFile
        {
            get { return TimeSpan.FromMinutes(this.MinutesOfDataPerScanFileImpl); }
            set { this.MinutesOfDataPerScanFileImpl = value.TotalMinutes; }
        }

        public TimeSpan SecondsOfDataPerSample
        {
            get { return TimeSpan.FromSeconds(this.SecondsOfDataPerSampleImpl); }
            set { this.SecondsOfDataPerSampleImpl = value.TotalSeconds; }
        }

        [ProtoMember(2)]
        private double MinutesOfDataPerScanFileImpl { get; set; }

        [ProtoMember(3)]
        private double SecondsOfDataPerSampleImpl { get; set; }

        [ProtoMember(4)]
        public bool OutputData { get; set; }
    }
}
