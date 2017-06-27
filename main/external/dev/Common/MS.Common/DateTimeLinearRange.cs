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

    public class DateTimeLinearRange : LinearRange<DateTime, TimeSpan, DateTimeLinearRange>
    {
        private DateTimeLinearRange(DateTime start, DateTime stop, int intervalCount)
            : base(start, stop, intervalCount)
        {
        }

        public override TimeSpan IntervalSize
        {
            get
            {
                TimeSpan totalLength = this.Stop - this.Start;
                TimeSpan intervalLength = TimeSpan.FromTicks(totalLength.Ticks / this.IntervalCount);
                return intervalLength;
            }
        }

        public static DateTimeLinearRange Create(DateTime start, DateTime stop)
        {
            return Create(start, stop, 1);
        }

        public static DateTimeLinearRange Create(DateTime start, DateTime stop, int intervalCount)
        {
            if (intervalCount < 1)
            {
                throw new ArgumentOutOfRangeException("intervalCount", "Count must be greater than 1");
            }

            if (stop < start)
            {
                throw new InvalidOperationException("The stop timestamp must occur after the start timestamp.");
            }

            return new DateTimeLinearRange(start, stop, intervalCount);
        }

        public static DateTimeLinearRange CreateWithClosedStop(DateTime start, DateTime stop, int pointCount)
        {
            if (pointCount < 2)
            {
                throw new ArgumentOutOfRangeException("pointCount", "Count must be greater than 2");
            }

            if (stop < start)
            {
                throw new InvalidOperationException("The stop timestamp must occur after the start timestamp.");
            }

            int intervalCount = pointCount - 1;
            TimeSpan totalLength = stop - start;
            TimeSpan intervalLength = TimeSpan.FromTicks(totalLength.Ticks / intervalCount);
            DateTime newStop = stop + intervalLength;
            int newIntervalCount = intervalCount + 1;

            return new DateTimeLinearRange(start, newStop, newIntervalCount);
        }

        public override bool Contains(DateTime point)
        {
            return (this.Start <= point) && (point < this.Stop);
        }

        public override bool Overlaps(Range<DateTime> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return (this.Start < other.Stop) && (this.Stop > other.Start);
        }

        public override int FindIntervalIndex(DateTime point)
        {
            if (!this.Contains(point))
            {
                throw new InvalidOperationException("The specified point is not contained in this range.");
            }

            return (int)((point.ToUniversalTime().Ticks - this.Start.ToUniversalTime().Ticks) / this.IntervalSize.Ticks);
        }

        public override IEnumerable<DateTime> GetAllPointValues()
        {
            return this.GetAllPointValues(0);        
        }

        public override IEnumerable<DateTime> GetAllPointValues(int startIntervalIndex)
        {
            int lastClosedPointIndex = this.IntervalCount - 1;
            if (startIntervalIndex < 0 || startIntervalIndex > lastClosedPointIndex)
            {
                throw new ArgumentOutOfRangeException("startIntervalIndex");
            }

            TimeSpan intervalSize = this.IntervalSize;
            DateTime current = this.Start.AddTicks(startIntervalIndex * intervalSize.Ticks);

            for (int i = startIntervalIndex; i <= lastClosedPointIndex; i++)
            {
                yield return current;
                current += intervalSize;                
            }
        }

        public override IEnumerable<DateTimeLinearRange> GetAllIntervals()
        {
            return this.GetAllIntervals(0);
        }

        public override IEnumerable<DateTimeLinearRange> GetAllIntervals(int startIntervalIndex)
        {
            int startPointIndex = startIntervalIndex;
            var points = this.GetAllPointValues(startPointIndex);
            bool first = true;

            DateTime leftPoint = DateTime.MinValue;
            foreach (DateTime rightPoint in points)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    yield return DateTimeLinearRange.Create(leftPoint, rightPoint);
                }

                leftPoint = rightPoint;
            }

            if (first)
            {
                throw new InvalidOperationException("The specified interval is not contained in this range ");
            }

            yield return DateTimeLinearRange.Create(leftPoint, this.Stop);
        }
    }
}