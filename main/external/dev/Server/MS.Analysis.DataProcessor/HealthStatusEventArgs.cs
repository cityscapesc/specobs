// Copyright (c) Microsoft Corporation
//
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use file except in compliance 
// with the License.  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER
// EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE,
// FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.

namespace Microsoft.Spectrum.Analysis.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Spectrum.Storage;

    public class HealthStatusEventArgs : EventArgs
    {
        public HealthStatusEventArgs(IEnumerable<MeasurementStationHealthStatus> stationsHealthStatus, IEnumerable<string> siteAdmins, IEnumerable<Feedback> userFeedbackList)
        {
            if (stationsHealthStatus == null)
            {
                throw new ArgumentNullException("stationsHealthStatus", "MeasurementStation health status can not be null");
            }

            if (siteAdmins == null)
            {
                throw new ArgumentNullException("siteAdmins", "Site Administrators alias list can not be null");
            }

            this.StationsHealthStatus = stationsHealthStatus;
            this.SiteAdmins = siteAdmins;
            this.UserFeedbackCollection = userFeedbackList;
        }

        public IEnumerable<MeasurementStationHealthStatus> StationsHealthStatus { get; private set; }

        public IEnumerable<string> SiteAdmins { get; private set; }

        public IEnumerable<Feedback> UserFeedbackCollection { get; private set; }
    }
}
