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
    using System.Diagnostics;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Common.Azure;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Portal.Filters
    /// Class:          CustomExceptionHandler
    /// Description:    custom error handler
    /// ----------------------------------------------------------------- 
    public class CustomExceptionHandler : HandleErrorAttribute
    {
        private readonly AzureLogger azureLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomExceptionHandler"/> class
        /// </summary>
        public CustomExceptionHandler()
        {
            this.azureLogger = new AzureLogger();
        }

        /// <summary>
        /// overrides onException method of base class
        /// </summary>
        /// <param name="filterContext">filter context</param>
        public override void OnException(ExceptionContext filterContext)
        {
            if (new HttpException(null, filterContext.Exception).GetHttpCode() != 500)
            {
                return;
            }

            if (!ExceptionType.IsInstanceOfType(filterContext.Exception))
            {
                return;
            }

            string referringUrl = "https://" + filterContext.HttpContext.Request.Url.Authority + "/" + (string)filterContext.RouteData.Values["controller"];

            if (filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                JsonResult errorResult = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                };

                errorResult.Data = new
                {
                    error = true,
                    message = filterContext.Exception.Message,
                    type = "Unknown"
                };

                filterContext.Result = errorResult;
            }
            else
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Error.cshtml"
                };

                base.OnException(filterContext);
            }

            this.azureLogger.Log(TraceEventType.Error, LoggingMessageId.PortalUnhandledError, filterContext.Exception.Message);

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}