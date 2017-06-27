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

namespace Microsoft.Spectrum.Storage
{
    using System;

    public class UserRoleRequestConfiguration
    {
        public UserRoleRequestConfiguration(int userId, long requestRoleId, long roleRequestStatusId, long stationId, string roleJustificationText)
        {
            this.UserId = userId;
            this.RequestRoleId = requestRoleId;
            this.RoleRequestStatusId = roleRequestStatusId;
            this.StationId = stationId;

            // Station information is optional, its required at portal page to avoid frequent database hits. 
            this.StationAccessKey = null;
            this.StationName = null;
            this.RoleJustificationText = roleJustificationText;
        }

        public UserRoleRequestConfiguration(int userId, long requestRoleId, long roleRequestStatusId, long stationId, string stationAccessKey, string stationName, string roleJustificationText)
        {
            this.UserId = userId;
            this.RequestRoleId = requestRoleId;
            this.RoleRequestStatusId = roleRequestStatusId;
            this.StationId = stationId;
            this.StationAccessKey = stationAccessKey;
            this.StationName = stationName;
            this.RoleJustificationText = roleJustificationText;
        }

        public int UserId { get; private set; }

        public long RequestRoleId { get; private set; }

        public long RoleRequestStatusId { get; private set; }

        public long StationId { get; private set; }

        public string StationAccessKey { get; private set; }

        public string StationName { get; private set; }

        public string RoleJustificationText { get; private set; }
    }
}
