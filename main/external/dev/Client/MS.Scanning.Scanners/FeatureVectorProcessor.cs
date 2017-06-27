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

namespace Microsoft.Spectrum.Scanning.Scanners
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Numerics;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.ScanFile;

    /// <summary>
    /// This class takes the raw data produced by the scanner and processes it into feature vectors.
    /// The feature vectors that we currently support are in the ReadingKind enum...
    ///   - Average
    ///   - Min
    ///   - Max
    ///   - Average Above / Peak Below the noise floor
    /// 
    /// </summary>
    public class FeatureVectorProcessor : IDisposable
    {
        private readonly int sampleCountInAFullScan;
        private readonly int samplesPerFft;

        private BlockingCollection<FixedShort[]> dataPool;
        private int[] itemsInAverage;
        private double[] avgData;
        private double[] minData;
        private double[] maxData;
        private bool decibelData;

        public FeatureVectorProcessor(int sampleCountInAFullScan, int samplesPerFft)
        {
            this.sampleCountInAFullScan = sampleCountInAFullScan;
            this.samplesPerFft = samplesPerFft;

            // 4 is arbitrary. Currently, it would mean 4 minutes worth of data that could be in the file writing queue.
            // 3 is the number of feature vectors currently supported (Min, Max, Avg)
            int itemsInPool = 4 * 3;
            this.dataPool = new BlockingCollection<FixedShort[]>(itemsInPool);

            for (int i = 0; i < this.dataPool.BoundedCapacity; i++)
            {
                this.dataPool.Add(new FixedShort[sampleCountInAFullScan]);
            }

            this.itemsInAverage = new int[sampleCountInAFullScan];
            this.avgData = new double[sampleCountInAFullScan];
            this.minData = new double[sampleCountInAFullScan];
            this.maxData = new double[sampleCountInAFullScan];
            this.decibelData = false;

            this.Reset();
        }

        // out[0] is called the zero-frequency, or DC. It is often dropped because of the extra processing from 
        // the radio front-end.
        //
        // Here is how you map it to the frequency: let's start with a hypothetical setting.
        //
        // Let's say your center frequency is 100MHz, and your sampling rate is 20MHz, and you have 2048 samples. 
        // As a result, each sample represents 20MHz/2048 = 9.7kHz. For simplicity of explanation, let's round it
        // to 10KHz.
        //        
        // From FFTW, you have out[0], out[1], ...., out[2047].
        //
        // out[0] corresponds to frequency 100MHz. 
        // out[1] is frequency 100MHz + 10KHz. 
        // until, 
        // out[1023] --> 100MHz + (1023) * 10KHz. 
        //
        // However, from out[1024], the mapping wraps over to the negative side, 
        //
        // out[1024] --> 100MHz + (1024 - 2048) * 10 KHz = 100MHz - 1024 * 10KHz
        // out[1025] --> 100MHz - 1023 * 10KHz
        // ...
        // out[2047] --> 100MHz - 10KHz
        //
        // So here we process both halves of the FFT data and put them "in order"
        // This results in [start, stop)
        public void ProcessData(Complex[] fftData, int instantPowerStartIndex)
        {
            int fftHalfLength = fftData.Length / 2;

            for (int instantPowerIndex = 0; instantPowerIndex < fftData.Length; instantPowerIndex++)
            {
                int fftIndex = 0;

                if (instantPowerIndex < fftHalfLength)
                {
                    fftIndex = instantPowerIndex + fftHalfLength;
                }
                else
                {
                    fftIndex = instantPowerIndex - fftHalfLength;
                }

                Complex c = fftData[fftIndex];
                double instantPower = (c.Real / this.samplesPerFft * c.Real / this.samplesPerFft) + (c.Imaginary / this.samplesPerFft * c.Imaginary / this.samplesPerFft);

                this.ProcessData(instantPower, instantPowerStartIndex + instantPowerIndex);
            }
        }

        public void ProcessDataDCSpikeScan(Complex[] fftDataFirst, Complex[] fftDataSecond, int instantPowerStartIndex)
        {
            int fftHalfLength = fftDataFirst.Length / 2;
            Complex[] readFftArray;

            for (int instantPowerIndex = 0; instantPowerIndex < fftDataFirst.Length; instantPowerIndex++)
            {
                //Calculate the index
                int fftIndex = 0;

                int fftIndexFirst = 0, fftIndexSecond = 0;
                int PowerIndex_Offsetted_First = instantPowerIndex + (int)(fftDataFirst.Length * 0.15);
                int PowerIndex_Offsetted_Second = instantPowerIndex - (int)(fftDataFirst.Length * 0.15);

                if (PowerIndex_Offsetted_First < fftHalfLength)
                {
                    fftIndexFirst = PowerIndex_Offsetted_First + fftHalfLength;
                }
                else
                {
                    fftIndexFirst = PowerIndex_Offsetted_First - fftHalfLength;
                }

                if (PowerIndex_Offsetted_Second < fftHalfLength)
                {
                    fftIndexSecond = PowerIndex_Offsetted_Second + fftHalfLength;
                }
                else
                {
                    fftIndexSecond = PowerIndex_Offsetted_Second - fftHalfLength;
                }

                //Choose Which FFT Data to use
                if ((instantPowerIndex <= (0.3 * fftDataFirst.Length) || (instantPowerIndex >= (0.5 * fftDataFirst.Length) && instantPowerIndex < (0.7 * fftDataFirst.Length)) )
                    && fftDataFirst.Length > fftIndexFirst)
                {
                    readFftArray = fftDataFirst;
                    fftIndex = fftIndexFirst;
                }
                else
                {
                    readFftArray = fftDataSecond;
                    fftIndex = fftIndexSecond;
                }

                //Execute
                Complex c = readFftArray[fftIndex];
                double instantPower = (c.Real / this.samplesPerFft * c.Real / this.samplesPerFft) + (c.Imaginary / this.samplesPerFft * c.Imaginary / this.samplesPerFft);

                this.ProcessData(instantPower, instantPowerStartIndex + instantPowerIndex);
            }
        }

        public void ProcessDbData(double[] instantPowerData, int instantPowerStartIndex)
        {
            for (int instantPowerIndex = 0; instantPowerIndex < instantPowerData.Length; instantPowerIndex++)
            {
                this.ProcessData(instantPowerData[instantPowerIndex], instantPowerStartIndex + instantPowerIndex);
            }

            this.decibelData = true;
        }

        public void ProcessData(double instantPower, int index)
        {
            this.itemsInAverage[index]++;

            this.avgData[index] += instantPower;

            if (instantPower < this.minData[index])
            {
                this.minData[index] = instantPower;
            }

            if (instantPower > this.maxData[index])
            {
                this.maxData[index] = instantPower;
            }
        }

        public IEnumerable<ReadingKindData> GetResults()
        {
            FixedShort[] avgData = this.dataPool.Take();
            FixedShort[] minData = this.dataPool.Take();
            FixedShort[] maxData = this.dataPool.Take();

            // Calculate average before returning all items
            for (int i = 0; i < this.sampleCountInAFullScan; i++)
            {
                if (this.decibelData)
                {
                    avgData[i] = new FixedShort((float)(this.avgData[i] / this.itemsInAverage[i]));
                    minData[i] = new FixedShort((float)this.minData[i]);
                    maxData[i] = new FixedShort((float)this.maxData[i]);
                }
                else
                {
                    avgData[i] = new FixedShort((float)(10 * Math.Log10(this.avgData[i] / this.itemsInAverage[i])));
                    minData[i] = new FixedShort((float)(10 * Math.Log10(this.minData[i])));
                    maxData[i] = new FixedShort((float)(10 * Math.Log10(this.maxData[i])));
                }
            }

            yield return new ReadingKindData(ReadingKind.Average, avgData);
            yield return new ReadingKindData(ReadingKind.Minimum, minData);
            yield return new ReadingKindData(ReadingKind.Maximum, maxData);

            this.Reset();
        }

        public void ReturnItemToPool(FixedShort[] item)
        {
            if (item.Length != this.sampleCountInAFullScan)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "This item does not belong to this data pool - it is the wrong length.  Actual length: {0}, Expected length: {1}",
                    item.Length,
                    this.sampleCountInAFullScan));
            }

            this.dataPool.Add(item);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.dataPool.Dispose();
            }
        }

        private void Reset()
        {
            for (int i = 0; i < this.sampleCountInAFullScan; i++)
            {
                this.itemsInAverage[i] = 0;
                this.avgData[i] = 0f;
                this.minData[i] = double.MaxValue;
                this.maxData[i] = double.MinValue;
            }
        }
    }
}