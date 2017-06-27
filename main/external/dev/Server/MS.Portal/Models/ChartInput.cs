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
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ChartInput
    {
        [Required]
        public int TimeScale { get; set; }

        [Required]
        public string StartDate { get; set; }

        [Required]
        public string StartDateIso { get; set; }

        [Required]
        public string StartTime { get; set; }

        [Required]
        public double StartFrequency { get; set; }

        [Required]
        public double StopFrequency { get; set; }

        public bool RemoveOutliers { get; set; }

        public double OutlierThresholdPercentage { get; set; }

        public string MeasurementStationId { get; set; }

        public string StationStorage { get; set; }        
    }
}