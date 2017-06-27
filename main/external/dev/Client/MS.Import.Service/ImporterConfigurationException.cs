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
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    public class ImporterConfigurationException : Exception
    {
        public ImporterConfigurationException(string message, IEnumerable<ValidationResult> validationResults)
            : this(message, validationResults, null)
        {
        }

        public ImporterConfigurationException(string message, IEnumerable<ValidationResult> validationResults, Exception innerException)
            : base(FormatMessage(message, validationResults), innerException)
        {
            if (validationResults == null)
            {
                this.ValidationResults = Enumerable.Empty<ValidationResult>();
            }
            else
            {
                this.ValidationResults = validationResults;
            }
        }

        protected ImporterConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public IEnumerable<ValidationResult> ValidationResults { get; private set; }

        private static string FormatMessage(string message, IEnumerable<ValidationResult> validationResults)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine("Validation results:");

            foreach (ValidationResult validationResult in validationResults)
            {
                sb.AppendLine(validationResult.ErrorMessage);
            }

            return sb.ToString();
        }
    }
}