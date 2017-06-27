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

namespace Microsoft.Spectrum.Storage.DataContracts
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage
    /// Class:          IUserManagementTableOperations
    /// Description:    interface for <see cref="UserManagementTableOperations"/> class
    /// ----------------------------------------------------------------- 
    public interface IUserManagementTableOperations
    {
        /// <summary>
        /// Get user details by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>user</returns>
        User GetUserById(int userId);

        /// <summary>
        /// Get user details by the provider user Id
        /// </summary>
        /// <param name="accountEmail">user name</param>
        /// <returns>user</returns>
        User GetUserByProviderUserId(string providerUserId);
        
        /// <summary>
        /// Get user details by user name
        /// </summary>
        /// <param name="accountEmail">user name</param>
        /// <returns>user</returns>
        User GetUserByAccountEmail(string accountEmail);

        /// <summary>
        /// insert or update user
        /// </summary>
        /// <param name="user">user details</param>
        void InsertOrUpdateUser(User user);

        /// <summary>
        /// Get user roles by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="measurementStationId">MeasurementStationId</param>
        /// <returns>user role</returns>
        UserRole GetUserRole(int userId, Guid measurementStationId);

        /// <summary>
        /// insert or update user role
        /// </summary>
        /// <param name="userRole"></param>
        void InsertOrUpdate(UserRole userRole);

        /// <summary>
        /// Get membership info by provider user id
        /// </summary>
        /// <param name="providerUserId">provider user id</param>
        /// <returns>membership info</returns>
        OAuthMembershipInfo GetMembershipInfoByProviderUserId(string providerUserId);

        /// <summary>
        /// Gets membership info by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>membership info</returns>
        OAuthMembershipInfo GetMembershipInfoByUserId(int userId);

        /// <summary>
        /// Inserts or update membership info
        /// </summary>
        /// <param name="membershipInfo">membership info</param>
        void InsertOrUpdateMembershipInfo(OAuthMembershipInfo membershipInfo);

        /// <summary>
        /// Gets all the Station administrators for a measurement station.
        /// </summary>
        /// <param name="measurementStationId">Measurement Station id.</param>
        /// <returns>Collection of Station administrators.</returns>
        IEnumerable<User> GetAllStationAdmins(Guid measurementStationId);

        /// <summary>
        /// Gets all the Site administrators.
        /// </summary>
        /// <returns>Collection of Site administrators.</returns>
        IEnumerable<User> GetAllSiteAdmins();

        /// <summary>
        /// Get all the roles of a user
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>list of roles</returns>
        IEnumerable<UserRole> GetUserRoles(int userId);

        void RemoveAdmin(int userId, Guid measurementStationId);
    }
}
