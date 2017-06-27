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
using System.Collections.Concurrent;

    // TODO: Consider moving this to the .Collections subnamespace

    /// <summary>
    /// Provides a concurrent dictionary implementation that calls a valueFactory delegate in a thread-safe way.
    /// </summary>
    public class LazyConcurrentDictionary<TKey, TValue>
    {
        private ConcurrentDictionary<TKey, Lazy<TValue>> dictionary = new ConcurrentDictionary<TKey, Lazy<TValue>>();

        public LazyConcurrentDictionary()
        {
        }

        /// <returns>Gets the atomic entry or creates new if it does not exist.</returns>
        public TValue GetOrAddAtomic(TKey key, Func<TKey, TValue> valueFactory)
        {
            // NB: The ConcurrentDictionary does not guarantee that the valueFactory delegate is only invoked once, so 
            // we must wrap that in a Lazy<T> (which is thread-safe).
            return this.dictionary.GetOrAdd(
                key,
                (k) =>
            {
                return new Lazy<TValue>(() =>
                {
                    TValue value = valueFactory(k);
                    return value;
                });
            }).Value;
        }

        public void Clear()
        {
            this.dictionary.Clear();
        }
    }
}