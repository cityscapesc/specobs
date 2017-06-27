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
    using System.Globalization;
    using Microsoft.Spectrum.Common;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      MS.Storage.AzureTables
    /// Class:          WebpagesOAuthMembership (Entity Type:Private)
    /// Description:    WebpagesOAuthMembership Entity Class
    /// -----------------------------------------------------------------
    /// </summary>
    public class WebpagesOAuthMembership : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebpagesOAuthMembership"/> class
        /// </summary>
        public WebpagesOAuthMembership()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebpagesOAuthMembership"/> class
        /// </summary>
        /// <param name="userId"> provider user id </param>
        public WebpagesOAuthMembership(string providerUserId, string userId, string provider)
        {
            this.PartitionKey = Constants.DummyPartitionKey;
            this.RowKey = providerUserId + ":" + userId;
            this.Provider = provider;
        }

        /// <summary> 
        /// Gets the UserId of the WebpagesOAuthMembership. 
        /// </summary>
        public int UserId
        {
            get
            {
                string[] ids = this.RowKey.Split(':');
                return int.Parse(ids[1], CultureInfo.InvariantCulture);
            }
        }

        /// <summary> 
        /// Gets or sets the Provider of the WebpagesOAuthMembership. 
        /// </summary>
        public string Provider { get; set; }

        public string ProviderUserId
        {
            get
            {
                string[] ids = this.RowKey.Split(':');
                return ids[0];
            }
        }
    }
}
