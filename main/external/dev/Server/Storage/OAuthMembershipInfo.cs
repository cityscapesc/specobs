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
    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage
    /// Class:          OAuthMembershipInfo
    /// Description:    Model to hold membership information
    /// ----------------------------------------------------------------- 
    public class OAuthMembershipInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebpagesOAuthMembership"/> class
        /// </summary>
        /// <param name="userId"> user Id </param>
        public OAuthMembershipInfo(string providerUserId, string provider, int userId)
        {
            this.UserId = userId;
            this.Provider = provider;
            this.ProviderUserId = providerUserId;
        }

        /// <summary> 
        /// Gets the UserId of the WebpagesOAuthMembership. 
        /// </summary>
        public int UserId { get; set; }

        /// <summary> 
        /// Gets or sets the Provider of the WebpagesOAuthMembership. 
        /// </summary>
        public string Provider { get; set; }

        /// <summary> 
        /// Gets or sets the ProviderUserId of the WebpagesOAuthMembership. 
        /// </summary>
        public string ProviderUserId { get; set; }
    }
}
