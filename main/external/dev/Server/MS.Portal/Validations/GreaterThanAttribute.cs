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

namespace Microsoft.Spectrum.Portal.Validations
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Web.Mvc;

    [AttributeUsage(AttributeTargets.Property)]
    public class GreaterThanAttribute : ValidationAttribute, IClientValidatable
    {
        private const string ValiationType = "greaterthan";
        private const string AllowEqualityParamName = "allowequality";
        private const string MinValueParamName = "minvalue";

        public GreaterThanAttribute(int minValue, bool allowEquality)
        {
            this.MinValue = minValue;
            this.AllowEquality = allowEquality;
        }

        public int MinValue { get; private set; }

        public bool AllowEquality { get; private set; }

        public override bool IsValid(object value)
        {
            int currentValue;

            if (int.TryParse(value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out currentValue))
            {
                if (this.AllowEquality)
                {
                    return currentValue >= this.MinValue;
                }

                return currentValue > this.MinValue;
            }

            return false;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            string errorMessage = string.Format(CultureInfo.InvariantCulture, this.ErrorMessage, metadata.PropertyName, this.MinValue);

            ModelClientValidationRule clientValidationRule = new ModelClientValidationRule
            {
                ErrorMessage = errorMessage,
                ValidationType = ValiationType
            };

            clientValidationRule.ValidationParameters[AllowEqualityParamName] = this.AllowEquality ? "true" : string.Empty;
            clientValidationRule.ValidationParameters[MinValueParamName] = this.MinValue;

            yield return clientValidationRule;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, this.ErrorMessageString, name, this.MinValue);
        }
    }
}