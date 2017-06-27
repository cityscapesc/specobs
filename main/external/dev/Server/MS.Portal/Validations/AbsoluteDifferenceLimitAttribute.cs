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
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    public class AbsoluteDifferenceLimitAttribute : ValidationAttribute, IClientValidatable
    {
        private const string ValidationType = "absolutedifferencelimit";
        private const string DependencyPropertyParamName = "dependencyproperty";
        private const string IsMaxDifferenceParamName = "ismaxthreshold";
        private const string DifferenceValueParamName = "thresholdvalue";

        public AbsoluteDifferenceLimitAttribute(double thresholdValue, string dependencyProperty, bool isMaxThreshold = true)
        {
            if (string.IsNullOrWhiteSpace(dependencyProperty))
            {
                throw new ArgumentException("DependencyProperty Name can't be empty", "dependencyProperty");
            }

            this.ThresholdValue = thresholdValue;
            this.DependencyPropertyName = dependencyProperty;
            this.IsMaxThreshold = isMaxThreshold;
        }

        public double ThresholdValue { get; private set; }

        public string DependencyPropertyName { get; private set; }

        public bool IsMaxThreshold { get; set; }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            ModelClientValidationRule clientValidationRule = new ModelClientValidationRule
            {
                ErrorMessage = this.ErrorMessage,
                ValidationType = ValidationType
            };

            clientValidationRule.ValidationParameters[DependencyPropertyParamName] = this.DependencyPropertyName;
            clientValidationRule.ValidationParameters[IsMaxDifferenceParamName] = this.IsMaxThreshold ? "true" : string.Empty;
            clientValidationRule.ValidationParameters[DifferenceValueParamName] = this.ThresholdValue;

            yield return clientValidationRule;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ValidationResult validationResult = ValidationResult.Success;
            object dependencyPropertyValue = validationContext.ObjectType.GetProperty(this.DependencyPropertyName);

            if (value != null)
            {
                double currentValue;
                bool valueExists = double.TryParse(value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out currentValue)
                                    && (dependencyPropertyValue != null);

                if (valueExists)
                {
                    double valueToBeComparedWith;
                    string dependencyValue = dependencyPropertyValue.ToString();

                    if (double.TryParse(dependencyValue, NumberStyles.Number, CultureInfo.InvariantCulture, out valueToBeComparedWith))
                    {
                        double difference = Math.Abs(currentValue - valueToBeComparedWith);

                        bool isInvalid = (this.IsMaxThreshold && (difference > this.ThresholdValue))
                                         || (!this.IsMaxThreshold && (difference < this.ThresholdValue));

                        if (isInvalid)
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