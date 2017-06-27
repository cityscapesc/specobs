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

namespace Microsoft.Spectrum.Storage.Table.Azure.Helpers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Storage.Models;

    internal static class DataProcessorDALHelper
    {
        public static IEnumerable<SpectrumFrequency> GetSpectrumFrequencies(IEnumerable<SpectralDataSchema> spectralData)
        {
            foreach (SpectralDataSchema spectrumData in spectralData)
            {
                SpectrumFrequency spectrumFrequency = new SpectrumFrequency(spectrumData.Frequency, spectrumData.SampleCount);

                SpectralDensityReading minSpectralDesityReading = new SpectralDensityReading(ReadingKind.Minimum, spectrumData.MinPower);
                SpectralDensityReading maxSpectralDesityReading = new SpectralDensityReading(ReadingKind.Maximum, spectrumData.MaxPower);
                SpectralDensityReading avgSpectralDesityReading = new SpectralDensityReading(ReadingKind.Average, spectrumData.AveragePower);
                SpectralDensityReading averageOfMinSpectralDensityReading = new SpectralDensityReading(ReadingKind.AverageOfMinimum, spectrumData.AverageOfMinimumPower);
                SpectralDensityReading averageOfMaxSpectralDensityReading = new SpectralDensityReading(ReadingKind.AverageOfMaximum, spectrumData.AverageOfMaximumPower);
                SpectralDensityReading standardDeviationOfMinSpectralDensityReading = new SpectralDensityReading(ReadingKind.StandardDeviationOfMinimum, spectrumData.StandardDeviationOfMinimumPower);
                SpectralDensityReading standardDeviationOfMaxSpectralDensityReading = new SpectralDensityReading(ReadingKind.StandardDeviationOfMaximum, spectrumData.StandardDeviationOfMaximumPower);
                SpectralDensityReading standardDeviationOfAverageSpectralDensityReading = new SpectralDensityReading(ReadingKind.StandardDeviationOfAverage, spectrumData.StandardDeviationOfAveragePower);

                spectrumFrequency.AddSpectrumDensityReading(minSpectralDesityReading);
                spectrumFrequency.AddSpectrumDensityReading(maxSpectralDesityReading);
                spectrumFrequency.AddSpectrumDensityReading(avgSpectralDesityReading);
                spectrumFrequency.AddSpectrumDensityReading(averageOfMinSpectralDensityReading);
                spectrumFrequency.AddSpectrumDensityReading(averageOfMaxSpectralDensityReading);
                spectrumFrequency.AddSpectrumDensityReading(standardDeviationOfMinSpectralDensityReading);
                spectrumFrequency.AddSpectrumDensityReading(standardDeviationOfMaxSpectralDensityReading);
                spectrumFrequency.AddSpectrumDensityReading(standardDeviationOfAverageSpectralDensityReading);

                yield return spectrumFrequency;
            }
        }

        public static RawSpectralDataSchema GetRawSpectralDataSchema(ScanFileInformation scanFileInformation)
        {
            RawSpectralDataSchema rawSpectralDataSchema = new RawSpectralDataSchema(scanFileInformation.MeasurementStationKey, scanFileInformation.TimeStart);

            rawSpectralDataSchema.CompressionType = scanFileInformation.CompressionType;
            rawSpectralDataSchema.TypeId = (int)scanFileInformation.TypeId;
            rawSpectralDataSchema.BlobUri = scanFileInformation.BlobUri;
            rawSpectralDataSchema.StartFrequency = scanFileInformation.StartFrequency;
            rawSpectralDataSchema.EndFrequency = scanFileInformation.EndFrequency;

            return rawSpectralDataSchema;
        }

        public static SpectrumFileProcessingFailures GetSpectrumFileProcessingFailureEntity(ScanFileProcessingError scanFileProcessingError)
        {
            SpectrumFileProcessingFailures fileProcessingFailure = new SpectrumFileProcessingFailures(scanFileProcessingError.MeasurementStationId, scanFileProcessingError.TimeOfFailure)
            {
                AbsoluteFilePath = scanFileProcessingError.AbsoluteFilePath,
                Error = scanFileProcessingError.Error
            };

            return fileProcessingFailure;
        }
    }
}
