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

    [Serializable]
    public class SettingsConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("settingsDirectory", IsRequired = true)]
        public string SettingsDirectory
        {
            get { return (string)base["settingsDirectory"]; }
        }

        [ConfigurationProperty("measurementStationConfigurationFile", IsRequired = true)]
        public string MeasurementStationConfigurationFile
        {
            get { return (string)base["measurementStationConfigurationFile"]; }
        }

        public string MeasurementStationConfigurationFileFullPath
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(this.SettingsDirectory) + "\\" + this.MeasurementStationConfigurationFile;
            }
        }
    }
}