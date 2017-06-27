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

namespace Microsoft.Spectrum.Portal.Filters
{
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Spectrum.Portal.Security;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:     Microsoft.Spectrum.Portal.Filters
    /// Class:         AuthorizeUserAttribute
    /// Description:   custom user authorization filter
    /// ----------------------------------------------------------------- 
    /// 
    public class AuthorizeUserAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// roles
        /// </summary>
        public string AccessLevels { get; set; }               

        /// <summary>
        /// redirect to custom authorization logic, called before default authorization logic by MVC infrastructure
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (this.AuthorizeCore(filterContext.HttpContext))
            {
                base.OnAuthorization(filterContext);
            }
            else
            {
                this.HandleUnauthorizedRequest(filterContext);
            }
        }

        /// <summary>
        /// custom authorization logic which checks roles of user
        /// </summary>
        /// <param name="httpContext">http context</param>
        /// <returns>is authorized </returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }

            if (string.IsNullOrEmpty(this.AccessLevels))
            {
                return true;
            }
            else
            {
                UserPrincipal principal = (UserPrincipal)httpContext.User;

                string[] roles = this.AccessLevels.Split(',');

                foreach (string role in roles)
                {
                    if (principal.IsInRole(role))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// handles unauthorized call and redirect back to calling url
        /// </summary>
        /// <param name="filterContext">filter context</param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {            
            filterContext.HttpContext.Response.Redirect(filterContext.HttpContext.Request.UrlReferrer.ToString());
        }
    }
}