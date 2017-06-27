(function ($) {
    "use strict";

    var lessthanRule = {
        RuleName: 'lessthan',
        Params: ['maxvalue', 'allowequality'],
        ValidationCallback: function (value, element, params) {
            var maxValue,
                currentValue;

            if (!params) {
                console.log("No items found in 'params'", this);

                return;
            } else if (!params.maxvalue) {
                console.log("'maxvalue' cannot be null or empty", this);

                return;
            }

            if (value) {
                currentValue = parseInt(value);
                maxValue = parseInt(params.maxvalue);

                if (params.allowequality) {
                    return currentValue <= maxValue;
                }
                else {
                    return currentValue < maxValue;
                }
            }

            return true;
        }
    }

    var greaterthanRule = {
        RuleName: 'greaterthan',
        Params: ['minvalue', 'allowequality'],
        ValidationCallback: function (value, element, params) {
            var minValue,
                currentValue;

            if (!params) {
                console.log("No items found in 'params'", this);

                return;
            } else if (!params.minvalue) {
                console.log("'minvalue' cannot be null or empty", this);

                return;
            }

            if (value) {

                currentValue = parseInt(value);
                minValue = parseInt(params.minvalue);

                if (params.allowequality) {

                    return currentValue >= minValue;
                } else {

                    return currentValue > minValue;
                }
            }

            return true;
        }
    };

    var comapreNumbersRule = {
        RuleName: 'comparenumbers',
        Params: ['dependencypropertyname', 'allowequality'],
        ValidationCallback: function (value, element, params) {
            var nameStructure = element.name.split('.'),
                dependencyFieldSelector = '',
                i,
                dependencyFieldValue,
                currentValue,
                dependencyValue;

            if (nameStructure.length >= 2) {
                // Will have to leave last item in the nameStructure, that is the property name for the "element".
                for (i = (nameStructure.length - 2) ; i >= 0; i -= 1) {
                    dependencyFieldSelector = nameStructure[i] + '.' + dependencyFieldSelector;
                }

                dependencyFieldSelector = dependencyFieldSelector + params.dependencypropertyname;
            }
            else {
                dependencyFieldSelector = params.dependencypropertyname;
            }

            if (!dependencyFieldSelector) {
                console.error("Could not find dependencyFieldSelector", this);

                return;
            }

            dependencyFieldValue = $('input[name="' + dependencyFieldSelector + '"]').val();

            if (dependencyFieldValue && value) {
                currentValue = parseFloat(value);
                dependencyValue = parseFloat(dependencyFieldValue);

                if (($(element).attr('name').toLowerCase().indexOf('begin') >= 0)
                    || ($(element).attr('name').toLowerCase().indexOf('start') >= 0)
                    || ($(element).attr('name').toLowerCase().indexOf('min') >= 0)) {
                    if (params.allowequality) {
                        if (currentValue > dependencyValue) {
                            return false;
                        }
                    } else {
                        if (currentValue >= dependencyValue) {
                            return false;
                        }
                    }
                } else {
                    if (params.allowequality) {
                        if (currentValue < dependencyValue) {
                            return false;
                        };
                    } else {
                        if (currentValue <= dependencyValue) {
                            return false;
                        };
                    }
                }
            }

            return true;
        }
    }

    var absoluteDifferenceLimitRule = {
        RuleName: 'absolutedifferencelimit',
        Params: ['dependencyproperty', 'ismaxthreshold', 'thresholdvalue'],
        ValidationCallback: function (value, element, params) {
            var nameStructure = element.name.split('.'),
               dependencyFieldSelector = '',
               i,
               dependencyFieldValueText,
               currentValue,
               difference,
               dependencyPropertyValue;

            if (!params) {
                console.log("'parmas' can't be null", this);

                return;

            } else if (!params.dependencyproperty) {
                console.log("'params.dependencypropertyname' can't be null", this);

                return;
            }

            if (nameStructure.length >= 2) {
                // Will have to leave last item in the nameStructure, that is the property name for the "element".
                for (i = (nameStructure.length - 2) ; i >= 0; i -= 1) {
                    dependencyFieldSelector = nameStructure[i] + '.' + dependencyFieldSelector;
                }

                dependencyFieldSelector = dependencyFieldSelector + params.dependencyproperty;
            }
            else {
                dependencyFieldSelector = params.dependencyproperty;
            }

            if (!dependencyFieldSelector) {
                console.error("Could not find dependencyFieldSelector", this);

                return;
            }

            dependencyFieldValueText = $('input[name="' + dependencyFieldSelector + '"]').val();

            if (dependencyFieldSelector && value) {

                currentValue = parseFloat(value);
                dependencyPropertyValue = parseFloat(dependencyFieldValueText);

                difference = Math.abs((currentValue - dependencyPropertyValue));

                return (params.ismaxthreshold && (difference <= params.thresholdvalue))
                       || (!params.ismaxthreshold && (difference >= params.thresholdvalue));
            }

            return true;
        }
    }

    var customValidationRules = (function (rules) {
        $.each(rules, function (index, value) {
            $.validator.unobtrusive.adapters.add(value.RuleName, value.Params, function (options) {
                options.rules[value.RuleName] = options.params;

                if (options.message) {
                    options.messages[value.RuleName] = options.message;
                }
            });

            $.validator.addMethod(value.RuleName, value.ValidationCallback, '');
        });

    }([greaterthanRule, comapreNumbersRule, absoluteDifferenceLimitRule, lessthanRule]));

}(jQuery));