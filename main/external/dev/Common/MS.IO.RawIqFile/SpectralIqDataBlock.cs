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

namespace Microsoft.Spectrum.IO.RawIqFile
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ProtoBuf;

    [ProtoContract]
    public class SpectralIqDataBlock : DataBlock
    {
        public SpectralIqDataBlock(DateTime timestamp, double startFrequencyHz, double stopFrequencyHz, double centerFrequencyHz, double[] dataPoints, string nmeaGpggaLocation)
        {
            this.Timestamp = timestamp;
            this.StartFrequencyHz = startFrequencyHz;
            this.StopFrequencyHz = stopFrequencyHz;      
            this.DataPoints = dataPoints;
            this.CenterFrequencyHz = centerFrequencyHz;
            this.NmeaGpggaLocation = nmeaGpggaLocation;
        }

        internal SpectralIqDataBlock()
        {
        }

        [ProtoMember(1)]
        public override DateTime Timestamp { get; set; }

        [ProtoMember(2)]
        public double StartFrequencyHz { get; set; }

        [ProtoMember(3)]
        public double StopFrequencyHz { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Performance is important")]
        [ProtoMember(4, DataFormat = DataFormat.ZigZag, IsPacked = true)]
        public double[] DataPoints { get; set; }

        [ProtoMember(5)]
        public double CenterFrequencyHz { get; set; }

        [ProtoMember(6)]
        public string NmeaGpggaLocation { get; private set; }
    }
}
