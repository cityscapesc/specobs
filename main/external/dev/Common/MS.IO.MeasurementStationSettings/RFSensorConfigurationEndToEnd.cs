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
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ProtoBuf;

    [ProtoContract]
    public class RFSensorConfigurationEndToEnd
    {
        public RFSensorConfigurationEndToEnd()
        {
            this.Cables = new List<CableConfiguration>();
            this.Connectors = new List<ConnectorConfiguration>();
            this.Antennas = new List<AntennaConfiguration>();
        }

        [ProtoMember(1)]
        public List<CableConfiguration> Cables { get; set; }

        [ProtoMember(2)]
        public List<ConnectorConfiguration> Connectors { get; set; }

        [ProtoMember(3)]
        public List<AntennaConfiguration> Antennas { get; set; }

        [ProtoMember(4)]
        public string DescriptiveName { get; set; }
        
        [ProtoMember(5)]
        public string DeviceType { get; set; }        

        [ProtoMember(6)]
        public double MinPossibleStartFrequencyHz { get; set; }

        [ProtoMember(7)]
        public double MaxPossibleEndFrequencyHz { get; set; }                       

        [ProtoMember(8)]
        public string DeviceAddress { get; set; }      

        [ProtoMember(9)]
        public double CurrentStartFrequencyHz { get; set; }      

        [ProtoMember(10)]
        public double CurrentStopFrequencyHz { get; set; }      

        [ProtoMember(11)]
        public double Gain { get; set; }      

        [ProtoMember(12)]
        public string AntennaPort { get; set; }      

        [ProtoMember(13)]
        public string ScanPattern { get; set; }      

        [ProtoMember(14)]
        public string CommunicationsChannel { get; set; }            

        [ProtoMember(15)]
        public bool LockingCommunicationsChannel { get; set; }      

        [ProtoMember(16)]
        public double BandwidthHz { get; set; }      

        [ProtoMember(17)]
        public int TuneSleep { get; set; }       

        [ProtoMember(18)]
        public int SamplesPerScan { get; set; }      

        [ProtoMember(19)]
        public int NumberOfSampleBlocksToThrowAway { get; set; }      

        [ProtoMember(20)]
        public int NumberOfSampleBlocksPerScan { get; set; }      

        [ProtoMember(21)]
        public bool GpsEnabled { get; set; }    
        
        [ProtoMember(22)]
        public int AdditionalTuneDelayInMilliSecs { get; set; }  
    }
}
