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
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using Microsoft.Spectrum.Common;

    public class AutoUpdateConfiguration : IValidatableObject
    {
        public AutoUpdateConfiguration(AutoUpdateConfigurationSection section)
        {
            if (section == null)
            {
                throw new ArgumentNullException("section");
            }

            this.MsiDownloadPath = section.MsiDownloadPath;
            this.UpdateInterval = section.UpdateInterval;
            this.ServiceInstallerConfigUri = section.ServiceInstallerConfigUri;
        }

        // Just need this since the C# type system doesn't support varied type parameters (e.g. Action<string, params object[]>)
        private delegate void AddValidationResult(string format, params object[] args);

        public string MsiDownloadPath { get; set; }

        public double UpdateInterval { get; set; }

        public Uri ServiceInstallerConfigUri { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = new List<ValidationResult>();
            AddValidationResult addResult = (format, args) => results.Add(new ValidationResult(string.Format(CultureInfo.InvariantCulture, format, args)));

            if (!Directory.Exists(this.MsiDownloadPath))
            {
                Directory.CreateDirectory(this.MsiDownloadPath);
            }

            if (!FileHelper.DirectoryHasPermission(this.MsiDownloadPath, FileIOPermissionAccess.Read))
            {
                addResult("This process does not have read permission for the invalid files directory ({0})", this.MsiDownloadPath);
            }

            if (!FileHelper.DirectoryHasPermission(this.MsiDownloadPath, FileIOPermissionAccess.Write))
            {
                addResult("This process does not have write permission for the staging directory ({0})", this.MsiDownloadPath);
            }

            return results;
        }
    }
}
