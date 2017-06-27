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
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Security;
    using DotNetOpenAuth.AspNet;
    using Microsoft.Spectrum.Portal.Helpers;
    using Microsoft.Spectrum.Portal.Models;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using Microsoft.Web.WebPages.OAuth;
    using System.Security.Principal;
    using WebMatrix.WebData;    /// <summary>
                                /// -----------------------------------------------------------------
                                /// Namespace:      Microsoft.Spectrum.Portal.Controllers
                                /// Class:          AccountController
                                /// Description:    controller that deals with authentication
                                /// ----------------------------------------------------------------- 
    [AllowAnonymous]
    public class AccountController : Controller
    {
        /// <summary>
        /// property to hold instance of <see cref="IAzureTableOperation"/>
        /// </summary>
        private readonly UserManager userManager;

        private string tempUrl = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class
        /// </summary>
        public AccountController()
        {
            this.userManager = PortalGlobalCache.Instance.UserManager;
        }

        /// <summary>
        /// Log off authenticated user
        /// </summary>
        /// <returns>redirect to specified action</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();
            System.Web.Security.FormsAuthentication.SignOut();
            HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);

            Session.Abandon();
            if (Request.Cookies["OAuthCookie"] != null)
            {
                HttpCookie myCookie = new HttpCookie("OAuthCookie");
                myCookie.Values.Clear();
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }

            // https://login.live.com/oauth20_logout.srf?client_id=[CLIENT_ID]&redirect_uri=[REDIRECT_URL]

            return this.RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// It will redirect the site for external login like Microsoft for authentication
        /// </summary>
        /// <param name="returnUrlParam">URL to return back after authentication</param>
        /// <returns>redirects to external login result</returns>        
        public ActionResult Login(string returnUrlParam)
        {
            if (string.IsNullOrEmpty(returnUrlParam))
            {
                returnUrlParam = HttpContext.Request.QueryString["ReturnUrl"];
            }

            return new ExternalLoginResult(OAuthWebSecurity.RegisteredClientData.FirstOrDefault().AuthenticationClient.ProviderName, Url.Action("ExternalLoginCallback", new { returnUrl = returnUrlParam }));
        }

        /// <summary>
        /// It will redirect the site for external login like Microsoft for authentication
        /// </summary>
        /// <param name="provider">provider name</param>
        /// <param name="returnUrl">URL to return back after authentication</param>
        /// <returns>redirects to external login result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = HttpContext.Request.UrlReferrer.ToString();
            }

            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { redirect_uri = returnUrl }));
        }

        /// <summary>
        /// External provider will call this method once authentication is done
        /// </summary>
        /// <param name="returnUrl">URL to return back after authentication</param>
        /// <returns>associated view</returns>
        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(returnUrl);

            if (!result.IsSuccessful)
            {
                if (result.Error != null)
                {
                    PortalGlobalCache.Instance.Logger.Log(TraceEventType.Error, Common.LoggingMessageId.PortalUnhandledError, string.Format("User could not login - Provider: {0}, UserName{1}, UserId {2}, Exception: {3}", result.Provider, result.UserName, result.ProviderUserId, result.Error.ToString()));
                }
                else
                {
                    PortalGlobalCache.Instance.Logger.Log(TraceEventType.Error, Common.LoggingMessageId.PortalUnhandledError, string.Format("User could not login - Provider: {0}, UserName{1}, UserId {2}", result.Provider, result.UserName, result.ProviderUserId));
                }

                return this.RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                this.CreateAuthenticationTicket(result.ExtraData["id"].ToString(CultureInfo.InvariantCulture), result.ExtraData["accesstoken"].ToString(CultureInfo.InvariantCulture), DateTime.Now);

                return this.RedirectToLocal(returnUrl);
                //// return this.Redirect(redirectUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                this.CreateAuthenticationTicket(result.ExtraData["id"], result.ExtraData["accesstoken"].ToString(CultureInfo.InvariantCulture), DateTime.Now);

                return this.RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for them to register
                string accessToken = result.ExtraData["accesstoken"].ToString(CultureInfo.InvariantCulture);
                var userData = UserManager.GetClientUserData(accessToken);
                userData.AccessToken = accessToken;

                this.TempData["ClientUserData"] = userData;

                RegisterExternalLoginModel registerModel = new RegisterExternalLoginModel
                {
                    FirstName = userData.FirstName,
                    LastName = userData.LastName,
                    UserName = string.IsNullOrEmpty(userData.UserName) ? userData.Emails.Account : userData.UserName,
                    AccountEmail = userData.Emails.Account,
                    PreferredEmail = userData.Emails.Preferred,
                    City = userData.Business != null ? userData.Business.City : string.Empty,
                    ExternalLoginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId),
                };

                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.Country = new SelectList(Utility.GetCountries());
                ViewBag.TimeZone = new SelectList(Utility.GetTimeZones(), "(UTC-08:00) Pacific Time (US & Canada)");
                ViewBag.PhoneCountryCode = new SelectList(Utility.GetCountryPhoneCodes(), "United States(+1)");
                return this.View("Register", registerModel);
            }
        }

        /// <summary>
        /// New user registration  
        /// </summary>
        /// <param name="model">registration info</param>
        /// <param name="returnUrl">return back URL after completion of registration</param>
        /// <returns>associated view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return this.RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                UserInfo userInfo = this.userManager.GetUserDetailsByProviderUserId(providerUserId);

                if (userInfo == null)
                {
                    ClientUserData clientUserData = (ClientUserData)TempData["ClientUserData"];

                    Random random = new Random();
                    int userId = random.Next(int.MaxValue);
                    var userName = clientUserData.Emails.Account;
                    string link = clientUserData.Link != null ? clientUserData.Link.ToString() : string.Empty;

                    User user = new User(userId, userName, model.FirstName, model.LastName, model.City, model.State, model.TimeZone, model.PreferredEmail, clientUserData.Emails.Account, System.DateTime.Now.ToString(CultureInfo.InvariantCulture), System.DateTime.Now.ToString(CultureInfo.InvariantCulture), link, clientUserData.Gender, model.Address1, model.Address2, model.Phone, model.Country, model.ZipCode, model.PhoneCountryCode,model.SubscribeNotifications);

                    // [Note]: We don't have to set user role and persist them into database.The user role is only valid for those user who have station 
                    // registrations.
                    userInfo = new UserInfo(user, null, null, null, null);

                    this.userManager.SaveUserInfo(userInfo);

                    OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, userName);
                    OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                    //// By Default IsSuperAdmin is false
                    this.CreateAuthenticationTicket(providerUserId, clientUserData.AccessToken, DateTime.Now);
                    return this.RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("UserName", "User already exists. Please enter a different user name.");
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Country = new SelectList(Utility.GetCountries());
            ViewBag.TimeZone = new SelectList(Utility.GetTimeZones());
            ViewBag.PhoneCountryCode = new SelectList(Utility.GetCountryPhoneCodes());
            return this.View("Register", model);
        }

        /// <summary>
        /// called by external authentication provider after authentication is failed
        /// </summary>
        /// <returns>external login failure view</returns>
        public ActionResult ExternalLoginFailure()
        {
            return this.View();
        }

        #region Helpers

        /// <summary>
        /// returns code based on membership create status
        /// </summary>
        /// <param name="createStatus">membership create status</param>
        /// <returns>error message</returns>
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        /// <summary>
        /// Checks URL and redirect to given URL
        /// </summary>
        /// <param name="returnUrl">URL to which it should be redirected</param>
        /// <returns>redirect to the passing URL or default action</returns>
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return this.Redirect(returnUrl);
            }
            else
            {
                return this.RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// creates authentication ticket
        /// </summary>
        /// <param name="providerUserId">OAuth provider user id</param>
        /// <param name="accessToken">access token for the user</param>
        /// <param name="tokenCreatedTime">time the token was created</param>
        /// <param name="isSuperAdmin">is this is super admin</param>
        /// <param name="isRegionAdmin">is this a region admin</param>
        private void CreateAuthenticationTicket(string providerUserId, string accessToken, DateTime tokenCreatedTime)
        {
            //var userData = accessToken;

            //FormsAuthenticationTicket authenticationTicket = new FormsAuthenticationTicket(1, providerUserId, DateTime.Now, DateTime.Now.AddHours(12), false, null);
            //string encryptedTicket = FormsAuthentication.Encrypt(authenticationTicket);

            HttpCookie cookie = new HttpCookie("OAuthCookie");
            cookie.Values.Add("PUID", providerUserId);
            cookie.Values.Add("ACCESS_TOKEN", accessToken);
            cookie.Values.Add("Expires", DateTime.Now.AddHours(12).Ticks.ToString());

            cookie.Expires = DateTime.Now.AddHours(12);

            Response.Cookies.Add(cookie);
        }

        #endregion
    }
}