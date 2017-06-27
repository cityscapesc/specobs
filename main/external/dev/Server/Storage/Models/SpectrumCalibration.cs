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

namespace Microsoft.Spectrum.Storage.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Spectrum.Common;

    public class SpectrumCalibration
    {
        /// <summary>
        /// Idea is to having a list of array is to have high performance insert and lookup operations.
        /// More info: <see cref="http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx"/>
        /// </summary>
        private List<SpectrumFrequency>[] frequencyBandsBuckets = new List<SpectrumFrequency>[101];

        public SpectrumCalibration(Guid measurementStationKey, TimeRangeKind timeRangeKind, DateTime timeStart)
        {
            if (measurementStationKey == null)
            {
                throw new ArgumentNullException("measurementStationKey");
            }

            this.MeasurementStationId = measurementStationKey;
            this.TimeRangeKind = timeRangeKind;
            this.TimeStart = timeStart;
        }

        public Guid MeasurementStationId { get; set; }

        public TimeRangeKind TimeRangeKind { get; set; }

        public DateTime TimeStart { get; set; }

        public int FrequencyBandsCount
        {
            get
            {
                if (!this.frequencyBandsBuckets.Any())
                {
                    return 0;
                }

                return this.frequencyBandsBuckets.Sum(frequencyBucket => frequencyBucket.Count);
            }
        }

        public IEnumerable<SpectrumFrequency> FrequencyBands
        {
            get
            {
                if (!this.frequencyBandsBuckets.Any())
                {
                    yield break;
                }

                for (int i = 0; i < this.frequencyBandsBuckets.Length; i++)
                {
                    List<SpectrumFrequency> frequencyBandsBucket = this.frequencyBandsBuckets[i];

                    foreach (SpectrumFrequency frequencyBand in frequencyBandsBucket)
                    {
                        yield return frequencyBand;
                    }
                }
            }
        }

        public void AddSpectrumFrequency(SpectrumFrequency frequency)
        {
            if (frequency == null)
            {
                throw new ArgumentNullException("frequency");
            }

            int bucketIndex = this.GetBucket(frequency.GetHashCode());

            if (this.Contains(frequency) != null)
            {
                throw new InvalidOperationException("This spectrum calibration already contains a frequency band with the same start frequency.");
            }

            if (this.frequencyBandsBuckets[bucketIndex] == null)
            {
                this.frequencyBandsBuckets[bucketIndex] = new List<SpectrumFrequency>();                
            }

            this.frequencyBandsBuckets[bucketIndex].Add(frequency);
        }

        public void UpdateSpectrumFrequency(SpectrumFrequency frequency)
        {
            if (frequency == null)
            {
                throw new ArgumentNullException("frequency");
            }

            int bucketIndex = this.GetBucket(frequency.GetHashCode());

            if (this.frequencyBandsBuckets[bucketIndex] != null)
            {
                List<SpectrumFrequency> frequencyBandsBucket = this.frequencyBandsBuckets[bucketIndex];

                for (int i = 0; i < frequencyBandsBucket.Count; i++)
                {
                    if (frequencyBandsBucket[i].StartHz == frequency.StartHz)
                    {
                        this.frequencyBandsBuckets[bucketIndex][i] = frequency;
                        break;
                    }
                }
            }
        }

        // TODO : How about just passing Start Frequency value as parameter here, and make use of hash-code obtained from Start Frequency ?
        public SpectrumFrequency Contains(SpectrumFrequency spectrumFrequency)
        {
            if (spectrumFrequency == null)
            {
                throw new ArgumentNullException("spectrumFrequency");
            }

            return this.Contains(spectrumFrequency, this.GetBucket(spectrumFrequency.GetHashCode()));
        }

        private int GetBucket(int hashcode)
        {
            unchecked
            {
                return (int)((uint)hashcode % (uint)this.frequencyBandsBuckets.Length);
            }
        }

        private SpectrumFrequency Contains(SpectrumFrequency spectrumFrequency, int bucketIndex)
        {
            SpectrumFrequency frequency = null;

            if (this.frequencyBandsBuckets[bucketIndex] != null)
            {
                frequency = this.frequencyBandsBuckets[bucketIndex].FirstOrDefault(frequencyBand => frequencyBand.StartHz == spectrumFrequency.StartHz);
            }

            return frequency;
        }
    }
}
