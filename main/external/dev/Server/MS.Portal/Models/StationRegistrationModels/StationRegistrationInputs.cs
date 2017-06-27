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
    using Validations;

    public class StationRegistrationInputs
    {
        [Required]
        public string StationName { get; set; }

        public string Description { get; set; }

        [Required]
        public Address Address { get; set; }

        [Required]
        public string StationType { get; set; }

        [Required]
        public string RadioType { get; set; }

        [Required]
        public GpsDetails Gps { get; set; }

        [Required]
        public StationContact ContactInfo { get; set; }

        [Required]
        public ClientAggregationConfigurationViewModel ClientAggregationConfiguration { get; set; }

        [Required]
        public List<RFSensorConfigurationEndToEndViewModel> RFSensorConfigurationEndToEnd { get; set; }

        public RawIqDataConfigurationElementViewModel RawIqDataConfiguration { get; set; }

        public SelectList Countries { get; set; }

        public SelectList StationTypes { get; set; }

        public bool ReceiveHealthStatusNotifications { get; set; }

        [Required]
        [GreaterThan(5, true, ErrorMessage = "{0} value must be greater than or equal to {1}")]
        public int ClientHealthStatusCheckIntervalInMin { get; set; }
    }
}