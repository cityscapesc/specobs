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

    public class DirectoryWatcherConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("WatchDirectory")]
        public string WatchDirectory
        {
            get { return (string)this["WatchDirectory"]; }
            set { this["WatchDirectory"] = value; }
        }

        [ConfigurationProperty("InvalidFilesDirectory")]
        public string InvalidFilesDirectory
        {
            get { return (string)this["InvalidFilesDirectory"]; }
            set { this["InvalidFilesDirectory"] = value; }
        }

        [ConfigurationProperty("WatchFileFilter")]
        public string WatchFileFilter
        {
            get { return (string)this["WatchFileFilter"]; }
            set { this["WatchFileFilter"] = value; }
        }

        [ConfigurationProperty("StationAccessId")]
        public string StationAccessId
        {
            get { return (string)this["StationAccessId"]; }
            set { this["StationAccessId"] = value; }
        }

        [ConfigurationProperty("MeasurementStationServiceUri")]
        public string MeasurementStationServiceUri
        {
            get { return (string)this["MeasurementStationServiceUri"]; }
            set { this["MeasurementStationServiceUri"] = value; }
        }      

        [ConfigurationProperty("UploadRetryCount")]
        public int UploadRetryCount
        {
            get { return (int)this["UploadRetryCount"]; }
            set { this["UploadRetryCount"] = value; }
        }

        [ConfigurationProperty("ServerUploadTimeout")]
        public int ServerUploadTimeout
        {
            get { return (int)this["ServerUploadTimeout"]; }
            set { this["ServerUploadTimeout"] = value; }
        }

        [ConfigurationProperty("RetryDeltaBackoff")]
        public int RetryDeltaBackoff
        {
            get { return (int)this["RetryDeltaBackoff"]; }
            set { this["RetryDeltaBackoff"] = value; }
        }
    }
}
