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

namespace Microsoft.Spectrum.Storage.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;
    using Microsoft.Spectrum.Storage.Models;

    public class SpectrumDataProcessorStorage
    {
        private readonly ISpectrumDataProcessorMetadataStorage dataProcessorMetadataStorage;        
        private readonly ILogger logger;

        public SpectrumDataProcessorStorage(ISpectrumDataProcessorMetadataStorage dataProcessorMetadataStorage, ILogger logger)
        {
            if (dataProcessorMetadataStorage == null)
            {
                throw new ArgumentNullException("dataProcessorMetadataStorage");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.dataProcessorMetadataStorage = dataProcessorMetadataStorage;
            this.logger = logger;
        }

        public IEnumerable<SpectrumFrequency> GetSpectralData(Guid measurementStationId, TimeRangeKind timeGranularity, DateTime timeStart)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId");
            }

            try
            {
                return this.dataProcessorMetadataStorage.GetSpectralData(measurementStationId, timeGranularity, timeStart);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        public IEnumerable<SpectrumFrequency> GetSpectralDataByFrequency(Guid measurementStationId, TimeRangeKind timeGranularity, DateTime timeStart, long startFrequency, long endFrequency)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId");
            }

            if (startFrequency > endFrequency)
            {
                throw new ArgumentException("startFrequency");
            }

            if (startFrequency < 0)
            {
                throw new ArgumentException("startFrequency");
            }

            if (endFrequency < 0)
            {
                throw new ArgumentException("endFrequency");
            }

            try
            {
                return this.dataProcessorMetadataStorage.GetSpectralDataByFrequency(measurementStationId, timeGranularity, timeStart, startFrequency, endFrequency);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        public void InsertOrUpdateSpectralData(SpectrumCalibration spectralData)
        {
            if (spectralData == null)
            {
                throw new ArgumentNullException("spectralData");
            }

            try
            {
                this.AggregateRawSpectralDataWithProcessedSpectralData(spectralData);
                this.dataProcessorMetadataStorage.InsertOrUpdateSpectralData(spectralData);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        public async Task InsertOrUpdateSpectralDataAsync(SpectrumCalibration spectralData)
        {
            if (spectralData == null)
            {
                throw new ArgumentNullException("spectralData");
            }

            try
            {
                // TODO: Reading processed data asynchronously might provide some more performance benefit ?
                this.AggregateRawSpectralDataWithProcessedSpectralData(spectralData);
                await this.dataProcessorMetadataStorage.InsertOrUpdateSpectralDataAsync(spectralData);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        public void InsertOrUpdateScanFileInformation(ScanFileInformation scanFileInformation)
        {
            if (scanFileInformation == null)
            {
                throw new ArgumentNullException("scanFileInformation");
            }

            try
            {
                this.dataProcessorMetadataStorage.InsertOrUpdateScanFileInformation(scanFileInformation);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        public void PersistScanFileProcessingError(ScanFileProcessingError scanFileProcessingError)
        {
            if (scanFileProcessingError == null)
            {
                throw new ArgumentNullException("scanFileProcessingError", "ScanFileProcessingError parameter type can not be null");
            }

            try
            {
                this.dataProcessorMetadataStorage.PersistScanFileProcessingError(scanFileProcessingError);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        public IEnumerable<ScanFileProcessingError> GetAllSpectrumFileProcessingFailures(Guid measurementStationId, DateTime failuresSince)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "MeasurementStation Id can not be null");
            }

            try
            {
                return this.dataProcessorMetadataStorage.GetAllSpectrumFileProcessingFailures(measurementStationId, failuresSince);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        public IEnumerable<ScanFileInformation> GetSpectralMetadataInfo(Guid measurementStationId, DateTime timeStart)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "MeasurementStation Id can not be null");
            }

            try
            {
                return this.dataProcessorMetadataStorage.GetSpectralMetadataInfo(measurementStationId, timeStart);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        public IEnumerable<RawSpectralDataInfo> GetRawSpectralDataSchema(Guid measurementStationId, FileType fileType)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "MeasurementStation Id can not be null");
            }

            try
            {
                return this.dataProcessorMetadataStorage.GetRawSpectralDataSchema(measurementStationId, fileType);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, ex.ToString());
                throw;
            }
        }

        private static double CalculateSpectralDensityReading(double tableReading, double fileReading, ReadingKind readingKind, long fileSampleCount, long tableSampleCount)
        {
            double currentReading = fileReading;

            switch (readingKind)
            {
                case ReadingKind.AverageOfMaximum:
                case ReadingKind.AverageOfMinimum:
                case ReadingKind.Average:
                    {
                        long newSampleCount = fileSampleCount + tableSampleCount;
                        double newAverage = ((tableReading * tableSampleCount) + (fileReading * fileSampleCount)) / newSampleCount;
                        currentReading = newAverage;

                        break;
                    }

                case ReadingKind.Maximum:
                    {
                        if (tableReading > fileReading)
                        {
                            currentReading = tableReading;
                        }

                        break;
                    }

                case ReadingKind.Minimum:
                    {
                        if (tableReading < fileReading)
                        {
                            currentReading = tableReading;
                        }

                        break;
                    }

                case ReadingKind.StandardDeviationOfAverage:
                case ReadingKind.StandardDeviationOfMaximum:
                case ReadingKind.StandardDeviationOfMinimum:
                    {
                        long newSampleCount = fileSampleCount + tableSampleCount;
                        float newStandardDeviation = (float)Math.Sqrt(((Math.Pow(tableReading, 2) * tableSampleCount) + (Math.Pow(fileReading, 2) * fileSampleCount)) / newSampleCount);
                        currentReading = newStandardDeviation;

                        break;
                    }
            }

            return currentReading;
        }

        private void AggregateRawSpectralDataWithProcessedSpectralData(SpectrumCalibration aggregatedRawSpectralData)
        {
            var processedSpectralData = this.GetSpectralData(aggregatedRawSpectralData.MeasurementStationId, aggregatedRawSpectralData.TimeRangeKind, aggregatedRawSpectralData.TimeStart);

            foreach (SpectrumFrequency tableSpectrumFrequency in processedSpectralData)
            {
                SpectrumFrequency fileSpectrumFrequency = aggregatedRawSpectralData.Contains(tableSpectrumFrequency);

                if (fileSpectrumFrequency != null)
                {
                    long newSampleCount = tableSpectrumFrequency.SampleCount + fileSpectrumFrequency.SampleCount;

                    foreach (SpectralDensityReading tableSpectrumDensityReading in tableSpectrumFrequency.SpectralDensityReadings)
                    {
                        ReadingKind currentReadingKind = tableSpectrumDensityReading.Kind;
                        SpectralDensityReading fileSpectrumDensityReading = fileSpectrumFrequency.ContainsValueFor(currentReadingKind);

                        if (fileSpectrumDensityReading != null)
                        {
                            double newDataPoint = CalculateSpectralDensityReading(tableSpectrumDensityReading.DataPoint, fileSpectrumDensityReading.DataPoint, currentReadingKind, fileSpectrumFrequency.SampleCount, tableSpectrumFrequency.SampleCount);

                            SpectralDensityReading resultingSpectralDensityReading = new SpectralDensityReading(currentReadingKind, newDataPoint);
                            fileSpectrumFrequency.UpdateSpectrumDensityReading(resultingSpectralDensityReading);
                        }
                    }

                    fileSpectrumFrequency.UpdateSampleCount(newSampleCount);
                    aggregatedRawSpectralData.UpdateSpectrumFrequency(fileSpectrumFrequency);
                }
            }
        }
    }
}
