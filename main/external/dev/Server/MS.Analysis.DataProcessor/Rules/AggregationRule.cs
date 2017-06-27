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

namespace Microsoft.Spectrum.Analysis.DataProcessor.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.ScanFile;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Models;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;

    public abstract class AggregationRule
    {
        private readonly SpectrumDataProcessorStorage spectrumDataProcessStorage;

        protected AggregationRule(SpectrumDataProcessorStorage spectrumDataProcessStorage)
        {
            if (spectrumDataProcessStorage == null)
            {
                throw new ArgumentNullException("spectrumDataProcessStorage");
            }

            this.spectrumDataProcessStorage = spectrumDataProcessStorage;
        }

        protected SpectrumDataProcessorStorage SpectrumDataProcessStorage
        {
            get
            {
                return this.spectrumDataProcessStorage;
            }
        }

        public abstract void Run(IEnumerable<SpectralPsdDataBlock> spectralData, Guid measurementStationKey);

        public abstract Task RunAsync(IEnumerable<SpectralPsdDataBlock> spectralData, Guid measurementStationKey);

        protected IEnumerable<SpectrumCalibration> NormalizeSpectralDataByTimeGranularity(IEnumerable<SpectralPsdDataBlock> rawSpectralData, TimeRangeKind timeRangeKind, Guid measurementStationKey)
        {
            // Grouping the spectral data by timeRangeKind such as by Hour, Day, Week and Month.
            foreach (var dataBlocks in rawSpectralData.GroupByTimeGranularity(timeRangeKind))
            {
                DateTime timeStart = dataBlocks.Key;
                IEnumerable<SpectralPsdDataBlock> spectralPsdDataBlocks = dataBlocks.OfType<SpectralPsdDataBlock>();

                SpectrumCalibration spectralData = AggregationRule.GetSpectrumMeasurement(spectralPsdDataBlocks, timeRangeKind, measurementStationKey, timeStart);

                yield return spectralData;
            }
        }

        private static SpectrumCalibration GetSpectrumMeasurement(IEnumerable<SpectralPsdDataBlock> aggregatedRawSpectralData, TimeRangeKind timeRangeKind, Guid measurementStationKey, DateTime timeStart)
        {
            SpectrumCalibration spectrumMeasurement = new SpectrumCalibration(measurementStationKey, timeRangeKind, timeStart);

            // In case, multiple spectral blocks per sample with different frequency range group them by frequency and process.
            var spectralDataByFrequencyRange = aggregatedRawSpectralData.GroupBy(spectralData => new { StartFrequency = spectralData.StartFrequencyHz, StopFrequency = spectralData.StopFrequencyHz });

            if (spectralDataByFrequencyRange.Any())
            {
                foreach (var spectralDataByFrequency in spectralDataByFrequencyRange)
                {
                    double startFrequency = spectralDataByFrequency.Key.StartFrequency;
                    double stopFrequency = spectralDataByFrequency.Key.StopFrequency;

                    // Should we expect same data points length for all the SPBs ?
                    int dataPointsCount = spectralDataByFrequency.Select(spectralData => spectralData.DataPoints.Length).Only();

                    double[] frequencyDivisions = MathLibrary.GetLinearSpace(startFrequency, stopFrequency, dataPointsCount).ToArray();

                    // Resolve SpectralDataBlock samples into an array and then use them in further iteration.
                    var spectralDataByReadingKind = spectralDataByFrequency.GroupBy(spectralData => spectralData.ReadingKind)
                        .Select(readingKindSpectralData => new { ReadingKind = readingKindSpectralData.Key, Samples = readingKindSpectralData.ToArray() });

                    // TODO: Running this in parallel will enhance the performance.
                    for (int frequencyIndex = 0; frequencyIndex < frequencyDivisions.Length; frequencyIndex++)
                    {
                        SpectrumFrequency spectrumFrequency = new SpectrumFrequency((long)frequencyDivisions[frequencyIndex], 0);

                        foreach (var readingKindSpectralData in spectralDataByReadingKind)
                        {
                            // Obtain all the samples for a spectral reading kind.                        
                            IEnumerable<FixedShort> samplesPerFrequency = readingKindSpectralData.Samples.Select(sample => sample.DataPoints[frequencyIndex]);

                            // Should we be doing the average of the max, average of the min, and the average of the average here or do we want
                            //      the max of the max, min of the min, and the average of the average. To make sure that we catch bursts in 
                            //      usage, we want to do the max of the max, and the min of the min
                            if (readingKindSpectralData.ReadingKind == ReadingKind.Maximum)
                            {
                                double averageFixedShort = FixedShortReducer.Average(samplesPerFrequency);
                                double maxFixedShort = FixedShortReducer.Max(samplesPerFrequency);
                                double standardDeviation = FixedShortReducer.StandardDeviation(samplesPerFrequency, averageFixedShort);

                                SpectralDensityReading spectralStandardDeviationReading = new SpectralDensityReading(ReadingKind.StandardDeviationOfMaximum, standardDeviation);
                                spectrumFrequency.AddSpectrumDensityReading(spectralStandardDeviationReading);

                                SpectralDensityReading spectralAverageReading = new SpectralDensityReading(ReadingKind.AverageOfMaximum, averageFixedShort);
                                spectrumFrequency.AddSpectrumDensityReading(spectralAverageReading);

                                SpectralDensityReading spectralMaxReading = new SpectralDensityReading(ReadingKind.Maximum, maxFixedShort);
                                spectrumFrequency.AddSpectrumDensityReading(spectralMaxReading);
                            }
                            else if (readingKindSpectralData.ReadingKind == ReadingKind.Minimum)
                            {
                                double averageFixedShort = FixedShortReducer.Average(samplesPerFrequency);
                                double minFixedShort = FixedShortReducer.Min(samplesPerFrequency);
                                double standardDeviation = FixedShortReducer.StandardDeviation(samplesPerFrequency, averageFixedShort);

                                SpectralDensityReading spectralStandardDeviationReading = new SpectralDensityReading(ReadingKind.StandardDeviationOfMinimum, standardDeviation);
                                spectrumFrequency.AddSpectrumDensityReading(spectralStandardDeviationReading);

                                SpectralDensityReading spectralAverageReading = new SpectralDensityReading(ReadingKind.AverageOfMinimum, averageFixedShort);
                                spectrumFrequency.AddSpectrumDensityReading(spectralAverageReading);

                                SpectralDensityReading spectralMinReading = new SpectralDensityReading(ReadingKind.Minimum, minFixedShort);
                                spectrumFrequency.AddSpectrumDensityReading(spectralMinReading);
                            }
                            else
                            {
                                double averageFixedShort = FixedShortReducer.Average(samplesPerFrequency);
                                double standardDeviation = FixedShortReducer.StandardDeviation(samplesPerFrequency, averageFixedShort);

                                SpectralDensityReading spectralStandardDeviationReading = new SpectralDensityReading(ReadingKind.StandardDeviationOfAverage, standardDeviation);
                                spectrumFrequency.AddSpectrumDensityReading(spectralStandardDeviationReading);

                                SpectralDensityReading spectralAverageReading = new SpectralDensityReading(ReadingKind.Average, averageFixedShort);
                                spectrumFrequency.AddSpectrumDensityReading(spectralAverageReading);
                            }
                            
                            long samplesCount = samplesPerFrequency.Count();

                            // Reading Kind having maximum number of Samples as Samples count.
                            if (spectrumFrequency.SampleCount < samplesCount)
                            {
                                spectrumFrequency.UpdateSampleCount(samplesCount);
                            }
                        }

                        spectrumMeasurement.AddSpectrumFrequency(spectrumFrequency);
                    }
                }
            }

            return spectrumMeasurement;
        }
    }
}
