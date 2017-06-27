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
    using System;
    using System.Globalization;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Common
    /// Class:          Check
    /// Description:    Generic null checker
    /// ----------------------------------------------------------------- 
    public static class Check
    {
        /// <summary>
        /// Checks is not null
        /// </summary>
        /// <typeparam name="T">type of generic type</typeparam>
        /// <param name="value">parameter value</param>
        /// <param name="parameterName">parameter name</param>
        /// <returns>is null</returns>
        public static T IsNotNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        /// <summary>
        /// check not null
        /// </summary>
        /// <typeparam name="T">type of parameter</typeparam>
        /// <param name="objectToCheck">checking object</param>
        public static void CheckNotNull<T>(this T objectToCheck) where T : class
        {
            if (objectToCheck == null)
            {
                throw new ArgumentNullException(objectToCheck.GetType().ToString());
            }

            NullChecker<T>.Check(objectToCheck);
        }

        /// <summary>
        /// check string values is not null
        /// </summary>
        /// <param name="value">parameter value</param>
        /// <param name="parameterName">parameter name</param>
        /// <returns></returns>
        public static string IsNotEmptyOrWhiteSpace(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                        string.Format(CultureInfo.InvariantCulture, "{0} is Null or Whitespace which are invalid", parameterName));
            }

            return value;
        }
    }
}
