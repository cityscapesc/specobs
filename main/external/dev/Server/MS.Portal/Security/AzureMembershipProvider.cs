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
    using System.Collections.Generic;
    using System.Web.Security;
    using Microsoft.Spectrum.Portal.Helpers;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;    
    using WebMatrix.WebData;   

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:       Microsoft.Spectrum.Portal.Security
    /// Class:          AzureMembershipProvider
    /// Description:    custom simple membership class to work with azure tables
    /// ----------------------------------------------------------------- 
    public sealed class AzureMembershipProvider : ExtendedMembershipProvider
    {
        /// <summary>
        /// variable to hold <see cref="AzureTableOperations"/>
        /// </summary>
        private readonly UserManager userManager;

        /// <summary>
        /// Application name
        /// </summary>
        private string applicationName;        

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureMembershipProvider"/> class
        /// </summary>
        /// <param name="userManager">instance of user manager object</param>
        public AzureMembershipProvider()
        {
            this.userManager = PortalGlobalCache.Instance.UserManager;
        }

        #region properties

        /// <summary>
        /// Unimplemented property returns default value
        /// </summary>
        public override int MaxInvalidPasswordAttempts
        {
            get { return default(int); }
        }

        /// <summary>
        /// Unimplemented property returns default value
        /// </summary>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return default(int); }
        }

        /// <summary>
        /// Unimplemented property returns default value
        /// </summary>
        public override int MinRequiredPasswordLength
        {
            get { return default(int); }
        }

        /// <summary>
        /// Unimplemented property returns default value
        /// </summary>
        public override int PasswordAttemptWindow
        {
            get { return default(int); }
        }

        /// <summary>
        /// Unimplemented property returns default value
        /// </summary>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return default(MembershipPasswordFormat); }
        }

        /// <summary>
        /// Unimplemented property returns default value
        /// </summary>
        public override string PasswordStrengthRegularExpression
        {
            get { return default(string); }
        }

        /// <summary>
        /// Unimplemented property returns default value
        /// </summary>
        public override bool RequiresQuestionAndAnswer
        {
            get { return default(bool); }
        }

        /// <summary>
        /// Unimplemented property returns default value
        /// </summary>
        public override bool RequiresUniqueEmail
        {
            get { return default(bool); }
        }

        /// <summary>
        /// Gets application name
        /// </summary>
        public override string ApplicationName
        {
            get
            {
                return this.applicationName;
            }

            set
            {
                this.applicationName = PortalConstants.ApplicationName;
            }
        }

        /// <summary>
        /// Unimplemented, it returns default value boolean
        /// </summary>
        public override bool EnablePasswordReset
        {
            get { return default(bool); }
        }

        /// <summary>
        /// Unimplemented, returns default value
        /// </summary>
        public override bool EnablePasswordRetrieval
        {
            get { return default(bool); }
        }

        #endregion

        #region implemented methods

        /// <summary>
        /// Get user id for the given provider and provider user id
        /// </summary>
        /// <param name="provider">provider name</param>
        /// <param name="providerUserId">provider user id</param>
        /// <returns>user id</returns>
        public override int GetUserIdFromOAuth(string provider, string providerUserId)
        {
            try
            {
                OAuthMembershipInfo data = this.userManager.GetMembershipInfoByProviderUserId(providerUserId);

                if (data != null)
                {
                    return data.UserId;
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get user name for the given user id
        /// </summary>
        /// <param name="userId">user id</param>
        /// <returns>user name</returns>
        public override string GetUserNameFromId(int userId)
        {
            if (userId > 0)
            {
                User user = this.userManager.GetUserById(userId);

                if (user != null)
                {
                    return user.UserName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// create or update membership information of a user
        /// </summary>
        /// <param name="provider">provider name</param>
        /// <param name="providerUserId">provider user id</param>
        /// <param name="accountEmail">user name</param>
        public override void CreateOrUpdateOAuthAccount(string provider, string providerUserId, string accountEmail)
        {
            User userProfile = this.userManager.GetUserByAccountEmail(accountEmail);
            var membershipData = new OAuthMembershipInfo(providerUserId, provider, userProfile.UserId);

            this.userManager.SaveMembershipInfo(membershipData);
        }

        /// <summary>
        /// Get membership data 
        /// </summary>
        /// <param name="username">user name</param>
        /// <param name="userIsOnline">is user on line</param>
        /// <returns>user membership data</returns>
        public override MembershipUser GetUser(string accountEmail, bool userIsOnline)
        {
            var userProfile = this.userManager.GetUserByAccountEmail(accountEmail);
            var membership = this.userManager.GetMembershipInfoByUserId(userProfile.UserId);
            var providerUserId = membership != null ? membership.ProviderUserId : null;

            if (userProfile != null)
            {
                return new MembershipUser(Membership.Provider.Name, accountEmail, providerUserId, userProfile.PreferredEmail, string.Empty, string.Empty, true, false, DateTime.Parse(userProfile.CreatedOn), DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            }

            return null;
        }

        #endregion

        #region Unimplemented methods

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="accountConfirmationToken">account confirmation token</param>
        /// <returns>boolean value</returns>
        public override bool ConfirmAccount(string accountConfirmationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="accountConfirmationToken">account confirmation token</param>
        /// <returns>boolean value</returns>
        public override bool ConfirmAccount(string userName, string accountConfirmationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="password">user password</param>
        /// <param name="requireConfirmationToken">requires confirmation token</param>
        /// <returns>string value</returns>
        public override string CreateAccount(string userName, string password, bool requireConfirmationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="password">user password</param>
        /// <param name="requireConfirmation">require confirmation</param>
        /// <param name="values">related values</param>
        /// <returns>string value</returns>
        public override string CreateUserAndAccount(string userName, string password, bool requireConfirmation, IDictionary<string, object> values)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="userName">user name</param>
        /// <returns>boolean value</returns>
        public override bool DeleteAccount(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="userName">user name</param>
        /// <param name="tokenExpirationInMinutesFromNow">token expiration time</param>
        /// <returns>password rest token</returns>
        public override string GeneratePasswordResetToken(string userName, int tokenExpirationInMinutesFromNow)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="userName">user name</param>
        /// <returns>list of accounts associated to user name</returns>
        public override ICollection<OAuthAccountData> GetAccountsForUser(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="userName">user name</param>
        /// <returns>creation date</returns>
        public override DateTime GetCreateDate(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="userName">user name</param>
        /// <returns>last failure date</returns>
        public override DateTime GetLastPasswordFailureDate(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="userName">user name</param>
        /// <returns>password change data</returns>
        public override DateTime GetPasswordChangedDate(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="userName">user name</param>
        /// <returns>number of failure attempts from last success</returns>
        public override int GetPasswordFailuresSinceLastSuccess(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="token">password reset token</param>
        /// <returns>user id</returns>
        public override int GetUserIdFromPasswordResetToken(string token)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="userName">user name</param>
        /// <returns>is user name confirmed</returns>
        public override bool IsConfirmed(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="token">password reset token</param>
        /// <param name="newPassword">new password</param>
        /// <returns>boolean value</returns>
        public override bool ResetPasswordWithToken(string token, string newPassword)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="username">user name</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>boolean value</returns>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="username">user name</param>
        /// <param name="password">user password</param>
        /// <param name="newPasswordQuestion">new password question</param>
        /// <param name="newPasswordAnswer">new password answer</param>
        /// <returns>boolean value</returns>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="username">user name</param>
        /// <param name="password">user password</param>
        /// <param name="email">user email</param>
        /// <param name="passwordQuestion">password question</param>
        /// <param name="passwordAnswer">password answer</param>
        /// <param name="isApproved">is approved</param>
        /// <param name="providerUserKey">provider user key</param>
        /// <param name="status">user status</param>
        /// <returns>membership user</returns>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="username">user name</param>
        /// <param name="deleteAllRelatedData">delete all related data</param>
        /// <returns>boolean value</returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="emailToMatch">email to match</param>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <param name="totalRecords">total records</param>
        /// <returns>membership user collection</returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="usernameToMatch">user name to match</param>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <param name="totalRecords">total records</param>
        /// <returns>membership user collection</returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <param name="totalRecords">total records</param>
        /// <returns>membership user collection</returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <returns>number of users online</returns>
        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="username">user name</param>
        /// <param name="answer">answer to the selected question</param>
        /// <returns>user password</returns>
        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="providerUserKey">provider user key</param>
        /// <param name="userIsOnline">is user on line</param>
        /// <returns>membership user</returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="email">user email</param>
        /// <returns>user name</returns>
        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="username">user name</param>
        /// <param name="answer">answer to the selected question</param>
        /// <returns>user password</returns>
        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="userName">user name</param>
        /// <returns>unlock user</returns>
        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="user">update user</param>
        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="username">user name</param>
        /// <param name="password">user password</param>
        /// <returns>validate user</returns>
        public override bool ValidateUser(string username, string password)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}