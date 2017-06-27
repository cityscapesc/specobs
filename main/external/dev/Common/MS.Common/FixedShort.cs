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

    // This particular format is called 'Q8.7' since there are 8 integer bits and
    // 7 fractional bits (with one sign bit).  The integer range of the number is [-255, 255].
    // 
    // Bit structure:
    //   siiiiiiiifffffff
    //   1111111
    //   6543210987654321
    //  s = sign bit
    //  i = 8 integer bits (0 to 255 values)
    //  f = 7 fractional bits (0 to 0.9921875 in 0.0078125 increments)
    //
    //  base = 2^7 = 128
    //
    // Note: 
    //  Multiplying a value by 2^N can be done faster by the CPU by shifting the bits left N positions (value << N).
    //  Dividing a value by 2^N can be done faster by the CPU by shifting the bits right N positions (value >> N).
    //
    // Reference:
    //  http://en.wikipedia.org/wiki/Q_(number_format)
    [Serializable]
    ////[DebuggerDisplay("{0} ([{1}={2}])", ToFloat(), _value, _value.ToString("X")]
    public struct FixedShort : IComparable, IComparable<FixedShort>, IEquatable<FixedShort>
    {
        /// <summary>
        /// The value of this constant is 256.0 (0x8000).
        /// </summary>
        public static readonly FixedShort MaxValue = new FixedShort() { value = short.MaxValue };
        
        /// <summary>
        /// The value of this constant is -255.984375 (0x7FFE).
        /// </summary>
        public static readonly FixedShort MinValue = new FixedShort() { value = short.MinValue + 1 };

        /// <summary>
        /// The value of this constant is -255.9921875 (0x7FFF).
        /// </summary>
        public static readonly FixedShort NaN = new FixedShort() { value = short.MinValue };

        private const int M = 8;
        private const int N = 7;
        private const int FullBase = 1 << N;
        private const int HalfBase = 1 << (N - 1);

        private short value;

        public FixedShort(int value)
        {
            // TODO: Catch the potential overflow here
            this.value = (short)(value * (int)FullBase);
        }

        public FixedShort(float value)
        {
            if (float.IsNaN(value))
            {
                this.value = short.MinValue; // internal implementation of NaN
            }
            else
            {
                this.value = (short)(value * (float)FullBase);
            }
        }

        public short Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.value = value;
            }
        }

        public static explicit operator short(FixedShort value)
        {
            return value.value;
        }

        /// <remarks>
        /// Note that this conversion interprets the specified short value as encoded underlying bits 
        /// used to store a FixedShort value. If integer interpretation is desired, use alternate 
        /// int overload.
        /// </remarks>
        public static explicit operator FixedShort(short value)
        {
            return new FixedShort() { value = value };
        }

        public static explicit operator int(FixedShort value)
        {
            return value.ToInt32();
        }

        public static explicit operator float(FixedShort value)
        {
            return value.ToFloat();
        }

        public static FixedShort operator +(FixedShort left, FixedShort right)
        {
            // (left/base) + (right/base) = (left+right)/base
            return new FixedShort() { value = (short)(left.value + right.value) };
        }

        public static FixedShort operator -(FixedShort left, FixedShort right)
        {
            // (left/base) - (right/base) = (left-right)/base
            return new FixedShort() { value = (short)(left.value - right.value) };
        }

        public static FixedShort operator *(FixedShort left, FixedShort right)
        {
            // (left/base) * (right/base) = (left*right)/(base^2) = ((left*right)/base)/base)
            int temp = (int)left.value * (int)right.value;
            temp += HalfBase; // Round the mid values up

            return new FixedShort() { value = (short)(temp >> N) };
        }

        public static FixedShort operator /(FixedShort left, FixedShort right)
        {
            // (left/base) / (right/base) = (left/base)*(base/right) = ((left*base)/right)/base)
            int temp = (int)left.value << N;
            temp = temp + ((int)right.value / 2); // Round the mid values up
            return new FixedShort() { value = (short)(temp / (int)right.value) };
        }

        public static bool operator <(FixedShort left, FixedShort right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(FixedShort left, FixedShort right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(FixedShort left, FixedShort right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(FixedShort left, FixedShort right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator ==(FixedShort left, FixedShort right)
        {
            return left.CompareTo(right) == 0;
        }

        public static bool operator !=(FixedShort left, FixedShort right)
        {
            return left.CompareTo(right) != 0;
        }

        public int ToInt32()
        {
            return (int)this.value / (int)FullBase;
        }

        public float ToFloat()
        {
            if (this == FixedShort.NaN)
            {
                return float.NaN;
            }

            return (float)this.value / (float)FullBase;
        }

        // ret < 0: this < obj
        // ret == 0: this == obj
        // ret > 0: this > obj
        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is FixedShort))
            {
                return 1;
            }

            return this.CompareTo((FixedShort)obj);
        }

        // ret < 0: this < other
        // ret == 0: this == other
        // ret > 0: this > other
        public int CompareTo(FixedShort other)
        {
            return this.value - other.value;
        }

        /// <summary>
        /// Value comparison, not reference comparison
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is FixedShort)
            {
                return this.Equals((FixedShort)obj);
            }

            return false;
        }

        /// <summary>
        /// Value comparison, not reference comparison
        /// </returns>
        public bool Equals(FixedShort other)
        {
            return this.CompareTo(other) == 0;
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} ([{1}={2}])", this.ToFloat(), this.value, this.value.ToString("X", CultureInfo.InvariantCulture));
        }
    }
}
