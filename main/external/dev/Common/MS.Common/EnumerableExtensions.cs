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

    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            foreach (T item in items)
            {
                action(item);
            }
        }

        /// <summary>
        /// Allows the creation of objects that represent ranges from an input sequence.
        /// </summary>
        /// <remarks>
        /// For example, if the input sequence is {1,2,3}, the range selector function could return a sequence of tuples of {(1,2), (2,3)}.
        /// In general if the input sequence contains N items, the output sequence will contain N-1 items. 
        /// This means that an input sequence of one item will return an empty output sequence.
        /// </remarks>
        public static IEnumerable<Tout> SelectRanges<Tin, Tout>(this IEnumerable<Tin> source, Func<Tin, Tin, Tout> rangeSelector)
        {
            using (IEnumerator<Tin> sourceEnumerator = source.GetEnumerator())
            {
                Tin previous;
                Tin current;

                // Get the first one
                sourceEnumerator.MoveNext();
                previous = sourceEnumerator.Current;

                // Now get the rest of them
                while (sourceEnumerator.MoveNext())
                {
                    current = sourceEnumerator.Current;
                    Tout range = rangeSelector(previous, current);
                    yield return range;
                    previous = current;
                }
            }
        }

        /// <summary>
        /// Ensures that the source collection contains any number of items that are all the same.
        /// If one or more items is different, an exception is thrown.
        /// </summary>
         public static T Only<T>(this IEnumerable<T> source)
        {
            return source.Only(EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Ensures that the source collection contains any number of items that are all the same.
        /// If one or more items is different, an exception is thrown.
        /// </summary>
        public static T Only<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentException("Source can not be null", "source");
            }

            if (comparer == null)
            {
                throw new ArgumentException("comparer can not be null", "comparer");
            }

            using (IEnumerator<T> sourceEnumerator = source.GetEnumerator())
            {
                T first;

                // Get the first one
                sourceEnumerator.MoveNext();
                first = sourceEnumerator.Current;

                // Compare the rest of them
                while (sourceEnumerator.MoveNext())
                {
                    T current = sourceEnumerator.Current;
                    if (!comparer.Equals(first, current))
                    {
                        throw new ArgumentException("The source sequence should contain all of the same items.");
                    }
                }

                return first;
            }
        }
    }
}