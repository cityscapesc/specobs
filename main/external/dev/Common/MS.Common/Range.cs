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
    using System.Globalization;

    /// <summary>
    /// Provides a base class for ranges that are represented values between a start value and a stop value.
    /// The range represents a closed-open interval - the start value is included, but the stop value is excluded.
    /// </summary>
    /// <remarks>
    /// In mathematical notation, the interval is [start,stop).
    /// These ranges have the benefit that [a,b) and [b,c) is continuous and non-overlapping on [a,c).
    /// </remarks>
    /// <typeparam name="TPoint">The type of the point values contained in the range.</typeparam>
    /// <typeparam name="TPoint">The type of the interval values between the points in the range.</typeparam>
    public abstract class Range<TPoint>
    {
        protected Range(TPoint start, TPoint stop, int intervalCount)
        {
            this.Start = start;
            this.Stop = stop;
            this.IntervalCount = intervalCount;
        }

        public TPoint Start { get; private set; }

        public TPoint Stop { get; private set; }

        public int IntervalCount { get; private set; }
        
        public abstract IEnumerable<TPoint> GetAllPointValues();

        public abstract IEnumerable<TPoint> GetAllPointValues(int startIntervalIndex);

        public abstract bool Contains(TPoint point);

        public abstract bool Overlaps(Range<TPoint> other);

        /// <summary>
        /// Checks for value equality, not reference equality
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj", "obj can not be null");
            }

            Range<TPoint> range = obj as Range<TPoint>;
            if (range == null)
            {
                throw new ArgumentException("obj must be of type Range<TPoint>.", "obj");
            }

            return this.Start.Equals(range.Start) 
                && this.Stop.Equals(range.Stop) 
                && this.IntervalCount.Equals(range.IntervalCount);
        }

        public override int GetHashCode()
        {
            // Wrap instead of overflowing
            unchecked
            {
                int hash = 0;
                hash += this.Start.GetHashCode();
                hash *= 23;
                hash += this.Stop.GetHashCode();
                hash *= 23;
                hash += this.IntervalCount.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "[{0},{1})_{2}", this.Start, this.Stop, this.IntervalCount);
        }
    }
}
