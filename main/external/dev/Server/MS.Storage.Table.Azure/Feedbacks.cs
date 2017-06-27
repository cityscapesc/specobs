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

namespace Microsoft.Spectrum.Storage.Table.Azure
{
    using System;
    using System.Globalization;
    using Microsoft.Spectrum.Common;
    using Microsoft.WindowsAzure.Storage.Table;    

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage.Table.Azure
    /// Class:          Feedbacks (Entity Type:Private)
    /// Description:    Feedbacks Entity Class
    /// -----------------------------------------------------------------
    /// </summary>
    public class Feedbacks : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Feedbacks"/> class
        /// </summary>
        public Feedbacks()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Feedbacks"/> class
        /// </summary>
        /// <param name="timeOfFeedback">Time of feedback</param>        
        public Feedbacks(DateTime timeOfFeedback)
        {
            this.PartitionKey = Constants.DummyPartitionKey;
            this.RowKey = timeOfFeedback.ToUniversalTime().Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);
        }

        public DateTime TimeOfFeedback
        {
            get
            {
                return new DateTime(long.Parse(this.RowKey, CultureInfo.InvariantCulture));
            }
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
        /// Gets or sets Read.
        /// </summary>
        public bool Read { get; set; }
    }
}
