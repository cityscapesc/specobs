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

namespace Microsoft.Spectrum.DeploymentHelper
{
    using System;
    using System.Configuration;

    [Serializable]
    public class SubscriptionConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("ServiceManagementUrl", IsRequired = true)]
        public string ServiceManagementUrl
        {
            get { return (string)base["ServiceManagementUrl"]; }
        }

        [ConfigurationProperty("Id", IsRequired = true)]
        public string Id
        {
            get { return (string)base["Id"]; }
        }

        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get { return (string)base["Name"]; }
        }

        [ConfigurationProperty("ManagementCertificate", IsRequired = true)]
        public string ManagementCertificate
        {
            get { return (string)base["ManagementCertificate"]; }
        }
    }
}