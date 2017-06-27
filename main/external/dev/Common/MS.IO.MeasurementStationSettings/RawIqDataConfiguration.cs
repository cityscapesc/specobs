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
    using ProtoBuf;

    [ProtoContract]
    public class RawIqDataConfigurationElement
    {
        public RawIqDataConfigurationElement()
        {
        }

        [ProtoMember(1)]
        public bool OutputData
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public double StartFrequencyHz
        {
            get;
            set;
        }

        [ProtoMember(3)]
        public double StopFrequencyHz
        {
            get;
            set;
        }

        [ProtoMember(4)]
        public int SecondsOfDataPerFile
        {
            get;
            set;
        }

        [ProtoMember(5)]
        public int RetentionSeconds
        {
            get;
            set;
        }

        [ProtoMember(6)]
        public int DutycycleOnTimeInMilliSec
        {
            get;
            set;
        }

        [ProtoMember(7)]
        public int DutycycleTimeInMilliSec
        {
            get;
            set;
        }

        [ProtoMember(8)]
        public bool OuputPSDDataInDutyCycleOffTime
        {
            get;
            set;
        }
    }
}