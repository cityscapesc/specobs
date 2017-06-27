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
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Spectrum.Portal.Validations;

    public class RawIqDataConfigurationElementViewModel
    {
        [Required]
        public bool OutputData { get; set; }

        [Required]
        [CompareNumbers("StopFrequencyMHz", false, ErrorMessage = "StartFrequency should be less than StopFrequency")]
        // [AbsoluteDifferenceLimit(100000000, "StopFrequencyHz", ErrorMessage = "Difference between Start and Stop Freq should not be greater than 100 MHz")]
        public double StartFrequencyMHz { get; set; }

        [Required]
        [CompareNumbers("StartFrequencyMHz", false, ErrorMessage = "StartFrequency should be less than StopFrequency")]
        // [AbsoluteDifferenceLimit(100000000, "StartFrequencyHz", ErrorMessage = "Difference between Start and Stop Freq should not be greater than 100 MHz")]
        public double StopFrequencyMHz { get; set; }

        [Required]
        [GreaterThan(60, true, ErrorMessage = "{0} value must be greater than or equal to {1}")]
        public int SecondsOfDataPerFile { get; set; }

        [Required]
        //[GreaterThan(3600, true, ErrorMessage = "{0} value must be greater than or equal to {1}")]
        public int RetentionSeconds { get; set; }

        [Required]
        //[GreaterThan(0, true, ErrorMessage = "{0} value must be greater than or equal to {1}")]
        //[LessThan(1000, true, ErrorMessage = "{0} value must be less than or equal to {1}")]
        //[CompareNumbers("DutycycleTimeInSec", false, ErrorMessage = "DutycycleOnTimeInSec should be less than DutycycleTimeInSec")]
        public int DutycycleOnTimeInMilliSec { get; set; }

        [Required]
        //[GreaterThan(60, true, ErrorMessage = "{0} value must be greater than or equal to {1}")]
        //[CompareNumbers("DutycycleOnTimeInMilliSec", true, ErrorMessage = "DutycycleOnTimeInSec should be less than DutycycleTimeInSec")]
        public int DutycycleTimeInMilliSec { get; set; }

        [Required]
        public bool OuputPSDDataInDutyCycleOffTime { get; set; }

        public string RawIQScanPolicyCategory { get; set; }

        public string RawIQPolicyDetails { get; set; }
    }
}