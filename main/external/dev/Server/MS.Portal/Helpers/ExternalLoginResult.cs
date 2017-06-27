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

namespace Microsoft.Spectrum.Portal.Helpers
{
    using System.Web.Mvc;
    using Microsoft.Web.WebPages.OAuth;

    /// <summary>
    /// External login result
    /// </summary>
    internal class ExternalLoginResult : ActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalLoginResult"/> class
        /// </summary>
        /// <param name="provider">provider name</param>
        /// <param name="returnUrl">return URL</param>
        public ExternalLoginResult(string provider, string returnUrl)
        {
            this.Provider = provider;
            this.ReturnUrl = returnUrl;
        }

        /// <summary>
        /// Gets Provider name
        /// </summary>
        public string Provider { get; private set; }

        /// <summary>
        /// Gets Return URL
        /// </summary>
        public string ReturnUrl { get; private set; }

        /// <summary>
        /// Request authentication for a given controller context
        /// </summary>
        /// <param name="context">controller context</param>
        public override void ExecuteResult(ControllerContext context)
        {
            OAuthWebSecurity.RequestAuthentication(this.Provider, this.ReturnUrl);
        }
    }
}