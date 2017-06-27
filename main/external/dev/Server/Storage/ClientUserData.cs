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
    using Newtonsoft.Json;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage
    /// Class:          ClientUserData
    /// Description:    Model to hold user information got from provider
    /// ----------------------------------------------------------------- 
    [Serializable]
    public class ClientUserData
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("id")]
        public string ProviderUserId { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("link")]
        public Uri Link { get; set; }

        [JsonProperty("name")]
        public string UserName { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("updated_time")]
        public string UpdatedTime { get; set; }

        [JsonProperty("emails")]
        public Emails Emails { get; set; }

        [JsonProperty("personal")]
        public Address Personal { get; set; }

        [JsonProperty("business")]
        public Address Business { get; set; }

        public string AccessToken { get; set; }
    }
}
