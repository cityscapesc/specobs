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

namespace Microsoft.Spectrum.Common
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public static class ValidationHelper
    {
        public static bool TryValidate(object value, out IList<ValidationResult> results)
        {
            ValidationContext context = new ValidationContext(value, null, null);
            results = new List<ValidationResult>();

            return Validator.TryValidateObject(value, context, results, true);
        }

        public static IList<ValidationResult> Validate(object value)
        {
            IList<ValidationResult> results;
            TryValidate(value, out results);

            return results;
        }
    }
}