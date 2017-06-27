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

    public static class MathLibrary
    {
        /// <summary>
        /// Generate linearly spaced points [start, stop].
        /// </summary>
        public static IEnumerable<double> GetLinearSpace(double startFrequencyClosed, double stopFrequencyClosed, int sampleCount)
        {
            if (stopFrequencyClosed <= startFrequencyClosed)
            {
                throw new ArgumentException("the stop frequency has to be larger than the start frequency");
            }

            if (sampleCount < 2)
            {
                throw new ArgumentException("the number of samples has to be larger than 1");
            }

            double stepSize = (double)(stopFrequencyClosed - startFrequencyClosed) / sampleCount;

            for (int i = 0; i < sampleCount; ++i)
            {
                yield return startFrequencyClosed + (stepSize * i);
            }
        }

        /// <summary>
        /// Generate logarithmically spaced points [start, stop)
        /// </summary>
        public static IEnumerable<float> GetLogarithmicSpace(float startFrequencyClosed, float stopFrequencyOpen, int sampleCount)
        {
            if (stopFrequencyOpen <= startFrequencyClosed)
            {
                throw new ArgumentException("the stop frequency has to be larger than the start frequency");
            }

            if (sampleCount <= 0)
            {
                throw new ArgumentException("the number of samples has to be larger than 0");
            }

            // calculating the values between [StartFreq, StopFreq]
            // the formula is 
            // log(freq(i+1)) = log(freq(i)) + (log(StopFreq) - log(StartFreq))/(samples - 1)
            // freq(i+i) = freq(i) * 10 ^(log(StopFreq) - log(StartFreq))/(samples - 1) 

            // the target space is the log space
            float evenStepInLogSpace = (float)(Math.Log10(stopFrequencyOpen) - Math.Log10(startFrequencyClosed)) / sampleCount;
            float stepSize = (float)Math.Pow(10, evenStepInLogSpace);
            float x = startFrequencyClosed;

            for (int i = 0; i < sampleCount; ++i)
            {
                yield return x;
                x *= stepSize;
            }
        }

        /// <summary>
        /// Seek the location of the value in a sequence of buckets, 
        /// For example, if the bucket is { 0, 10, 20, 30 }, and the value is 9 the return result should be 1
        /// </summary>
        public static int SeekInIntervals(float[] buckets, int start, float value)
        {
            if (buckets == null || buckets.Length <= 1)
            {
                throw new ArgumentException("buckets must contain more than one element");
            }

            if (start >= buckets.Length)
            {
                throw new ArgumentException("start index has to be smaller than the size of buckets");
            }

            if (value < buckets[start])
            {
                throw new ArgumentException("the value has to be larger than the buckets[0]");
            }

            // TODO: Using binary search rather tha linear search;
            for (int i = start; i < buckets.Length - 1; i++)
            {
                if (value >= buckets[i] && value < buckets[i + 1])
                {
                    return i;
                }
            }

            // if the value is larger than the last element, return the last index as bucket
            return buckets.Length - 1;
        }

        /// <summary>
        /// Seek the location of the value in the log space defined by [start, stop) with samples 
        /// For example, if input is [0,100) with 3 points , and the value is 20 the return result should be 1 
        /// </summary>
        public static int SeekPositionInLogarithmicSpace(float startFrequencyClosed, float stopFrequencyOpen, int intervalCount, float value)
        {
            if (stopFrequencyOpen <= startFrequencyClosed)
            {
                throw new ArgumentException("the stop frequency has to be larger than the start frequency");
            }

            if (value < startFrequencyClosed || value > stopFrequencyOpen)
            {
                throw new ArgumentException("the value should be in the range of [start, stop]");
            }

            if (intervalCount < 2)
            {
                throw new ArgumentException("the number of sample has to be large than 1");
            }

            // the target space is the log space
            float evenStepInLogSpace = (float)(Math.Log10(stopFrequencyOpen) - Math.Log10(startFrequencyClosed)) / intervalCount;

            return (int)((float)(Math.Log10(value) - Math.Log10(startFrequencyClosed)) / evenStepInLogSpace);
        }

        /// <summary>
        /// linearly interpolate two one dimensional points (x1, y1) and (x2, y2) and get the y for the x
        /// </summary>
        public static double LinearInterpolation(double x1, double y1, double x2, double y2, double x)
        {
            double dx = x2 - x1;

            if (dx == 0)
            {
                throw new ArgumentException("X must be monotonic. A duplicate x2 and x1 was found");
            }

            if (dx < 0)
            {
                throw new ArgumentException("X must be sorted, x2 > x1");
            }

            if (x == x1)
            {
                return y1;
            }

            if (x == x2)
            {
                return y2;
            }

            if (x > x2 || x < x1)
            {
                return double.NaN;
            }

            // Perform the interpolation here
            double dy = y2 - y1;
            double slope = dy / dx;
            double intercept = y1 - (x1 * slope);
            double y = (slope * x) + intercept;

            return y;
        }

        public static double HzToMHz(double input)
        {
            return input / 1000000;
        }

        public static double MHzToHz(double input)
        {
            return input * 1000000;
        }

        public static double ToRawIQLinearGain(double gainIndB)
        {
            return Math.Pow(10, (gainIndB / 20));
        }

        public static double ToPSDLinearGain(double gainIndB)
        {
            return Math.Pow(10, (gainIndB / 10));
        }

        //FFT Window Fcts
        public enum WindowFunctions { Rectangle, Hann };

        
        public static double[] GetWindowFunction(WindowFunctions type, int length)
        {
            //!!!!!!!!!!!! Assumes interleaved complex array stored in double[].!!!!!!!!!!!!!!!
            double[] res = new double[length];
            
            switch (type)
            {
                //Rectangular / None.
                case WindowFunctions.Rectangle:
                    for(int i = 0; i < length; i++)
                    {
                        res[i] = 1;
                    }
                    break;

                //Almighthy Hann.
                case WindowFunctions.Hann:
                    for (int i = 0; i < length; i++)
                    {
                        //halve i (since we are dealing w. interleaved data)
                        int hann_val = i >> 1;

                        //calculate window
                        res[i] = 0.5 - 0.5*Math.Cos(2 * Math.PI * hann_val / ((length>>1) -1));
                    }
                    break;
                default:
                    throw new System.ArgumentException("Unsupported Window Function?!?!");
            }
            return res;
        }

        public static double[] ApplyWindowFunction(double[] data, double[] fct)
        {
            double[] res = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                res[i] = data[i] * fct[i];
            }
            return res;
        }

        public static double GetWindowCompensationFactor(WindowFunctions type)
        {
            switch (type)
            {
                //Rectangular / None.
                case WindowFunctions.Rectangle:
                    return 1;

                //Almighthy Hann.
                case WindowFunctions.Hann:
                    return 2;

                default:
                    throw new System.ArgumentException("Unsupported Window Function?!?!");
            }
        }

    }
}
