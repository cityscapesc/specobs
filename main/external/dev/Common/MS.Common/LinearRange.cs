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
    using System.Collections.Generic;

    /// <summary>
    /// Provides a base class for ranges that are represented by a number of linearly-spaced values between [start,stop)
    /// </summary>
    /// <remarks>
    /// These ranges have the benefit that [a,b) and [b,c) is continuous and non-overlapping on [a,c).
    /// </remarks>
    public abstract class LinearRange<TPoint, TInterval, TSubtype> : Range<TPoint>
        where TSubtype : LinearRange<TPoint, TInterval, TSubtype>
    {
        protected LinearRange(TPoint start, TPoint stop, int intervalCount)
            : base(start, stop, intervalCount)
        {
        }

        public abstract TInterval IntervalSize { get; }

        public abstract int FindIntervalIndex(TPoint point);

        public abstract IEnumerable<TSubtype> GetAllIntervals();

        public abstract IEnumerable<TSubtype> GetAllIntervals(int startIntervalIndex);

        public abstract override IEnumerable<TPoint> GetAllPointValues();

        public abstract override IEnumerable<TPoint> GetAllPointValues(int startIntervalIndex);

        public abstract override bool Contains(TPoint point);

        public abstract override bool Overlaps(Range<TPoint> other);
    }
}