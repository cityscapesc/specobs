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
    /// the reducer calculates the min/max/average value;
    /// if there is NaN, just skip;
    /// the reason we don't use Linq.Min/Max/Average is it doesn't handle NaN in the way we need
    /// </summary>
    public static class FixedShortReducer
    {        
        public static double Min(IEnumerable<FixedShort> values)
        {
            if (values == null)
            {
                throw new ArgumentException("values can not be null", "values");
            }

            int validValueCount = 0;
            FixedShort min = FixedShort.MaxValue;

            foreach (FixedShort v in values)
            {
                if (v != FixedShort.NaN)
                {
                    if (v < min)
                    {
                        min = v;
                    }

                    validValueCount++;
                }
            }

            return validValueCount == 0 ? double.NaN : min.ToFloat();
        }        

        public static double Max(IEnumerable<FixedShort> values)
        {
            if (values == null)
            {
                throw new ArgumentException("values can not be null", "values");
            }

            int validValueCount = 0;
            FixedShort max = FixedShort.MinValue;

            foreach (FixedShort v in values)
            {
                if (v != FixedShort.NaN)
                {
                    if (v > max)
                    {
                        max = v;
                    }

                    validValueCount++;
                }
            }

            return validValueCount == 0 ? double.NaN : max.ToFloat();
        }

        public static double Average(IEnumerable<FixedShort> values)
        {
            if (values == null)
            {
                throw new ArgumentException("values can not be null", "values");
            }

            int c = 0;
            float sum = 0;

            foreach (FixedShort v in values)
            {
                if (v != FixedShort.NaN)
                {
                    sum += v.ToFloat();
                    c++;
                }
            }

            return (c != 0) ? sum / c : double.NaN;
        }
        
        public static double StandardDeviation(IEnumerable<FixedShort> values, double mean)
        {
            if (values == null)
            {
                throw new ArgumentException("values can not be null", "values");
            }

            int c = 0;
            double sum = 0;

            foreach (FixedShort v in values)
            {
                if (v != FixedShort.NaN)
                {
                    sum += Math.Pow(v.ToFloat() - mean, 2);
                    c++;
                }
            }

            return (c > 1) ? Math.Sqrt(sum / (c - 1)) : 0.0;
        }
    }
}