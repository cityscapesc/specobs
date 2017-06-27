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

namespace Microsoft.Spectrum.Import.Service
{
    using System;
    using System.Configuration;
    using System.Globalization;

    internal class AppConfigConfigurationSource : IConfigurationSource
    {
        private readonly Lazy<DirectoryWatcherConfiguration> config = new Lazy<DirectoryWatcherConfiguration>(() =>
            {
                string configSectionName = "DirectoryWatcherConfiguration";

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                DirectoryWatcherConfigurationSection configSection = config.Sections.Get(configSectionName) as DirectoryWatcherConfigurationSection;
                
                if (configSection == null)
                {
                    string message = string.Format(
                        CultureInfo.InvariantCulture,
                        "The .exe.config file does not contain a {0} section. The settings could not be loaded and the service startup is aborting.",
                        configSectionName);
                    throw new InvalidOperationException(message);
                }

                // Replace the name of each environment with the string equivalent of the value of the variable, then return the resulting string
                configSection.WatchDirectory = Environment.ExpandEnvironmentVariables(configSection.WatchDirectory);
                configSection.InvalidFilesDirectory = Environment.ExpandEnvironmentVariables(configSection.InvalidFilesDirectory);

                return new DirectoryWatcherConfiguration(configSection);
            });

        public DirectoryWatcherConfiguration GetConfiguration()
        {
            return this.config.Value;
        }
    }
}