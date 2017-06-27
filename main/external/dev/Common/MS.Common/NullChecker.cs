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
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Common
    /// Class:          NullChecker
    /// Description:    check null values of passed type 
    /// ----------------------------------------------------------------- 
    public static class NullChecker<T> where T : class
    {
        private static readonly List<Func<T, bool>> Checkers = new List<Func<T, bool>>();

        private static readonly List<string> Names = new List<string>();

        static NullChecker()
        {
            try
            {
                // We can't rely on the order of the properties, but we 
                // can rely on the order of the constructor parameters 
                // in an anonymous type - and that there'll only be 
                // one constructor. 
                foreach (string name in typeof(T).GetConstructors()[0]
                                                 .GetParameters()
                                                 .Select(p => p.Name))
                {
                    Names.Add(name);
                    PropertyInfo property = typeof(T).GetProperty(name);
                    ParameterExpression param = Expression.Parameter(typeof(T), "container");
                    Expression propertyAccess = Expression.Property(param, property);
                    Expression nullValue = Expression.Constant(null, property.PropertyType);
                    Expression equality = Expression.Equal(propertyAccess, nullValue);
                    var lambda = Expression.Lambda<Func<T, bool>>(equality, param);
                    Checkers.Add(lambda.Compile());
                }
            }
            catch
            {
                throw;
            }
        }

        internal static void Check(T item)
        {
            for (int i = 0; i < Checkers.Count; i++)
            {
                if (Checkers[i](item))
                {
                    throw new ArgumentNullException(Names[i]);
                }
            }
        }
    }
}
