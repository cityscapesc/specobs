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

namespace Microsoft.Spectrum.Portal.Controllers
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Microsoft.Spectrum.Portal.Helpers;
    using Microsoft.Spectrum.Portal.Models;
    using Microsoft.Spectrum.Portal.Security;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Table.Azure;   

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Portal.Controllers
    /// Class:          CommonController
    /// Description:    controller that deals with common functions like faqs, feedback etc.
    /// ----------------------------------------------------------------- 
    [AllowAnonymous]
    public class CommonController : Controller
    {
        /// <summary>
        /// returns default view common controller
        /// </summary>
        /// <returns>view</returns>
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// returns partial view for each section
        /// </summary>
        /// <param name="sectionName">section name</param>
        /// <returns>partial view</returns>
        public PartialViewResult GetQuestions(string sectionName)
        {
            // This method is called multiple times parallely from view, in this case single instance of PortalTableOperations will throw exception.
            // So new instance is created every time instead of using PortalGlobalCache.Instance.PortalTableOperations
            IPortalTableOperations portalTableOperations = new PortalTableOperations(PortalGlobalCache.Instance.MasterAzureTableDbContext);
            PortalManager commonManager = new PortalManager(portalTableOperations);
            IEnumerable<Question> questions = commonManager.GetFrequentQuestionsAsync(sectionName);

            return this.PartialView("QuestionPartial", questions);
        }

        /// <summary>
        /// Gets default view of Feed back
        /// </summary>
        /// <returns>view</returns>
        public ActionResult FeedbackView()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                UserPrincipal userPrincipal = (UserPrincipal)this.User;
                UserInfo userInfo = PortalGlobalCache.Instance.UserManager.GetUserInfoByAccessToken(userPrincipal.AccessToken);

                if (userInfo != null)
                {
                    FeedbackViewModel model = new FeedbackViewModel
                    {
                        FirstName = userInfo.User.FirstName,
                        LastName = userInfo.User.LastName,
                        Email = userInfo.User.AccountEmail,
                        Phone = userInfo.User.Phone
                    };

                    return this.View("FeedbackView", model);
                }
            }

            return this.View("FeedbackView");
        }

        /// <summary>
        /// Saves Feed back
        /// </summary>
        /// <param name="model">feed back model</param>
        /// <returns>Feedback view</returns>
        public ActionResult SaveFeedback(FeedbackViewModel model)
        {
            if (ModelState.IsValid)
            {
                Feedback feedback = new Feedback(model.FirstName, model.LastName, model.Email, model.Phone, model.Subject, model.Comment);
                PortalManager commonManager = PortalGlobalCache.Instance.PortalManager;
                commonManager.SaveFeedback(feedback);
                this.ViewBag.Success = "Feedback saved successfully";
            }

            return this.View("FeedbackView", model);
        }

        /// <summary>
        /// Returns Privacy view
        /// </summary>
        /// <returns>Privacy view</returns>
        public ActionResult Privacy()
        {
            return this.View();
        }

        /// <summary>
        /// Returns Terms of Use view
        /// </summary>
        /// <returns>TermsOfUse view</returns>
        public ActionResult TermsOfUse()
        {
            return this.View();
        }

        /// <summary>
        /// Returns contact us view
        /// </summary>
        /// <returns>contact us view</returns>
        public ActionResult ContactUs()
        {
            return this.View();
        }

        /// <summary>
        /// Returns Contribute view
        /// </summary>
        /// <returns>Contribute view</returns>
        public ActionResult Contribute()
        {
            return this.View();
        }
    }
}