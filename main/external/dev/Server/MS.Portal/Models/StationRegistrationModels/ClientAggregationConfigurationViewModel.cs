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

    public class ClientAggregationConfigurationViewModel
    {
        [Required]
        [GreaterThan(60, true, ErrorMessage = "{0} value must be greater than or equal to {1}")]
        public double MinutesOfDataPerScanFile { get; set; }

        [Required]
        [GreaterThan(10, true, ErrorMessage = "{0} value must be greater than or equal to {1}")]
        public double SecondsOfDataPerSample { get; set; }

        [Required]
        public bool OutputData { get; set; }      
    }
}