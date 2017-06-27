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

namespace Microsoft.Spectrum.AutoUpdate
{
    using System;
    using System.Configuration;
    using System.Globalization;

    public class AppConfigConfigurationSource : IConfigurationSource
    {
        private readonly Lazy<AutoUpdateConfiguration> config = new Lazy<AutoUpdateConfiguration>(() =>
        {
            string configSectionName = "AutoUpdateConfiguration";

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            AutoUpdateConfigurationSection configSection = config.Sections.Get(configSectionName) as AutoUpdateConfigurationSection;

            if (configSection == null)
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "The config file does not contain a {0} section. The settings could not be loaded and the service startup is aborting.",
                    configSectionName);

                throw new InvalidOperationException(message);
            }

            // Replace the name of each environment with the string equivalent of the value of the variable, then return the resulting string
            configSection.MsiDownloadPath = Environment.ExpandEnvironmentVariables(configSection.MsiDownloadPath);

            return new AutoUpdateConfiguration(configSection);
        });

        public AutoUpdateConfiguration GetConfiguration()
        {
            return this.config.Value;
        }
    }
}
