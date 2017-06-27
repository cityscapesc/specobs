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
    using System.Linq;
    using System.Web;
    using DotNetOpenAuth.AspNet;
    using DotNetOpenAuth.AspNet.Clients;
    using Microsoft.Spectrum.Common;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Portal.Security
    /// Class:          MicrosoftOAuthClient
    /// Description:    custom microsoft oauth client
    /// ----------------------------------------------------------------- 
    public class MicrosoftOAuthClient : IAuthenticationClient
    {
        private readonly MicrosoftClient client;

        private Uri returnUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftOAuthClient"/> class
        /// </summary>
        /// <param name="clientId">provider client id</param>
        /// <param name="clientSecretId">provider secret client id</param>
        public MicrosoftOAuthClient(string clientId, string clientSecretId)
            : this(clientId, clientSecretId, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftOAuthClient"/> class
        /// </summary>
        /// <param name="clientId">provider client id</param>
        /// <param name="clientSecretId">provider secret client id</param>
        /// <param name="requestScopes">request scopes</param>
        public MicrosoftOAuthClient(string clientId, string clientSecretId, string requestScopes)
        {
            Check.IsNotNull(clientId, "ClientId");
            Check.IsNotNull(clientSecretId, "ClientSecretId");

            if (requestScopes != null && requestScopes.Count() <= 0)
            {
                this.client = new MicrosoftClient(clientId, clientSecretId);
            }
            else
            {
                var scopes = requestScopes.Split(',');
                this.client = new MicrosoftClient(clientId, clientSecretId, scopes);
            }
        }

        /// <summary>
        /// provider name
        /// </summary>
        public string ProviderName
        {
            get { return this.client != null ? this.client.ProviderName : PortalConstants.MicrosoftProvider; }
        }

        /// <summary>
        /// request for authentication
        /// </summary>
        /// <param name="context">http context</param>
        /// <param name="returnUrl">redirecting url after authentication</param>
        public void RequestAuthentication(HttpContextBase context, Uri returnUrl)
        {
            this.returnUrl = returnUrl;
            this.client.RequestAuthentication(context, returnUrl);
        }

        /// <summary>
        /// verify authentication
        /// </summary>
        /// <param name="context">http context</param>
        /// <returns>authentication result</returns>
        public AuthenticationResult VerifyAuthentication(HttpContextBase context)
        {
            if (this.returnUrl == null || !this.returnUrl.IsWellFormedOriginalString())
            {
                throw new UriFormatException("Invalid return url");
            }

            return this.client.VerifyAuthentication(context, this.returnUrl);
        }
    }
}