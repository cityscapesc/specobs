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

namespace Microsoft.Spectrum.Portal.Security
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Portal.Helpers;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Models;
    using Microsoft.Spectrum.Storage.Table.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using Enums = Microsoft.Spectrum.Storage.Enums;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage
    /// Class:          UserPrincipal 
    /// Description:    Model to hold membership information
    /// ----------------------------------------------------------------- 
    public class UserPrincipal : IPrincipal
    {
        private Enums.UserRoles role = Enums.UserRoles.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPrincipal"/> class
        /// </summary>
        /// <param name="accessToken">access token got from provider</param>
        /// <param name="userName">user name</param>
        public UserPrincipal(string accessToken, string providerUserId)
        {
            Check.IsNotNull(accessToken, "accessToken");
            Check.IsNotNull(providerUserId, "providerUserId");

            this.AccessToken = accessToken;
            this.ProviderUserId = providerUserId;
            this.Identity = new GenericIdentity(providerUserId);
        }

        /// <summary>
        /// access token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// user name
        /// </summary>
        public string ProviderUserId { get; set; }

        /// <summary>
        /// user identity
        /// </summary>
        public IIdentity Identity { get; private set; }

        /// <summary>
        /// Gets role of a user
        /// </summary>
        public Microsoft.Spectrum.Storage.Enums.UserRoles Role
        {
            get
            {
                if (this.role == Enums.UserRoles.None)
                {
                    this.role = PortalGlobalCache.Instance.UserManager.GetUserRoleByProviderUserId(this.ProviderUserId);
                }

                return this.role;
            }
        }

        /// <summary>
        /// checks role of user
        /// </summary>
        /// <param name="role">role name</param>
        /// <returns>bool value</returns>
        public bool IsInRole(string role)
        {
            IEnumerable<MeasurementStationInfo> measurementStations = PortalGlobalCache.Instance.MeasurementStationTableOperation.GetAllMeasurementStationInfoPublic();

            if (measurementStations == null || !measurementStations.Any())
            {
                // TODO: Log no measurement station found error here.
                return false;
            }

            return measurementStations.Any(station => this.IsInRole(role, station.Identifier.Id));
        }

        public bool IsInRole(string role, string measurementStationId)
        {
            // This indicates a Site Administrator.
            //if (string.Compare(measurementStationId, Constants.SiteAdminsPartitionKey, StringComparison.OrdinalIgnoreCase) == 0)
            //{
            //    return true;
            //}

            Guid measurementStationIdentifier;

            if (!Guid.TryParse(measurementStationId, out measurementStationIdentifier))
            {
                // TODO: Log a error message saying invalid measurement station id.
                return false;
            }

            return this.IsInRole(role, Guid.Parse(measurementStationId));
        }

        public bool IsInRole(string role, Guid measurementStationId)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException("User role can not be empty", "role");
            }

            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "MeasurementStaionId parameter can not be null");
            }

            UserManager userManager = PortalGlobalCache.Instance.UserManager;

            UserRole userRole = userManager.GetUserRoleByProviderUserId(this.ProviderUserId, measurementStationId);

            if (userRole != null)
            {
                return userRole.Role.Equals(role, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }
    }
}
