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

namespace Microsoft.Spectrum.Portal.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Portal.Models
    /// Class:          RegisterExternalLoginModel
    /// Description:    View model for registration
    /// ----------------------------------------------------------------- 
    public class RegisterExternalLoginModel
    {
        /// <summary>
        /// Gets or sets user name
        /// </summary>        
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets first name
        /// </summary>
        [Required(ErrorMessage = "Required")]
        [StringLength(50)]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets last name
        /// </summary>
        [StringLength(50)]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("TimeZone")]
        public string TimeZone { get; set; }

        /// <summary>
        /// Gets or sets email
        /// </summary>
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid Format")]
        [StringLength(100)]
        [DisplayName("Preferred Email")]
        public string PreferredEmail { get; set; }

        [DisplayName("Account Email ")]
        public string AccountEmail { get; set; }

        /// <summary>
        /// Gets or sets Address1
        /// </summary>
        [Required(ErrorMessage = "Required")]
        [DisplayName("Address Line 1")]
        public string Address1 { get; set; }

        /// <summary>
        /// Gets or sets Address2
        /// </summary>
        [DisplayName("Address Line 2")]
        public string Address2 { get; set; }

        /// <summary>
        /// Gets or sets City
        /// </summary>
        [Required(ErrorMessage = "Required")]
        [StringLength(60)]
        [DisplayName("City")]
        public string City { get; set; }

        /// <summary>
        /// Gets or sets State
        /// </summary>
        [Required(ErrorMessage = "Required")]
        [StringLength(60)]
        [DisplayName("State/Province/Region")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets State
        /// </summary>
        [Required(ErrorMessage = "Required")]
        [StringLength(60)]
        [DisplayName("Zip/Postal Code")]
        public string ZipCode { get; set; }

        /// <summary>
        /// Gets or sets country
        /// </summary>
        [DisplayName("Country")]
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets Country Code
        /// </summary>
        [DisplayName("Country Code")]
        public string PhoneCountryCode { get; set; }

        /// <summary>
        /// Gets or sets Phone
        /// </summary>
        [Required(ErrorMessage = "Required")]
        [Phone(ErrorMessage = "Invalid Phone number")]
        [DisplayName("Phone Number")]
        public string Phone { get; set; }

        public bool SubscribeNotifications { get; set; }

        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets External Login Data
        /// </summary>
        public string ExternalLoginData { get; set; }
    }
}