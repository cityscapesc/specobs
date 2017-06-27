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
    using System.Globalization;
    using System.Web.Mvc;
    using Microsoft.Spectrum.Portal.Helpers;
    using Microsoft.Spectrum.Portal.Models;
    using Microsoft.Spectrum.Portal.Security;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Table.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;    

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Portal.Controllers
    /// Class:          ProfileController
    /// Description:    controller that deals with User Profile
    /// -----------------------------------------------------------------
    public class ProfileController : Controller
    {
        private readonly UserManager userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileController"/> class
        /// </summary> 
        public ProfileController()
        {
            this.userManager = PortalGlobalCache.Instance.UserManager;
        }

        /// <summary>
        /// Fetch user details and display on the view
        /// </summary>
        /// <returns>view</returns>
        public ActionResult Index()
        {
            UserPrincipal userPrincipal = (UserPrincipal)this.User;
            UserInfo userInfo = this.userManager.GetUserInfoByAccessToken(userPrincipal.AccessToken);

            RegisterExternalLoginModel registerModel = new RegisterExternalLoginModel
            {
                AccountEmail = userInfo.User.AccountEmail,
                Address1 = userInfo.User.Address1,
                Address2 = userInfo.User.Address2,
                City = userInfo.User.Location,
                Country = userInfo.User.Country,
                FirstName = userInfo.User.FirstName,
                LastName = userInfo.User.LastName,
                Phone = userInfo.User.Phone,
                PhoneCountryCode = userInfo.User.PhoneCountryCode,
                PreferredEmail = userInfo.User.PreferredEmail,
                UserName = userInfo.User.UserName,
                TimeZone = userInfo.User.TimeZoneId,
                ZipCode = userInfo.User.ZipCode,
                State = userInfo.User.Region,
                SubscribeNotifications = userInfo.User.SubscribeNotifications
            };

            ViewBag.Country = new SelectList(Utility.GetCountries(), registerModel.Country);
            ViewBag.TimeZone = new SelectList(Utility.GetTimeZones(), registerModel.TimeZone);
            ViewBag.PhoneCountryCode = new SelectList(Utility.GetCountryPhoneCodes(), registerModel.PhoneCountryCode);

            return this.View(registerModel);
        }

        /// <summary>
        /// Update user details
        /// </summary>
        /// <param name="model">user details model</param>
        /// <returns>view</returns>
        [HttpPost]
        public ActionResult Update(RegisterExternalLoginModel model)
        {
            if (ModelState.IsValid)
            {
                UserPrincipal userPrincipal = (UserPrincipal)this.User;
                UserInfo userInfo = this.userManager.GetUserInfoByAccessToken(userPrincipal.AccessToken);

                if (userInfo != null)
                {                    
                    User user = new User(userInfo.User.UserId, userInfo.User.UserName, model.FirstName, model.LastName, model.City, model.State, model.TimeZone, model.PreferredEmail, userInfo.User.AccountEmail, userInfo.User.CreatedOn, System.DateTime.Now.ToString(CultureInfo.InvariantCulture), userInfo.User.Link, userInfo.User.Gender, model.Address1, model.Address2, model.Phone, model.Country, model.ZipCode, model.PhoneCountryCode,model.SubscribeNotifications);                    

                    userInfo = new UserInfo(user, null, null, null, null);

                    this.userManager.SaveUserInfo(userInfo);

                    return this.RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("UserName", "User not found");                    
                }
            }

            ViewBag.Country = new SelectList(Utility.GetCountries());
            ViewBag.TimeZone = new SelectList(Utility.GetTimeZones(), model.TimeZone);
            ViewBag.PhoneCountryCode = new SelectList(Utility.GetCountryPhoneCodes(), model.Country);

            return this.View(model);
        }
    }
}