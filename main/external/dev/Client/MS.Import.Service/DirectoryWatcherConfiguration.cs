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
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using Microsoft.Spectrum.Common;

    /// <summary>
    /// Provides a read-only wrapper class for the configuration information loaded from the .exe.config file.
    /// </summary>
    internal class DirectoryWatcherConfiguration : IValidatableObject
    {         
        private DirectoryWatcherConfigurationSection currentSection;

        /// <summary>
        /// Gets the Configuration from app.Config File
        /// </summary>
        public DirectoryWatcherConfiguration(DirectoryWatcherConfigurationSection section)
        {
            this.WatchDirectory = section.WatchDirectory;
            this.InvalidFilesDirectory = section.InvalidFilesDirectory;
            this.WatchDirectoryFileExtension = section.WatchFileFilter;
            this.StationAccessId = section.StationAccessId;
            this.MeasurementStationServiceUri = section.MeasurementStationServiceUri;
            this.UploadRetryCount = section.UploadRetryCount;
            this.ServerUploadTimeout = section.ServerUploadTimeout;
            this.RetryDeltaBackoff = section.RetryDeltaBackoff;
            this.currentSection = section;
        }

        // Just need this since the C# type system doesn't support variadic type parameters (e.g. Action<string, params object[]>)
        private delegate void AddValidationResult(string format, params object[] args);

        public string WatchDirectory { get; private set; }

        public string InvalidFilesDirectory { get; private set; }

        public string WatchDirectoryFileExtension { get; private set; }

        public string StationAccessId { get; private set; }

        public string MeasurementStationServiceUri { get; private set; }

        public int UploadRetryCount { get; private set; }

        public int ServerUploadTimeout { get; private set; }

        public int RetryDeltaBackoff { get; private set; }

        /// <returns>
        /// A collection that holds failed-validation information.
        /// </returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            AddValidationResult addResult = (format, args) => results.Add(new ValidationResult(string.Format(CultureInfo.InvariantCulture, format, args)));

            // Watch directory validation
            if (!Directory.Exists(this.WatchDirectory))
            {
                addResult("Could not find the specified watch directory ({0}). Create the directory if it does not exist.", this.WatchDirectory);
            }

            if (!FileHelper.DirectoryHasPermission(this.WatchDirectory, FileIOPermissionAccess.Read))
            {
                addResult("This process does not have read permission for the watch directory ({0})", this.WatchDirectory);
            }            

            // Errors directory validation
            if (!Directory.Exists(this.InvalidFilesDirectory))
            {
                Directory.CreateDirectory(this.InvalidFilesDirectory);
            }

            if (!FileHelper.DirectoryHasPermission(this.InvalidFilesDirectory, FileIOPermissionAccess.Read))
            {
                addResult("This process does not have read permission for the invalid files directory ({0})", this.InvalidFilesDirectory);
            }

            if (!FileHelper.DirectoryHasPermission(this.InvalidFilesDirectory, FileIOPermissionAccess.Write))
            {
                addResult("This process does not have write permission for the invalid files directory ({0})", this.InvalidFilesDirectory);
            }

            // Watch file filter            
            if (string.IsNullOrEmpty(this.WatchDirectoryFileExtension) || !this.WatchDirectoryFileExtension.StartsWith(@"*.", StringComparison.OrdinalIgnoreCase))
            {
                addResult("The specified watch file filter ({0}) is invalid. Use a string like '*.bin' to specify the file extension.", this.WatchDirectoryFileExtension);
            }

            // Station access ID
            if (string.IsNullOrEmpty(this.StationAccessId))
            {
                addResult("The station access ID ({0}) is missing or invalid.", this.StationAccessId);
            }

            // Upload service URI
            Uri unused;
            if (string.IsNullOrEmpty(this.MeasurementStationServiceUri) || !Uri.TryCreate(this.MeasurementStationServiceUri, UriKind.Absolute, out unused))
            {
                addResult("The upload service URI ({0}) is missing or not a valid URI.", this.MeasurementStationServiceUri);
            }

            return results;
        }

        /// <summary>
        /// This method is used to encrypt the section in the application config and save it
        /// </summary>
        public void Save()
        {
            // Only turn this on when we are ready to ship, it will make testing harder
            /*
            if (!this.currentSection.SectionInformation.IsProtected)
            {
                this.currentSection.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
                this.currentSection.SectionInformation.ForceSave = true;
            }
            */

            this.currentSection.CurrentConfiguration.Save(ConfigurationSaveMode.Modified);
        }
    }
}
