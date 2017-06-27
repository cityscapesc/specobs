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
    public class CompareNumbersAttribute : ValidationAttribute, IClientValidatable
    {
        private const string ValidationType = "comparenumbers";
        private const string DependencyPropertyParamName = "dependencypropertyname";
        private const string AllowEqualityParamName = "allowequality";

        public CompareNumbersAttribute(string dependentPropertyName, bool allowEquality)
        {
            if (string.IsNullOrWhiteSpace(dependentPropertyName))
            {
                throw new ArgumentException("DependentPropertyName can not be empty", dependentPropertyName);
            }

            this.DependentPropertyName = dependentPropertyName;
            this.AllowEquality = allowEquality;
        }

        public string DependentPropertyName { get; private set; }

        public bool AllowEquality { get; private set; }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            ModelClientValidationRule clientValidationRule = new ModelClientValidationRule
            {
                ErrorMessage = this.ErrorMessage,
                ValidationType = ValidationType
            };

            clientValidationRule.ValidationParameters[DependencyPropertyParamName] = this.DependentPropertyName;
            clientValidationRule.ValidationParameters[AllowEqualityParamName] = this.AllowEquality ? "true" : string.Empty;

            yield return clientValidationRule;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult validationResult = ValidationResult.Success;
            object dependencyPropertyValue = validationContext.ObjectType.GetProperty(this.DependentPropertyName);

            if (value != null)
            {
                double currentValue;
                bool valueExists = double.TryParse(value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out currentValue)
                                    && (dependencyPropertyValue != null);

                if (valueExists)
                {
                    double valueToBeCompared;
                    string dependencyValue = dependencyPropertyValue.ToString();

                    if (double.TryParse(dependencyValue, NumberStyles.Number, CultureInfo.InvariantCulture, out valueToBeCompared))
                    {
                        // To identify start value of the Dependency property with which current value is compared against.
                        bool isLowerBoundField = !dependencyValue.ToLower(CultureInfo.InvariantCulture).Contains("begin")
                                                 || !dependencyValue.ToLower(CultureInfo.InvariantCulture).Contains("start")
                                                 || !dependencyValue.ToLower(CultureInfo.InvariantCulture).Contains("min");

                        bool isNotValid = (isLowerBoundField && (currentValue > valueToBeCompared))
                                           || (currentValue < valueToBeCompared)
                                           || ((currentValue == valueToBeCompared) && !this.AllowEquality);

                        if (isNotValid)
                        {
                            validationResult = new ValidationResult(this.ErrorMessage);
                        }
                    }
                }
            }

            return validationResult;
        }
    }
}