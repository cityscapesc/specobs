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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;   
    using Microsoft.Spectrum.Common;
    using ProtoBuf;    

    [DebuggerDisplay("{DebuggerDisplay()}")]
    [ProtoContract]
    public class SpectralPsdDataBlock : DataBlock
    {
        private FixedShort[] dataPoints;
        private short[] outputDataPoints;

        public SpectralPsdDataBlock(DateTime timestamp, double startFrequencyHz, double stopFrequencyHz, ReadingKind readingKind, FixedShort[] dataPoints, int deviceId, string nmeaGpggaLocation)
        {
            this.Timestamp = timestamp;
            this.StartFrequencyHz = startFrequencyHz;
            this.StopFrequencyHz = stopFrequencyHz;
            this.ReadingKind = readingKind;            
            this.DataPoints = dataPoints;
            this.DeviceId = deviceId;
            this.NmeaGpggaLocation = nmeaGpggaLocation;
        }

        internal SpectralPsdDataBlock()
        {
        }

        [ProtoMember(1)]
        public override DateTime Timestamp { get; set; }

        [ProtoMember(2)]
        public double StartFrequencyHz { get; set; }

        [ProtoMember(3)]
        public double StopFrequencyHz { get; set; }

        [ProtoMember(4)]
        public ReadingKind ReadingKind { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Performance is important")]
        [ProtoMember(5, DataFormat = DataFormat.ZigZag, IsPacked = true)]
        public short[] OutputDataPoints 
        { 
            get
            {                
                return this.outputDataPoints;
            }

            set
            {
                this.outputDataPoints = value;
                if (this.dataPoints == null || this.dataPoints.Length != this.outputDataPoints.Length)
                {
                    this.dataPoints = new FixedShort[value.Length];
                }
               
                for (int i = 0; i < value.Length; i++)
                {
                    this.dataPoints[i].Value = value[i];
                }
            }
        }

        [ProtoMember(8)]
        public string NmeaGpggaLocation { get; private set; }
        
        public int DeviceId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
            Justification = "Performance is important")]
        public FixedShort[] DataPoints 
        { 
            get
            {
                return this.dataPoints;
            }

            set
            {
                this.dataPoints = value;
                if (this.outputDataPoints == null || this.outputDataPoints.Length != this.dataPoints.Length)
                {
                    this.outputDataPoints = new short[value.Length];
                }

                for (int i = 0; i < value.Length; i++)
                {
                    this.outputDataPoints[i] = value[i].Value;
                }
            }
        }

        private string DebuggerDisplay()
        {
            return string.Format(
                CultureInfo.InvariantCulture, 
                "SpectralDataBlock: {0} - {1} Hz, {2}, {3} items", 
                this.StartFrequencyHz, 
                this.StopFrequencyHz, 
                this.ReadingKind, 
                this.DataPoints.Length);
        }
    }
}