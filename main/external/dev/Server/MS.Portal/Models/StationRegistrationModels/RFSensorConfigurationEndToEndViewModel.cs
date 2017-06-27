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

namespace Microsoft.Spectrum.Portal.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using Microsoft.Spectrum.Portal.Validations;

    public class RFSensorConfigurationEndToEndViewModel
    {
        [Required]
        public List<CableConfigurationViewModel> Cables { get; set; }

        [Required]
        public List<ConnectorConfigurationViewModel> Connectors { get; set; }

        [Required]
        public List<AntennaConfigurationViewModel> Antennas { get; set; }

        [Required]
        public string DescriptiveName { get; set; }

        [Required]
        public string DeviceType { get; set; }

        [Required]
        [CompareNumbers("MaxPossibleEndFrequencyMHz", false, ErrorMessage = "MinPossibleStartFrequency should be less than MaxPossibleEndFrequency")]
        public double MinPossibleStartFrequencyMHz { get; set; }

        [Required]
        [CompareNumbers("MinPossibleStartFrequencyMHz", false, ErrorMessage = "MinPossibleStartFrequency should be less than MaxPossibleEndFrequency")]
        public double MaxPossibleEndFrequencyMHz { get; set; }

        [Required]
        public string DeviceAddress { get; set; }

        [Required]
        [CompareNumbers("CurrentStopFrequencyMHz", false, ErrorMessage = "CurrentStartFrequency should be less than CurrentStopFrequency")]
        public double CurrentStartFrequencyMHz { get; set; }

        [Required]
        [CompareNumbers("CurrentStartFrequencyMHz", false, ErrorMessage = "CurrentStartFrequency should be less than CurrentStopFrequency")]
        public double CurrentStopFrequencyMHz { get; set; }

        [Required]
        public double Gain { get; set; }

        [Required]
        public string AntennaPort { get; set; }

        [Required]
        public string ScanPattern { get; set; }

        [Required]
        public string CommunicationsChannel { get; set; }

        [Required]
        public bool LockingCommunicationsChannel { get; set; }

        [Required]
        public double BandwidthHz { get; set; }

        [Required]
        public int TuneSleep { get; set; }

        [Required]
        public int SamplesPerScan { get; set; }

        [Required]
        public int NumberOfSampleBlocksToThrowAway { get; set; }

        [Required]
        public int NumberOfSampleBlocksPerScan { get; set; }

        [Required]
        public bool GpsEnabled { get; set; }

        // TODO: Think of having this in StationRegistrationInputs, so that we will just have single memory footprint of this list.
        public SelectList ScanPatterns { get; set; }

        // TODO: Think of having this in StationRegistrationInputs, so that we will just have single memory footprint of this list.
        public SelectList DeviceTypes { get; set; }

        public bool AllowRFSensorConfigEntryDelete { get; set; }

        [Required]
        public int AdditionalTuneDelayInMilliSecs { get; set; }
    }
}