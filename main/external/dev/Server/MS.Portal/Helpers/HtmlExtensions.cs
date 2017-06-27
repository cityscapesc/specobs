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

namespace Microsoft.Spectrum.Portal.Helpers
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Storage.Models;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Portal.Models
    /// Class:          HtmlExtensions
    /// Description:    Html helper extension methods
    /// ----------------------------------------------------------------- 
    public static class HtmlExtensions
    {
        /// <summary>
        /// supress lable string to 11 characters and add "..." at end
        /// </summary>
        /// <param name="helper">html helper</param>
        /// <param name="target">target string</param>
        /// <returns>formatted string</returns>
        public static string FormattedText(this HtmlHelper helper, string target)
        {
            if (target.Length > 12)
            {
                return string.Format(target.Substring(0, 11) + "...");
            }

            return target;
        }

        /// <summary>
        /// Adds * at the end mandatory field label
        /// </summary>
        /// <typeparam name="TModel">model</typeparam>
        /// <typeparam name="TProperty">property</typeparam>
        /// <param name="htmlHelper">html helper</param>
        /// <param name="expression">expression</param>
        /// <returns>html string</returns>
        public static IHtmlString MandatoryLabelFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            bool isFieldMandator = MandatoryField(htmlHelper, expression);
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            if (isFieldMandator)
            {
                return new HtmlString(htmlHelper.Encode(metadata.DisplayName));
            }
            else
            {
                return new HtmlString(htmlHelper.Encode(metadata.DisplayName));
            }
        }

        /// <summary>
        /// Creates lablel, textbox, div and validation message(if field is mandatory) controls for a given property
        /// </summary>
        /// <typeparam name="TModel">model</typeparam>
        /// <typeparam name="TProperty">property</typeparam>
        /// <param name="htmlHelper">html helper</param>
        /// <param name="expression">expression</param>
        /// <param name="isReadonly">is field readonly</param>
        /// <returns>mvc html string</returns>
        public static IHtmlString TextFieldsFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, bool isReadonly)
        {
            bool isFieldMandator = MandatoryField(htmlHelper, expression);
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            StringBuilder builder = new StringBuilder();
            string displayName = string.IsNullOrEmpty(metadata.DisplayName) ? metadata.PropertyName : metadata.DisplayName;

            if (isFieldMandator)
            {
                builder.Append(htmlHelper.Label(displayName));
                builder.Append(" <div class=\"input mandatory\">");
                builder.Append(GetTextBox(htmlHelper, expression, isReadonly));
                builder.Append(htmlHelper.ValidationMessageFor(expression, string.Empty, new { @class = "mandatory" }));
                builder.Append("</div>");
            }
            else
            {
                builder.Append(htmlHelper.Label(displayName));
                builder.Append(" <div class=\"input\">");
                builder.Append(GetTextBox(htmlHelper, expression, isReadonly));
                builder.Append("</div>");
            }

            return MvcHtmlString.Create(builder.ToString());
        }

        public static IHtmlString DropDownListFieldsFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string dropDownName = "")
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            StringBuilder builder = new StringBuilder();
            builder.Append(htmlHelper.Label(metadata.DisplayName));
            builder.Append(" <div class=\"input\">");
            builder.Append(htmlHelper.DropDownList(!string.IsNullOrEmpty(dropDownName) ? dropDownName : metadata.DisplayName));
            builder.Append("</div>");

            return MvcHtmlString.Create(builder.ToString());
        }

        public static string GetStationAddress(this HtmlHelper htmlHelper, MeasurementStationInfo stationInfo)
        {
            Address address = stationInfo.Address;

            if (address != null)
            {
                StringBuilder addressBuilder = new StringBuilder();

                if (!string.IsNullOrEmpty(address.AddressLine1))
                {
                    addressBuilder.Append(address.AddressLine1);
                    addressBuilder.Append(", ");
                }

                if (!string.IsNullOrEmpty(address.AddressLine2))
                {
                    addressBuilder.Append(address.AddressLine2);
                    addressBuilder.Append(", ");
                }

                if (!string.IsNullOrEmpty(address.Location))
                {
                    addressBuilder.Append(address.Location);
                    addressBuilder.Append(", ");
                }

                if (!string.IsNullOrEmpty(address.Country))
                {
                    addressBuilder.Append(address.Country);
                    addressBuilder.Append(", ");
                }

                return addressBuilder.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// This extension is used to display empty string instead of default date, if date field is not assigned
        /// </summary>
        /// <param name="htmlHelper">html helper</param>
        /// <param name="date">date value</param>
        /// <returns>date string</returns>
        public static string FormattedDate(this HtmlHelper htmlHelper, DateTime date)
        {
            if (date == default(DateTime))
            {
                return "NA";
            }

            return date.ToString();
        }

        public static string GetFrequencyRangesinMHz(this HtmlHelper htmlHelper, long startFrquency, long endFrequency)
        {
            return (long)MathLibrary.HzToMHz(startFrquency) + "-" + (long)MathLibrary.HzToMHz(endFrequency);
        }

        public static string PaginationString(this HtmlHelper htmlHelper, int pageIndex, int totalCount)
        {
            StringBuilder builder = new StringBuilder();

            if (totalCount > Constants.PageSize)
            {
                builder.Append((pageIndex * Constants.PageSize) + 1);
                builder.Append(" - ");
                builder.Append((pageIndex + 1) * Constants.PageSize);
                builder.Append(" of ");
                builder.Append(totalCount);
            }
            else
            {
                builder.Append(1);
                builder.Append(" - ");
                builder.Append(totalCount);
                builder.Append(" of ");
                builder.Append(totalCount);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Checks whether a field is mandatory or not
        /// </summary>
        /// <typeparam name="TModel">model</typeparam>
        /// <typeparam name="TProperty">property</typeparam>
        /// <param name="htmlHelper">html helper</param>
        /// <param name="expression">expression</param>
        /// <returns>result</returns>
        private static bool MandatoryField<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;

            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            if (memberExpression != null)
            {
                var required = memberExpression
                    .Member
                    .GetCustomAttributes(typeof(RequiredAttribute), false)
                    .Cast<RequiredAttribute>()
                    .FirstOrDefault();

                if (required != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets MVC Html string of text box with readonly property
        /// </summary>
        /// <typeparam name="TModel">model</typeparam>
        /// <typeparam name="TProperty">property</typeparam>
        /// <param name="htmlHelper">html helper</param>
        /// <param name="expression">expression</param>
        /// <param name="isReadOnly">readonly</param>
        /// <returns>mvc html string</returns>
        private static IHtmlString GetTextBox<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, bool isReadOnly)
        {
            if (isReadOnly)
            {
                return htmlHelper.TextBoxFor(expression, new { @readonly = true });
            }
            else
            {
                return htmlHelper.TextBoxFor(expression);
            }
        }
    }
}