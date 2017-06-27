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
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.Enums;
    using Newtonsoft.Json;    

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage
    /// Class:          UserManager
    /// Description:    Operations required to work with user details
    /// ----------------------------------------------------------------- 
    /// </summary>
    public class UserManager
    {
        private readonly IUserManagementTableOperations userManagementTableOperations;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserManager"/> class
        /// </summary>
        /// <param name="userManagementTableOperations"></param>
        public UserManager(IUserManagementTableOperations userManagementTableOperations)
        {
            this.userManagementTableOperations = userManagementTableOperations;
        }

        /// <summary>
        /// Gets user information from provider service by passing access token
        /// </summary>
        /// <param name="accessToken">access token</param>
        /// <returns>client user data got from provider</returns>
        public static ClientUserData GetClientUserData(string accessToken)
        {
            // return GetClientDataAsyn(accessToken).Result;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://apis.live.net/v5.0/");

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // New code:
                string callingMethod = string.Format(CultureInfo.InvariantCulture, "me?access_token={0}", accessToken);
                HttpResponseMessage response = client.GetAsync(callingMethod).Result;
                if (response.IsSuccessStatusCode)
                {
                    string jsonUserData = response.Content.ReadAsStringAsync().Result;
                    ClientUserData userData = JsonConvert.DeserializeObject<ClientUserData>(jsonUserData);
                    return userData;
                }
                else
                {
                    string jsonErrorData = response.Content.ReadAsStringAsync().Result;
                    AuthenticationResponse errorData = JsonConvert.DeserializeObject<AuthenticationResponse>(jsonErrorData);
                    throw new AccessDeniedException(errorData.AuthenticationResponseError);
                }
            }
        }

        /// <summary>
        /// Get user details by provider user id
        /// </summary>
        /// <param name="providerUserId">provider user id</param>
        /// <returns>user information</returns>
        public UserInfo GetUserDetailsByProviderUserId(string providerUserId)
        {
            OAuthMembershipInfo membershipInfo = this.userManagementTableOperations.GetMembershipInfoByProviderUserId(providerUserId);

            if (membershipInfo != null)
            {
                User userInfo = this.userManagementTableOperations.GetUserById(membershipInfo.UserId);

                return new UserInfo(userInfo, membershipInfo, null, null, null);
            }

            return null;
        }

        /// <summary>
        /// Get user details by user Id
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <param name="measurementStationId">measurementStation Id</param>
        /// <returns>user information</returns>
        public UserInfo GetUserInfoByUserId(int userId, Guid measurementStationId)
        {
            User userInfo = this.userManagementTableOperations.GetUserById(userId);

            if (userInfo != null)
            {
                UserRole roleInfo = this.userManagementTableOperations.GetUserRole(userInfo.UserId, measurementStationId);

                ////not including membership info, as it is not needed most of the time
                return new UserInfo(userInfo, null, roleInfo, null, null);
            }

            return null;
        }

        /// <summary>
        /// Get user information by access token
        /// </summary>
        /// <param name="accessToken">access token given by provider</param>
        /// <returns>user information</returns>
        public UserInfo GetUserInfoByAccessToken(string accessToken)
        {
            ClientUserData userData = UserManager.GetClientUserData(accessToken);

            if (userData != null)
            {
                return this.GetUserDetailsByProviderUserId(userData.ProviderUserId);
            }

            return null;
        }

        /// <summary>
        /// Get membership information by provider user id
        /// </summary>
        /// <param name="providerUserId">provider user id</param>
        /// <returns>membership information</returns>
        public OAuthMembershipInfo GetMembershipInfoByProviderUserId(string providerUserId)
        {
            return this.userManagementTableOperations.GetMembershipInfoByProviderUserId(providerUserId);
        }

        /// <summary>
        /// Get membership information by user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>membership information</returns>
        public OAuthMembershipInfo GetMembershipInfoByUserId(int userId)
        {
            return this.userManagementTableOperations.GetMembershipInfoByUserId(userId);
        }

        /// <summary>
        /// Get user details by user id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public User GetUserById(int userId)
        {
            return this.userManagementTableOperations.GetUserById(userId);
        }

        public User GetUserByProviderUserId(string providerUserId)
        {
            return this.userManagementTableOperations.GetUserByProviderUserId(providerUserId);
        }

        /// <summary>
        /// Get user details by user name
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public User GetUserByAccountEmail(string accountEmail)
        {
            return this.userManagementTableOperations.GetUserByAccountEmail(accountEmail);
        }

        /// <summary>
        /// Saves membership information
        /// </summary>
        /// <param name="memberShipInfo">membership information</param>
        public void SaveMembershipInfo(OAuthMembershipInfo membershipInfo)
        {
            this.userManagementTableOperations.InsertOrUpdateMembershipInfo(membershipInfo);
        }

        /// <summary>
        /// Save user all the details
        /// </summary>
        /// <param name="userInfo">user information</param>
        public void SaveUserInfo(UserInfo userInfo)
        {
            if (userInfo == null)
            {
                throw new ArgumentException("userInfo can not be null", "userInfo");
            }

            if (userInfo.User != null)
            {
                this.userManagementTableOperations.InsertOrUpdateUser(userInfo.User);
            }

            if (userInfo.Role != null)
            {
                this.userManagementTableOperations.InsertOrUpdate(userInfo.Role);
            }

            if (userInfo.MembershipInfo != null)
            {
                this.userManagementTableOperations.InsertOrUpdateMembershipInfo(userInfo.MembershipInfo);
            }
        }

        public UserRole GetUserRoleByProviderUserId(string providerUserId, Guid measurementStationId)
        {
            User user = this.userManagementTableOperations.GetUserByProviderUserId(providerUserId);

            if (user != null)
            {
                return this.userManagementTableOperations.GetUserRole(user.UserId, measurementStationId);
            }

            return null;
        }

        public UserRoles GetUserRoleByProviderUserId(string providerUserId)
        {
            User user = this.userManagementTableOperations.GetUserByProviderUserId(providerUserId);

            string[] measurementStations = this.GetAcessableStationIds(user.UserId);

            if (measurementStations != null)
            {
                if (measurementStations.Count() > 0)
                {
                    if (measurementStations.Contains(Constants.SiteAdminsPartitionKey))
                    {
                        return UserRoles.SiteAdmin;
                    }

                    return UserRoles.StationAdmin;
                }
            }

            return UserRoles.User;
        }

        /// <summary>
        /// Get User roles by name
        /// </summary>
        /// <param name="account email">user name</param>
        /// <param name="measurementStationId">measurementStation Id</param>
        /// <returns>user roles</returns>
        public UserRole GetUserRoleByAccountEmail(string accountEmail, Guid measurementStationId)
        {
            User user = this.userManagementTableOperations.GetUserByAccountEmail(accountEmail);

            if (user != null)
            {
                return this.userManagementTableOperations.GetUserRole(user.UserId, measurementStationId);
            }

            return null;
        }

        public UserRoles GetUserRoleByAccountEmail(string accountEmail)
        {
            User user = this.userManagementTableOperations.GetUserByAccountEmail(accountEmail);

            string[] measurementStations = this.GetAcessableStationIds(user.UserId);

            if (measurementStations != null)
            {
                if (measurementStations.Count() > 0)
                {
                    if (measurementStations.Contains(Constants.SiteAdminsPartitionKey))
                    {
                        return UserRoles.SiteAdmin;
                    }

                    return UserRoles.StationAdmin;
                }
            }

            return UserRoles.User;
        }

        public string[] GetAcessableStationIds(int userId)
        {
            IEnumerable<UserRole> roles = this.userManagementTableOperations.GetUserRoles(userId);

            if (roles.Count() > 0)
            {
                return roles.Select(x => x.MeasurementStationId).ToArray();
            }

            return null;
        }        

        public IEnumerable<User> GetAllStationAdmins(Guid measurementStationId)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "MeasurementStation Id can not be null");
            }

            try
            {
                return this.userManagementTableOperations.GetAllStationAdmins(measurementStationId);
            }
            catch
            {
                // TODO: Exception logging.                
                throw;
            }
        }

        public IEnumerable<User> GetAllSiteAdmins()
        {
            try
            {
                return this.userManagementTableOperations.GetAllSiteAdmins();
            }
            catch
            {
                // TODO: Exception logging.
                throw;
            }
        }

        public bool AddUserToStation(string emailId, Guid measurementStationId, out string resultMessage)
        {
            User user = this.userManagementTableOperations.GetUserByAccountEmail(emailId);
            bool result = false;            

            if (user != null)
            {
                // Check whether user is Site Admin
                UserRoles role = this.GetUserRoleByAccountEmail(user.AccountEmail);

                if (role == UserRoles.SiteAdmin)
                {
                    resultMessage = "The user you're trying is a super admin and do not require to be added again to the station admin list.";
                }
                else
                {
                    this.userManagementTableOperations.InsertOrUpdate(new UserRole(user.UserId, Microsoft.Spectrum.Storage.Enums.UserRoles.StationAdmin.ToString(), measurementStationId.ToString()));
                    resultMessage = "Success";
                    result = true;
                }
            }
            else
            {
                resultMessage = "The user was not found in our records. Please make sure the user registered with the Microsoft Spectrum Observatory.";
            }

            return result;
        }

        public void RemoveAdmin(int userId, Guid measurementStationId)
        {
            this.userManagementTableOperations.RemoveAdmin(userId, measurementStationId);
        }
    }
}
