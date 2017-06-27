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

namespace Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Enums;
    using Microsoft.Spectrum.Storage.Models;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Table;

    public class SpectrumDataProcessorMetadataStorage : ISpectrumDataProcessorMetadataStorage
    {
        private readonly AzureTableDbContext dataContext;
        private readonly ILogger logger;

        public SpectrumDataProcessorMetadataStorage(AzureTableDbContext dataContext, ILogger logger)
        {
            if (dataContext == null)
            {
                throw new ArgumentNullException("dataContext");
            }

            this.dataContext = dataContext;
            this.logger = logger;
        }

        public IEnumerable<SpectrumFrequency> GetSpectralData(Guid measurementStationId, TimeRangeKind timeGranularity, DateTime timeStart)
        {
            RetryAzureTableOperations<SpectralDataSchema> operationContext = this.dataContext.SpectralDataOperations;

            string spectralDataTableName = AzureTableHelper.GetSpectralDataTableName(measurementStationId, timeGranularity);
            operationContext.GetTableReference(spectralDataTableName);

            IEnumerable<SpectralDataSchema> spectralData = operationContext.GetDataByTime<SpectralDataSchema>(timeStart);

            IEnumerable<SpectrumFrequency> spectrumFrequencies = DataProcessorDALHelper.GetSpectrumFrequencies(spectralData);

            return spectrumFrequencies;
        }

        public IEnumerable<SpectrumFrequency> GetSpectralDataByFrequency(Guid measurementStationId, TimeRangeKind timeGranularity, DateTime timeStart, long startFrequency, long endFrequency)
        {
            RetryAzureTableOperations<SpectralDataSchema> operationContext = this.dataContext.SpectralDataOperations;

            string spectralDataTableName = AzureTableHelper.GetSpectralDataTableName(measurementStationId, timeGranularity);
            operationContext.GetTableReference(spectralDataTableName);

            IEnumerable<SpectralDataSchema> spectralData = operationContext.GetDataByTimeAndFrequency<SpectralDataSchema>(timeStart, startFrequency, endFrequency);

            IEnumerable<SpectrumFrequency> spectrumFrequencies = DataProcessorDALHelper.GetSpectrumFrequencies(spectralData);

            return spectrumFrequencies;
        }

        public void DeleteOldSpectralData(string spectralDataTableName, TimeSpan retentionTime)
        {
            RetryAzureTableOperations<SpectralDataSchema> operationContext = this.dataContext.SpectralDataOperations;
            operationContext.GetTableReference(spectralDataTableName);

            DateTime dayTimeRemoveBefore = DateTime.UtcNow.Subtract(retentionTime);

            string retentionTimeQuery = TableQuery.GenerateFilterCondition(Constants.PartitionKey, QueryComparisons.LessThan, dayTimeRemoveBefore.ToUniversalTime().Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture));
            operationContext.DeleteEntities(operationContext.ExecuteQueryWithContinuation<SpectralDataSchema>(retentionTimeQuery).ToList());
        }

        public void InsertOrUpdateSpectralData(SpectrumCalibration spectralData)
        {
            if (spectralData == null)
            {
                throw new ArgumentException("spectralData can not be null", "spectralData");
            }

            RetryAzureTableOperations<SpectralDataSchema> operationContext = this.GetSpectralDataSchemaOperationContext(spectralData.MeasurementStationId, spectralData.TimeRangeKind);

            List<SpectralDataSchema> spectralSchemaCollection = new List<SpectralDataSchema>();

            foreach (SpectrumFrequency spectrumFrequency in spectralData.FrequencyBands)
            {
                SpectralDataSchema spectrumDataEntity = new SpectralDataSchema(spectralData.TimeStart, spectrumFrequency.StartHz);
                spectrumDataEntity.SampleCount = spectrumFrequency.SampleCount;

                foreach (SpectralDensityReading reading in spectrumFrequency.SpectralDensityReadings)
                {
                    double powerValue = reading.DataPoint;

                    switch (reading.Kind)
                    {
                        case ReadingKind.Average:
                            spectrumDataEntity.AveragePower = powerValue;
                            break;

                        case ReadingKind.Maximum:
                            spectrumDataEntity.MaxPower = powerValue;
                            break;

                        case ReadingKind.Minimum:
                            spectrumDataEntity.MinPower = powerValue;
                            break;

                        case ReadingKind.StandardDeviationOfAverage:
                            spectrumDataEntity.StandardDeviationOfAveragePower = powerValue;
                            break;

                        case ReadingKind.StandardDeviationOfMaximum:
                            spectrumDataEntity.StandardDeviationOfMaximumPower = powerValue;
                            break;

                        case ReadingKind.StandardDeviationOfMinimum:
                            spectrumDataEntity.StandardDeviationOfMinimumPower = powerValue;
                            break;

                        case ReadingKind.AverageOfMaximum:
                            spectrumDataEntity.AverageOfMaximumPower = powerValue;
                            break;

                        case ReadingKind.AverageOfMinimum:
                            spectrumDataEntity.AverageOfMinimumPower = powerValue;
                            break;
                    }
                }

                spectralSchemaCollection.Add(spectrumDataEntity);

                if (spectralSchemaCollection.Count % 100 == 0)
                {
                    try
                    {
                        operationContext.InsertOrReplaceEntities(spectralSchemaCollection, false);
                        spectralSchemaCollection.Clear();
                    }
                    catch (Exception ex)
                    {
                        this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, string.Format(CultureInfo.InvariantCulture, "Error inserting spetral data entities for Measurement Station: {0}, Exception: {1}", spectralData.MeasurementStationId, ex.ToString()));
                    }
                }
            }

            // One last time before we finish.
            if (spectralSchemaCollection.Any())
            {
                try
                {
                    operationContext.InsertOrReplaceEntities(spectralSchemaCollection, false);
                    spectralSchemaCollection.Clear();
                }
                catch (Exception ex)
                {
                    this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorAgentId, string.Format(CultureInfo.InvariantCulture, "Error inserting spetral data entities for Measurement Station: {0}, Exception: {1}", spectralData.MeasurementStationId, ex.ToString()));
                }
            }
        }

        public async Task InsertOrUpdateSpectralDataAsync(SpectrumCalibration spectralData)
        {
            RetryAzureTableOperations<SpectralDataSchema> operationContext = this.GetSpectralDataSchemaOperationContext(spectralData.MeasurementStationId, spectralData.TimeRangeKind);

            List<SpectralDataSchema> spectralSchemaCollection = new List<SpectralDataSchema>();

            foreach (SpectrumFrequency spectrumFrequency in spectralData.FrequencyBands)
            {
                SpectralDataSchema spectrumDataEntity = new SpectralDataSchema(spectralData.TimeStart, spectrumFrequency.StartHz);
                spectrumDataEntity.SampleCount = spectrumFrequency.SampleCount;

                foreach (SpectralDensityReading reading in spectrumFrequency.SpectralDensityReadings)
                {
                    double powerValue = reading.DataPoint;

                    switch (reading.Kind)
                    {
                        case ReadingKind.Average:
                            spectrumDataEntity.AveragePower = powerValue;
                            break;

                        case ReadingKind.Maximum:
                            spectrumDataEntity.MaxPower = powerValue;
                            break;

                        case ReadingKind.Minimum:
                            spectrumDataEntity.MinPower = powerValue;
                            break;
                    }
                }

                spectralSchemaCollection.Add(spectrumDataEntity);

                if (spectralSchemaCollection.Count % 100 == 0)
                {
                    await operationContext.InsertOrReplaceEntitiesAsync(spectralSchemaCollection, true);
                    spectralSchemaCollection.Clear();
                }
            }

            // One last time before we finish.
            if (spectralSchemaCollection.Any())
            {
                await operationContext.InsertOrReplaceEntitiesAsync(spectralSchemaCollection, true);
                spectralSchemaCollection.Clear();
            }
        }

        public void InsertOrUpdateScanFileInformation(ScanFileInformation scanFileInformation)
        {
            RetryAzureTableOperations<RawSpectralDataSchema> operationContext = this.dataContext.RawSpectralDataSchemaOperations;
            operationContext.GetTableReference(AzureTableHelper.RawSpectralDataSchemaTable);

            RawSpectralDataSchema rawSpectralDataSchema = DataProcessorDALHelper.GetRawSpectralDataSchema(scanFileInformation);
            operationContext.InsertOrReplaceEntity(rawSpectralDataSchema, true);
        }

        public void PersistScanFileProcessingError(ScanFileProcessingError scanFileProcessingError)
        {
            if (scanFileProcessingError == null)
            {
                throw new ArgumentNullException("scanFileProcessingError", "ScanFileProcessingError parameter can not be null");
            }

            RetryAzureTableOperations<SpectrumFileProcessingFailures> operationContext = this.dataContext.SpectrumFileProcessingFailuresTableOperations;
            operationContext.GetTableReference(AzureTableHelper.SpectrumFileProcessingFailuresTable);

            SpectrumFileProcessingFailures scanFileProcessingFailureEntity = DataProcessorDALHelper.GetSpectrumFileProcessingFailureEntity(scanFileProcessingError);

            operationContext.InsertOrReplaceEntity(scanFileProcessingFailureEntity, true);
        }

        public IEnumerable<ScanFileProcessingError> GetAllSpectrumFileProcessingFailures(Guid measurementStationId, DateTime failuresSince)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "Measurement Station Id can not be null");
            }

            RetryAzureTableOperations<SpectrumFileProcessingFailures> operationContext = this.dataContext.SpectrumFileProcessingFailuresTableOperations;
            operationContext.GetTableReference(AzureTableHelper.SpectrumFileProcessingFailuresTable);

            string timeSince = failuresSince.ToUniversalTime().Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);

            string measurementStationQuery = TableQuery.GenerateFilterCondition(Constants.PartitionKey, QueryComparisons.Equal, measurementStationId.ToString());
            string timeSinceQuery = TableQuery.GenerateFilterCondition(Constants.RowKey, QueryComparisons.GreaterThanOrEqual, timeSince);

            string query = TableQuery.CombineFilters(measurementStationQuery, TableOperators.And, timeSinceQuery);

            IEnumerable<SpectrumFileProcessingFailures> fileProcessingFailures = operationContext.ExecuteQueryWithContinuation<SpectrumFileProcessingFailures>(query);

            List<ScanFileProcessingError> fileProcessingErrors = new List<ScanFileProcessingError>();

            foreach (SpectrumFileProcessingFailures failure in fileProcessingFailures)
            {
                fileProcessingErrors.Add(new ScanFileProcessingError(measurementStationId, failure.AbsoluteFilePath, failure.Error, failure.TimeOfFailure));
            }

            return fileProcessingErrors;
        }

        public IEnumerable<ScanFileInformation> GetSpectralMetadataInfo(Guid measurementStationId, DateTime timeStart)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "MeasurementStationId parameter can not be null");
            }

            RetryAzureTableOperations<RawSpectralDataSchema> operationContext = this.dataContext.RawSpectralDataSchemaOperations;
            operationContext.GetTableReference(AzureTableHelper.RawSpectralDataSchemaTable);

            string timeSince = timeStart.ToUniversalTime().Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);

            string measurementStationQuery = TableQuery.GenerateFilterCondition(Constants.PartitionKey, QueryComparisons.Equal, measurementStationId.ToString());
            string timeSinceQuery = TableQuery.GenerateFilterCondition(Constants.RowKey, QueryComparisons.GreaterThanOrEqual, timeSince);

            string query = TableQuery.CombineFilters(measurementStationQuery, TableOperators.And, timeSinceQuery);

            List<ScanFileInformation> scanFileInformationCollection = new List<ScanFileInformation>();

            IEnumerable<RawSpectralDataSchema> rawSpectralDataSchemaList = operationContext.ExecuteQueryWithContinuation<RawSpectralDataSchema>(query);

            foreach (RawSpectralDataSchema rawSpectralDataSchema in rawSpectralDataSchemaList)
            {
                scanFileInformationCollection.Add(new ScanFileInformation(measurementStationId, rawSpectralDataSchema.TimeStart, rawSpectralDataSchema.CompressionType, (FileType)rawSpectralDataSchema.CompressionType, rawSpectralDataSchema.BlobUri, rawSpectralDataSchema.StartFrequency, rawSpectralDataSchema.EndFrequency));
            }

            return scanFileInformationCollection;
        }

        public IEnumerable<ScanFileInformation> GetSpectralMetadataInfoByTimeRange(Guid measurementStationId, DateTime timeStart, DateTime timeEnd, int rawDataType)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "MeasurementStationId parameter can not be null");
            }

            RetryAzureTableOperations<RawSpectralDataSchema> operationContext = this.dataContext.RawSpectralDataSchemaOperations;
            operationContext.GetTableReference(AzureTableHelper.RawSpectralDataSchemaTable);

            string timeSince = timeStart.Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);
            string timeUntil = timeEnd.Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);

            string measurementStationQuery = TableQuery.GenerateFilterCondition(Constants.PartitionKey, QueryComparisons.Equal, measurementStationId.ToString());
            string timeSinceQuery = TableQuery.GenerateFilterCondition(Constants.RowKey, QueryComparisons.GreaterThanOrEqual, timeSince);
            string timeUntilQuery = TableQuery.GenerateFilterCondition(Constants.RowKey, QueryComparisons.LessThanOrEqual, timeUntil);

            string typeQuery = TableQuery.GenerateFilterConditionForInt("TypeId", QueryComparisons.Equal, rawDataType);

            string query = TableQuery.CombineFilters(measurementStationQuery, TableOperators.And, timeSinceQuery);
            query = TableQuery.CombineFilters(query, TableOperators.And, timeUntilQuery);
            query = TableQuery.CombineFilters(query, TableOperators.And, typeQuery);

            List<ScanFileInformation> scanFileInformationCollection = new List<ScanFileInformation>();

            IEnumerable<RawSpectralDataSchema> rawSpectralDataSchemaList = operationContext.ExecuteQueryWithContinuation<RawSpectralDataSchema>(query);

            foreach (RawSpectralDataSchema rawSpectralDataSchema in rawSpectralDataSchemaList)
            {
                scanFileInformationCollection.Add(new ScanFileInformation(measurementStationId, rawSpectralDataSchema.TimeStart, rawSpectralDataSchema.CompressionType, (FileType)rawSpectralDataSchema.CompressionType, rawSpectralDataSchema.BlobUri, rawSpectralDataSchema.StartFrequency, rawSpectralDataSchema.EndFrequency));
            }

            return scanFileInformationCollection;
        }

        public void DeleteOldSpectralMetadata(TimeSpan retentionTimeScanFile, TimeSpan retentionTimeRawIq)
        {
            RetryAzureTableOperations<RawSpectralDataSchema> operationContext = this.dataContext.RawSpectralDataSchemaOperations;
            operationContext.GetTableReference(AzureTableHelper.RawSpectralDataSchemaTable);

            string scanFileTypeQuery = TableQuery.GenerateFilterCondition("TypeId", QueryComparisons.Equal, Convert.ToString(FileType.ScanFile, CultureInfo.InvariantCulture));
            string scanFileTimeQuery = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, DateTime.UtcNow.Subtract(retentionTimeScanFile).Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture));
            string queryScanFile = TableQuery.CombineFilters(scanFileTypeQuery, TableOperators.And, scanFileTimeQuery);

            string rawFileTypeQuery = TableQuery.GenerateFilterCondition("TypeId", QueryComparisons.Equal, Convert.ToString(FileType.RawIqFile, CultureInfo.InvariantCulture));
            string rawFileTimeQuery = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, DateTime.UtcNow.Subtract(retentionTimeRawIq).Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture));
            string queryRawFile = TableQuery.CombineFilters(rawFileTypeQuery, TableOperators.And, rawFileTimeQuery);

            string query = TableQuery.CombineFilters(queryScanFile, TableOperators.Or, queryRawFile);

            IEnumerable<RawSpectralDataSchema> deleteEntites = operationContext.ExecuteQueryWithContinuation<RawSpectralDataSchema>(query);

            operationContext.DeleteEntities(deleteEntites.ToList());
        }

        public IEnumerable<Feedback> GetUnReadUserFeedbackCollection(DateTime feedbackSince)
        {
            RetryAzureTableOperations<Feedbacks> operationContext = this.dataContext.FeedbacksTableOperations;
            operationContext.GetTableReference(AzureTableHelper.FeedbacksTable);

            string startTime = feedbackSince.Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);

            string partionKeyQuery = TableQuery.GenerateFilterCondition(Constants.PartitionKey, QueryComparisons.Equal, Constants.DummyPartitionKey);
            string startTimeQuery = TableQuery.GenerateFilterCondition(Constants.RowKey, QueryComparisons.GreaterThanOrEqual, startTime);
            string readFilter = TableQuery.GenerateFilterConditionForBool("Read", QueryComparisons.Equal, false);
            string query = TableQuery.CombineFilters(partionKeyQuery, TableOperators.And, startTimeQuery);
            query = TableQuery.CombineFilters(query, TableOperators.And, readFilter);

            List<Feedback> userFeedbackCollection = new List<Feedback>();
            IEnumerable<Feedbacks> feedbackCollection = operationContext.ExecuteQueryWithContinuation<Feedbacks>(query);

            foreach (Feedbacks feedback in feedbackCollection)
            {
                Feedback userFeedback = new Feedback(feedback.FirstName, feedback.LastName, feedback.Email, feedback.Phone, feedback.Subject, feedback.Comment);
                userFeedback.TimeOfSubmission = feedback.TimeOfFeedback;
                userFeedbackCollection.Add(userFeedback);
            }

            return userFeedbackCollection;
        }

        public void MarkFeedbackAsRead(IList<Feedbacks> feedback)
        {
            if (feedback != null
                && feedback.Any())
            {
                this.dataContext.FeedbacksTableOperations.InsertOrReplaceEntities(feedback, true);
            }
        }

        public IEnumerable<RawSpectralDataInfo> GetRawSpectralDataSchema(Guid measurementStationId, FileType fileType)
        {
            if (measurementStationId != null)
            {
                string partitionKey = measurementStationId.ToString();

                RetryAzureTableOperations<RawSpectralDataSchema> operationContext = this.dataContext.RawSpectralDataSchemaOperations;
                operationContext.GetTableReference(AzureTableHelper.RawSpectralDataSchemaTable);

                string partitionKeyQuery = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
                string fileTypeQuery = TableQuery.GenerateFilterConditionForInt("TypeId", QueryComparisons.Equal, (int)fileType);

                string finalQuery = TableQuery.CombineFilters(partitionKeyQuery, TableOperators.And, fileTypeQuery);

                return operationContext.ExecuteQueryWithContinuation<RawSpectralDataSchema>(finalQuery)
                    .Select(x => new RawSpectralDataInfo(new Guid(x.PartitionKey), x.TimeStart, x.CompressionType, x.TypeId, x.BlobUri));
            }

            return null;
        }

        private RetryAzureTableOperations<SpectralDataSchema> GetSpectralDataSchemaOperationContext(Guid measurementStationKey, TimeRangeKind timeRangeKind)
        {
            string spectralDataTableName = AzureTableHelper.GetSpectralDataTableName(measurementStationKey, timeRangeKind);

            RetryAzureTableOperations<SpectralDataSchema> operationContext = this.dataContext.SpectralDataOperations;
            operationContext.GetTableReference(spectralDataTableName);

            return operationContext;
        }
    }
}
