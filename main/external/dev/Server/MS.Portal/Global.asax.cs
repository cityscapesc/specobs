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

namespace Microsoft.Spectrum.Portal
{
    using System;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using System.Web.Security;
    using Microsoft.Spectrum.Portal.Security;
    using Helpers;
    using System.Linq;

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            GlobalCacheInitializer.InitializeGlobalCache();
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie cookie = Request.Cookies["OAuthCookie"];

            if (cookie != null
                && cookie.Values.AllKeys.Any(key => string.Compare(key, "Expires", StringComparison.OrdinalIgnoreCase) == 0)
                && new DateTime(long.Parse(cookie.Values["Expires"])) >= DateTime.Now)
            {
                //if (authTicket.UserData == "OAuth")
                //{
                //    return;
                //}

                IPrincipal userPrincipal = new UserPrincipal(cookie.Values["ACCESS_TOKEN"], cookie.Values["PUID"]);
                HttpContext.Current.User = userPrincipal;
            }
        }
    }
}
