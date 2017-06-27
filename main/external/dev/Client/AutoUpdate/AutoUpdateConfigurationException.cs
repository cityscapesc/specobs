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
    using System.Runtime.Serialization;
    using System.Text;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Custom exception class")]
    [Serializable]
    public class AutoUpdateConfigurationException : Exception
    {
        public AutoUpdateConfigurationException(string message, IEnumerable<ValidationResult> validationResults)
            : this(message, validationResults, null)
        {
        }

        public AutoUpdateConfigurationException(string message, IEnumerable<ValidationResult> validationResults, Exception innerException)
            : base(FormatMessage(message, validationResults), innerException)
        {
        }

        protected AutoUpdateConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

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