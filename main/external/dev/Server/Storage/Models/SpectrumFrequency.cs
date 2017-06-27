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

    public class SpectrumFrequency
    {
        private List<SpectralDensityReading> spectralDensityReadings = null;

        public SpectrumFrequency(long startFrequency, long sampleCount)
        {
            this.StartHz = startFrequency;
            this.SampleCount = sampleCount;
        }

        public long StartHz { get; private set; }

        public long SampleCount { get; private set; }

        public int SpectralDensityReadingsCount
        {
            get
            {
                return this.spectralDensityReadings.Count;
            }
        }

        public IEnumerable<SpectralDensityReading> SpectralDensityReadings
        {
            get
            {
                return this.spectralDensityReadings ?? Enumerable.Empty<SpectralDensityReading>();
            }
        }

        public void AddSpectrumDensityReading(SpectralDensityReading reading)
        {
            if (reading == null)
            {
                throw new ArgumentNullException("reading");
            }

            if (this.spectralDensityReadings == null)
            {
                this.spectralDensityReadings = new List<SpectralDensityReading>();
            }

            if (this.ContainsValueFor(reading.Kind) == null)
            {
                this.spectralDensityReadings.Add(reading);
            }
        }

        public void UpdateSpectrumDensityReading(SpectralDensityReading reading)
        {
            if (reading == null)
            {
                throw new ArgumentNullException("reading");
            }

            if (this.spectralDensityReadings.Any())
            {
                for (int i = 0; i < this.spectralDensityReadings.Count; i++)
                {
                    if (this.spectralDensityReadings[i].Kind == reading.Kind)
                    {
                        this.spectralDensityReadings[i] = reading;
                        break;
                    }
                }
            }
        }

        public void UpdateSampleCount(long sampleCount)
        {
            this.SampleCount = sampleCount;
        }

        public SpectralDensityReading ContainsValueFor(ReadingKind readingKind)
        {
            return this.spectralDensityReadings.FirstOrDefault(reading => reading.Kind == readingKind);
        }

        public override int GetHashCode()
        {
            return (int)this.StartHz;
        }
    }
}
