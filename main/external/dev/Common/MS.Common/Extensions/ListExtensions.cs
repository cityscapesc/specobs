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

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Common
    /// Class:          ListExtensions
    /// Description:    extension method for list
    /// ----------------------------------------------------------------- 
    public static class ListExtensions
    {
        /// <summary>
        /// Moves items in the list
        /// </summary>
        /// <typeparam name="T">type of the list</typeparam>
        /// <param name="list">list</param>
        /// <param name="index">index of item to move to top</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Check.IsNotNull validates for null values")]
        public static void MoveItemAtIndexToFront<T>(this List<T> list, int index)
        {
            Check.IsNotNull(list, "list");

            if (list.Count > 0)
            {
                T item = list[index];
                list.RemoveAt(index);
                list.Insert(0, item);
            }
        }
    }
}
