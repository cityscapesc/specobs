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

namespace Microsoft.Spectrum.Portal
{
    public class PortalConstants
    {
        public const string ApplicationName = "CityScape Spectrum Observatory";

        public const string MicrosoftProvider = "Microsoft";

        public const int DownsampleThreshold = 1000;

        public const double RFSensoreDefaultGain = 20;

        public const int RFSensoreDefaultTuneSleep = 0;

        public const double RFSensoreDefaultBandwith = 25000000;

        public const int RFSensoreNumberOfSampleBlocksToThrowAway = 0;

        public const int RFSensoreNumberOfSmapleBlocksPerScan = 1;

        public const string USRPDefaultCommunicationChannel = "addr";

        public const string USRPDefaultAntennaPort = "RX2";

        public const int USRPSamplesPerScan = 1024;

        public const string RFExplorerDefaultAntennaPort = "N/A";

        public const string RFExplorerDefaultCommunicationChannel = "COM3";

        public const int RFExplorerSamplesPerScan = 112;

        public const int RawIQSecondsOfDataPerFile = 600;

        public const int RawIQRetentionSeconds = 14400;

        public const double ClientAggregationMinutesOfDataPerScanFile = 60;

        public const double ClientAggregationSecondsOfDataPerScanFile = 60;
    }   
}