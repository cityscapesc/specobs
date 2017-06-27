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
    using System;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:     Microsoft.Spectrum.Storage
    /// Class:          Feedback (Entity Type:Public)
    /// Description:    Feedbacks Model Class
    /// -----------------------------------------------------------------
    /// </summary>
    public class Feedback
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Feedback"/> class
        /// </summary>
        /// <param name="firstName">first name</param>
        /// <param name="lastName">last name</param>
        /// <param name="email">email</param>
        /// <param name="phone">phone</param>
        /// <param name="subject">subject</param>
        /// <param name="comment">comment</param>
        public Feedback(string firstName, string lastName, string email, string phone, string subject, string comment)
        {
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new ArgumentException("User FirstName can not be empty", "firstName");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("User email address can not be empty", "email");
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentException("Feedback subject can not be empty", "subject");
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                throw new ArgumentException("Feedback comments can not be empty", "comment");
            }

            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Phone = phone;
            this.Subject = subject;
            this.Comment = comment;
        }

        /// <summary>
        /// Gets or sets FirstName 
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets LastName 
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets Phone 
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets Subject 
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets Comment 
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets Read
        /// </summary>
        public bool Read { get; set; }

        /// <summary>
        /// Gets or sets TimeOfSubmission.
        /// </summary>
        public DateTime TimeOfSubmission { get; set; }
    }
}
