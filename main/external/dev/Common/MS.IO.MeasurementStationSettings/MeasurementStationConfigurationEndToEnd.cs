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
    public class MeasurementStationConfigurationEndToEnd
    {
        public MeasurementStationConfigurationEndToEnd()
        {
            this.RFSensorConfigurations = new List<RFSensorConfigurationEndToEnd>();
            this.AggregationConfiguration = new ClientAggregationConfiguration();
            this.RawIqConfiguration = new RawIqDataConfigurationElement();            
        }

        [ProtoMember(1)]
        public List<RFSensorConfigurationEndToEnd> RFSensorConfigurations { get; set; }

        [ProtoMember(2)]
        public ClientAggregationConfiguration AggregationConfiguration { get; set; }

        [ProtoMember(3)]
        public RawIqDataConfigurationElement RawIqConfiguration { get; set; }

        [ProtoMember(4)]
        public DateTime LastModifiedTime { get; set; }

        [ProtoMember(5)]
        public string MeasurementStationId { get; set; }

        [ProtoMember(6)]
        public string Name { get; set; }

        [ProtoMember(7)]
        public double Latitude { get; set; }

        [ProtoMember(8)]
        public double Longitude { get; set; }

        [ProtoMember(9)]
        public string Description { get; set; }

        [ProtoMember(10)]
        public string Location { get; set; }

        [ProtoMember(11)]
        public string AddressLine1 { get; set; }

        [ProtoMember(12)]
        public string AddressLine2 { get; set; }

        [ProtoMember(13)]
        public string Country { get; set; }

        public static MeasurementStationConfigurationEndToEnd Read(Stream input)
        {
            return Serializer.Deserialize<MeasurementStationConfigurationEndToEnd>(input);
        }

        public void Write(Stream output)
        {
            Serializer.Serialize<MeasurementStationConfigurationEndToEnd>(output, this);
        }
    }
}
