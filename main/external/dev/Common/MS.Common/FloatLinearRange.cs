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
    /// Represents a range of equally-spaced float values.
    /// </summary>
    public class FloatLinearRange : LinearRange<float, float, FloatLinearRange>
    {
        private FloatLinearRange(float start, float stop, int intervalCount)
            : base(start, stop, intervalCount)
        {
        }

        public override float IntervalSize
        {
            get { return (this.Stop - this.Start) / ((float)this.IntervalCount); }
        }

        /// <summary>
        /// [start, stop)
        /// </summary>
        public static FloatLinearRange Create(float start, float stop)
        {
            return Create(start, stop, 1);
        }

        /// <summary>
        /// [start, stop)
        /// </summary>
        public static FloatLinearRange Create(float start, float stop, int intervalCount)
        {
            if (intervalCount < 1)
            {
                throw new ArgumentOutOfRangeException("intervalCount", "intervalCount must be greater than 1");
            }

            if (start > stop)
            {
                throw new InvalidOperationException("The start value must be less than the stop value");
            }

            return new FloatLinearRange(start, stop, intervalCount);
        }

        /// <summary>
        /// [start, stop]
        /// </summary>
        public static FloatLinearRange CreateWithClosedStop(float start, float stop, int pointCount)
        {
            if (pointCount < 2)
            {
                throw new ArgumentOutOfRangeException("pointCount", "pointCount must be greater than 2");
            }

            if (start > stop)
            {
                throw new ArgumentOutOfRangeException("start", "The start value must be less than the stop value");
            }

            int intervalCount = pointCount - 1;
            float intervalLength = (stop - start) / ((float)intervalCount);
            float newStop = stop + intervalLength;
            int newIntervalCount = intervalCount + 1;

            return new FloatLinearRange(start, newStop, newIntervalCount);
        }

        public override IEnumerable<float> GetAllPointValues()
        {
            return this.GetAllPointValues(0);
        }

        public override IEnumerable<float> GetAllPointValues(int startIntervalIndex)
        {
            int lastPointIndex = this.IntervalCount - 1;
            if (startIntervalIndex < 0 || startIntervalIndex > lastPointIndex)
            {
                throw new ArgumentOutOfRangeException("startIntervalIndex is less than zero or greater than the last point", "startIntervalIndex");
            }

            float intervalSize = this.IntervalSize;
            float current = this.Start + (startIntervalIndex * intervalSize);

            for (int i = startIntervalIndex; i <= lastPointIndex; i++)
            {
                yield return current;
                current += intervalSize;
            }
        }

        public override bool Contains(float point)
        {
            return (this.Start <= point) && (point < this.Stop);
        }

        public override bool Overlaps(Range<float> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            return (this.Start < other.Stop) && (this.Stop > other.Start);
        }

        public override int FindIntervalIndex(float point)
        {
            if (!this.Contains(point))
            {
                throw new InvalidOperationException("The specified point is not contained in this range.");
            }

            return (int)((point - this.Start) / this.IntervalSize);
        }

        public override IEnumerable<FloatLinearRange> GetAllIntervals()
        {
            return this.GetAllIntervals(0);                     
        }

        public override IEnumerable<FloatLinearRange> GetAllIntervals(int startIntervalIndex)
        {
            int startPointIndex = startIntervalIndex;
            var points = this.GetAllPointValues(startPointIndex);
            bool first = true;
            float leftPoint = 0f;

            foreach (float rightPoint in points)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    yield return FloatLinearRange.Create(leftPoint, rightPoint);
                }

                leftPoint = rightPoint;
            }

            if (first)
            {
                throw new InvalidOperationException("The specified interval is not contained in this range ");
            }

            yield return FloatLinearRange.Create(leftPoint, this.Stop);
        }
    }
}