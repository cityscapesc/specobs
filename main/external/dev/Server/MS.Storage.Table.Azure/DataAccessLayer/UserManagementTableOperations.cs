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

namespace Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Microsoft.WindowsAzure.Storage.Table;
    using UserRoleType = Microsoft.Spectrum.Storage.Enums.UserRoles;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage.Table.Azure
    /// Class:          UserManagementTableOperations
    /// Description:    class containing operations to deal with user related tables
    /// ----------------------------------------------------------------- 
    public class UserManagementTableOperations : IUserManagementTableOperations
    {
        private readonly RetryAzureTableOperations<Users> userTableOperations;

        private readonly RetryAzureTableOperations<UserRoles> userRolesTableOperations;

        private readonly RetryAzureTableOperations<WebpagesOAuthMembership> oauthMembershipTableOperations;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserManagementTableOperations"/> class
        /// </summary>
        /// <param name="dataContext">data context containing table references</param>
        public UserManagementTableOperations(AzureTableDbContext dataContext)
        {
            if (dataContext == null)
            {
                throw new ArgumentNullException("dataContext");
            }

            this.userTableOperations = dataContext.UserTableOperations;
            this.userTableOperations.GetTableReference(AzureTableHelper.UsersTable);

            this.userRolesTableOperations = dataContext.UserRoleTableOperations;
            this.userRolesTableOperations.GetTableReference(AzureTableHelper.UserRoleTable);

            this.oauthMembershipTableOperations = dataContext.OAuthMembershipTableOperations;
            this.oauthMembershipTableOperations.GetTableReference(AzureTableHelper.WebpagesOAuthMembershipTable);
        }

        public User GetUserByProviderUserId(string providerUserId)
        {
            return this.GetUserById(this.GetMembershipInfoByProviderUserId(providerUserId).UserId);
        }

        /// <summary>
        /// Get user details by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>user</returns>
        public User GetUserByAccountEmail(string accountEmail)
        {
            if (string.IsNullOrWhiteSpace(accountEmail))
            {
                throw new ArgumentException("accountEmail should not be empty", "accountEmail");
            }

            string likeString = accountEmail.ToLower(CultureInfo.InvariantCulture).Replace(" ", string.Empty);
            Users user = this.userTableOperations.QueryEntities<Users>(x => x.RowKey.IndexOf(likeString, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();

            if (user != null)
            {
                return new User(user.Id, user.UserName, user.FirstName, user.LastName, user.Location, user.Region, user.TimeZoneId, user.PreferredEmail, user.AccountEmail, user.CreatedOn.ToString(), user.UpdatedTime.ToString(), user.Link, user.Gender, user.Address1, user.Address2, user.Phone, user.Country, user.ZipCode, user.PhoneCountryCode, user.SubscribeNotifications);
            }

            return null;
        }

        /// <summary>
        /// Get user details by user name
        /// </summary>
        /// <param name="userName">user name</param>
        /// <returns>user</returns>
        public void InsertOrUpdateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentException("user can not be null", "user");
            }

            Users userTableEntity = new Users
            {
                AccountEmail = user.AccountEmail,
                Address1 = user.Address1,
                Address2 = user.Address2,
                Country = user.Country,
                CreatedOn = Convert.ToDateTime(user.CreatedOn, CultureInfo.InvariantCulture),
                FirstName = user.FirstName,
                Gender = user.Gender,
                LastName = user.LastName,
                Link = user.Link,
                Location = user.Location,
                PartitionKey = Constants.DummyPartitionKey,
                RowKey = user.UserName.Replace(" ", string.Empty).ToLower(CultureInfo.InvariantCulture) + ":" + user.UserId.ToString(CultureInfo.InvariantCulture),
                Phone = user.Phone,
                PhoneCountryCode = user.PhoneCountryCode,
                PreferredEmail = user.PreferredEmail,
                Region = user.Region,
                TimeZone = user.TimeZone,
                TimeZoneId = user.TimeZoneId,
                UpdatedTime = Convert.ToDateTime(user.UpdatedTime, CultureInfo.InvariantCulture),
                UserName = user.UserName,
                ZipCode = user.ZipCode,
                SubscribeNotifications = user.SubscribeNotifications
            };

            this.userTableOperations.InsertOrReplaceEntity(userTableEntity, true);
        }

        /// <summary>
        /// insert or update user
        /// </summary>
        /// <param name="user">user details</param>
        public UserRole GetUserRole(int userId, Guid measurementStationId)
        {
            UserRoles userRoleEntity = this.userRolesTableOperations.GetByKeys<UserRoles>(measurementStationId.ToString(), userId.ToString(CultureInfo.InvariantCulture)).FirstOrDefault();

            if (userRoleEntity != null)
            {
                return new UserRole(userRoleEntity.UserId, userRoleEntity.Role, userRoleEntity.MeasurementStationId);
            }

            return null;
        }

        public void RemoveAdmin(int userId, Guid measurementStationId)
        {
            UserRoles userRoleEntity = this.userRolesTableOperations.GetByKeys<UserRoles>(measurementStationId.ToString(), userId.ToString(CultureInfo.InvariantCulture)).FirstOrDefault();
            this.userRolesTableOperations.DeleteEntity(userRoleEntity);
        }

        /// <summary>
        /// Get user roles by UserId
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <returns>list of user roles</returns>
        public IEnumerable<UserRole> GetUserRoles(int userId)
        {
            string query = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, userId.ToString(CultureInfo.InvariantCulture));
            return this.userRolesTableOperations.ExecuteQueryWithContinuation<UserRoles>(query)
                                              .Select(x => new UserRole(x.UserId, x.Role, x.MeasurementStationId));
        }

        /// <summary>
        /// Get user roles by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>user role</returns>
        public void InsertOrUpdate(UserRole userRole)
        {
            if (userRole == null)
            {
                throw new ArgumentException("userRole can not be null", "userRole");
            }

            UserRoles userRoleEntity = new UserRoles(userRole.MeasurementStationId, userRole.UserId)
            {
                Role = userRole.Role
            };

            this.userRolesTableOperations.InsertOrReplaceEntity(userRoleEntity, true);
        }

        /// <summary>
        /// insert or update user role
        /// </summary>
        /// <param name="userRole"></param>
        public OAuthMembershipInfo GetMembershipInfoByProviderUserId(string providerUserId)
        {
            WebpagesOAuthMembership membershipEntity = this.oauthMembershipTableOperations.QueryEntities<WebpagesOAuthMembership>(x =>
            {
                System.Diagnostics.Debug.WriteLine(x.RowKey);
                return x.RowKey.IndexOf(providerUserId, StringComparison.OrdinalIgnoreCase) == 0;
            }).SingleOrDefault();

            if (membershipEntity != null)
            {
                return new OAuthMembershipInfo(membershipEntity.ProviderUserId, membershipEntity.Provider, membershipEntity.UserId);
            }

            return null;
        }

        /// <summary>
        /// Get membership info by provider user id
        /// </summary>
        /// <param name="providerUserId">provider user id</param>
        /// <returns>membership info</returns>
        public OAuthMembershipInfo GetMembershipInfoByUserId(int userId)
        {
            WebpagesOAuthMembership membershipEntity = this.oauthMembershipTableOperations.QueryEntities<WebpagesOAuthMembership>(x => x.RowKey.IndexOf(userId.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase) > 0).SingleOrDefault();

            return new OAuthMembershipInfo(membershipEntity.ProviderUserId, membershipEntity.Provider, membershipEntity.UserId);
        }

        /// <summary>
        /// Gets membership info by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>membership info</returns>
        public void InsertOrUpdateMembershipInfo(OAuthMembershipInfo membershipInfo)
        {
            if (membershipInfo == null)
            {
                throw new ArgumentException("membershipInfo can not be null", "membershipInfo");
            }

            WebpagesOAuthMembership membershipEntity = new WebpagesOAuthMembership(membershipInfo.ProviderUserId, membershipInfo.UserId.ToString(CultureInfo.InvariantCulture), membershipInfo.Provider);

            this.oauthMembershipTableOperations.InsertOrReplaceEntity(membershipEntity, true);
        }

        /// <summary>
        /// Inserts or update membership info
        /// </summary>
        /// <param name="membershipInfo">membership info</param>
        public User GetUserById(int userId)
        {
            string likeString = ":" + userId.ToString(CultureInfo.InvariantCulture);
            Users user = this.userTableOperations.QueryEntities<Users>(x => x.RowKey.IndexOf(likeString, StringComparison.OrdinalIgnoreCase) > 0).SingleOrDefault();

            if (user != null)
            {
                return new User(user.Id, user.UserName, user.FirstName, user.LastName, user.Location, user.Region, user.TimeZoneId, user.PreferredEmail, user.AccountEmail, user.CreatedOn.ToString(), user.UpdatedTime.ToString(), user.Link, user.Gender, user.Address1, user.Address2, user.Phone, user.Country, user.ZipCode, user.PhoneCountryCode, user.SubscribeNotifications);
            }

            return null;
        }

        /// <summary>
        /// Gets all the Station administrators for the measurement station.
        /// </summary>
        /// <param name="measurementStationId">Measurement Station Id.</param>
        /// <returns>Collection Station administrators.</returns>
        public IEnumerable<User> GetAllStationAdmins(Guid measurementStationId)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "Measurement station id can not be null");
            }

            string stationAdminRole = UserRoleType.StationAdmin.ToString();
            string partitionKey = measurementStationId.ToString();

            IEnumerable<User> stationAdmins = this.userRolesTableOperations.QueryEntities<UserRoles>(
                (user) =>
                {
                    return (user.PartitionKey == partitionKey)
                        && (string.Compare(user.Role, stationAdminRole, StringComparison.OrdinalIgnoreCase) == 0);
                })
                .Select(stationAmdin => this.GetUserById(stationAmdin.UserId));

            return stationAdmins;
        }

        /// <summary>
        /// Gets all the Site administrators.
        /// </summary>
        /// <returns>Collection site administrators.</returns>
        public IEnumerable<User> GetAllSiteAdmins()
        {
            string stationAdminRole = UserRoleType.SiteAdmin.ToString();

            IEnumerable<User> siteAdmins = this.userRolesTableOperations.QueryEntities<UserRoles>(
                (user) =>
                {
                    // Idea behind having a different partition key for site Administrators instead of measurement station id is to avoid data redundancy that 
                    // can occur have a corresponding entry for each measurement station for a given user        
                    return (user.PartitionKey == Constants.SiteAdminsPartitionKey)
                        && (string.Compare(user.Role, stationAdminRole, StringComparison.OrdinalIgnoreCase) == 0);
                })
                .Select(siteAdmin => this.GetUserById(siteAdmin.UserId));

            return siteAdmins;
        }

        public User GetUserByEmail(string email)
        {
            Users user = this.userTableOperations.GetByKeys<Users>(Constants.DummyPartitionKey).Where(x => (string.Compare(x.AccountEmail, email, StringComparison.OrdinalIgnoreCase) == 0)).FirstOrDefault();

            if (user != null)
            {
                return new User(user.Id, user.UserName, user.FirstName, user.LastName, user.Location, user.Region, user.TimeZoneId, user.PreferredEmail, user.AccountEmail, user.CreatedOn.ToString(CultureInfo.InvariantCulture), user.UpdatedTime.ToString(CultureInfo.InvariantCulture), user.Link, user.Gender, user.Address1, user.Address2, user.Phone, user.Country, user.ZipCode, user.PhoneCountryCode, user.SubscribeNotifications);
            }

            return null;
        }
    }
}
