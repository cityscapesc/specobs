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
    using System.Collections.Generic;

    public class UserInfo
    {
        public UserInfo(User user, OAuthMembershipInfo membershipInfo, UserRole role, IEnumerable<UserRoleConfiguration> userRoleConfiguration, IEnumerable<UserRoleRequestConfiguration> userRoleRequestConfiguration)
        {
            this.User = user;
            this.UserRoleConfiguration = userRoleConfiguration;
            this.UserRoleRequestConfiguration = userRoleRequestConfiguration;
            this.MembershipInfo = membershipInfo;
            this.Role = role;
        }

        public User User { get; private set; }

        public OAuthMembershipInfo MembershipInfo { get; private set; }

        public UserRole Role { get; private set; }

        public IEnumerable<UserRoleConfiguration> UserRoleConfiguration { get; private set; }

        public IEnumerable<UserRoleRequestConfiguration> UserRoleRequestConfiguration { get; private set; }
    }
}
