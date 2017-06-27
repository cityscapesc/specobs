// Copyright (c) Microsoft Corporation
//
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use file except in compliance 
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
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Models;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    public class PessimisticLockingSpectrumDataProcessorStorage
    {
        private const string LeaseAlreadyPresentErrorCode = "LeaseAlreadyPresent";
        private const string LeaseIdMismatchWithLeaseOperationErrorcode = "LeaseIdMismatchWithLeaseOperation";
        private const int Conflict409 = (int)HttpStatusCode.Conflict;

        private const string MaxLeaseWaitTimeConfigName = "MaxLeaseWaitTime";
        private const string LastLeaseModifiedMetadata = "LastLeaseModified";
        private const int MaxRetryIntervalInSeconds = 128;

        private readonly SpectrumDataProcessorStorage spectrumDataProcessorStorage;
        private readonly Lazy<CloudBlobContainer> leaseBlobContainer;

        public PessimisticLockingSpectrumDataProcessorStorage(SpectrumDataProcessorStorage spectrumDataProcessorStorage, CloudBlobContainerName leaseBlobContainerName, CloudStorageAccount masterCloudStorageAccount)
        {
            if (spectrumDataProcessorStorage == null)
            {
                throw new ArgumentNullException("spectrumDataProcessorStorage");
            }

            if (leaseBlobContainerName == null)
            {
                throw new ArgumentNullException("leaseBlobContainerName");
            }

            if (masterCloudStorageAccount == null)
            {
                throw new ArgumentNullException("masterCloudStorageAccount");
            }

            this.spectrumDataProcessorStorage = spectrumDataProcessorStorage;
            this.leaseBlobContainer = new Lazy<CloudBlobContainer>(() =>
                {
                    CloudBlobClient blobClient = masterCloudStorageAccount.CreateCloudBlobClient();
                    CloudBlobContainer blobContainer = blobClient.GetContainerReference(leaseBlobContainerName.Value);
                    blobContainer.CreateIfNotExists();
                    return blobContainer;
                });
        }

        public IEnumerable<SpectrumFrequency> GetSpectralData(Guid measurementStationId, TimeRangeKind timeGranularity, DateTime timeStart)
        {
            return this.spectrumDataProcessorStorage.GetSpectralData(measurementStationId, timeGranularity, timeStart);
        }

        public void InsertOrUpdateSpectralData(SpectrumCalibration spectralData)
        {
            string leaseBlobName = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}.lck", spectralData.MeasurementStationId.ToString(), spectralData.TimeRangeKind.ToString(), spectralData.TimeStart.ToString());

            string proposedLeaseId = Guid.NewGuid().ToString();

            this.ExecuteTableOperationWithLock(
                leaseBlobName,
                () =>
                {
                    this.spectrumDataProcessorStorage.InsertOrUpdateSpectralData(spectralData);
                },
                null,
                proposedLeaseId);
        }

        public async Task InsertOrUpdateSpectralDataAsync(SpectrumCalibration spectralData)
        {
            string leaseBlobName = string.Format("{0}_{1}_{2}.lck", spectralData.MeasurementStationId.ToString(), spectralData.TimeRangeKind.ToString(), spectralData.TimeStart.ToString());

            string proposedLeaseId = Guid.NewGuid().ToString();

            await this.ExecuteTableOperationWithLockAsync(
                leaseBlobName,
                () =>
                {
                    return this.spectrumDataProcessorStorage.InsertOrUpdateSpectralDataAsync(spectralData);
                },
                null,
                proposedLeaseId);
        }

        public void InsertOrUpdateScanFileInformation(ScanFileInformation scanFileInformation)
        {
            this.spectrumDataProcessorStorage.InsertOrUpdateScanFileInformation(scanFileInformation);
        }

        public void ExecuteTableOperationWithLock(string leaseBlobName, Action action, TimeSpan? leaseExpirationTime, string proposedLeaseId)
        {
            int retryIntervalInSeconds = 1;
            const int MaxRetryIntervalInSeconds = 128;

            if (action == null)
            {
                throw new ArgumentException("action can not be null", "action");
            }

            if (!PessimisticLockingSpectrumDataProcessorStorage.ValidateLeaseExpirationTime(leaseExpirationTime))
            {
                throw new ArgumentOutOfRangeException("leaseExpirationTime", "Lease time should be grater than or equal to 15 seconds and less than or equal to 60 seconds");
            }

            CloudBlockBlob blob = this.leaseBlobContainer.Value.GetBlockBlobReference(leaseBlobName);
            string leaseId = string.Empty;

            TimeSpan maxLeaseWaitTime = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.ConcurrencyHandlerCategory, MaxLeaseWaitTimeConfigName, TimeSpan.FromHours(1));

            do
            {
                try
                {
                    if (!blob.Exists())
                    {
                        blob.UploadText(string.Empty);
                    }

                    // To ensure blob properties accessed are latest.
                    blob.FetchAttributes();

                    // Considering delay in acquiring lease because of retry, we should keep lastLeaseModified time as latest as possible.
                    DateTimeOffset initialAttempt = DateTimeOffset.UtcNow;

                    // Break lease for the blob whose LastModified time equal to or exceeds maxLeaseWaitTime.
                    if (blob.Properties.LeaseStatus == LeaseStatus.Locked && (initialAttempt.Subtract(blob.Properties.LastModified.Value) >= maxLeaseWaitTime))
                    {
                        BreakLease(blob, TimeSpan.FromSeconds(0));
                    }

                    leaseId = blob.AcquireLease(leaseExpirationTime, proposedLeaseId);

                    // Idea is to keep LastModified property of blob latest by updating a Metadata field LastLeaseModified.
                    blob.Metadata[LastLeaseModifiedMetadata] = initialAttempt.ToString();
                    blob.SetMetadata(AccessCondition.GenerateLeaseCondition(leaseId));

                    action();

                    blob.ReleaseLease(AccessCondition.GenerateLeaseCondition(leaseId));
                }
                catch (StorageException ex)
                {
                    blob.FetchAttributes();

                    if (ex.RequestInformation.HttpStatusCode != Conflict409 && ex.RequestInformation.ExtendedErrorInformation.ErrorCode != LeaseAlreadyPresentErrorCode)
                    {
                        if (string.IsNullOrEmpty(leaseId))
                        {
                            blob.ReleaseLease(AccessCondition.GenerateLeaseCondition(leaseId));
                        }
                        else if (blob.Properties.LeaseStatus == LeaseStatus.Locked)
                        {
                            BreakLease(blob, leaseExpirationTime);
                        }

                        blob.DeleteIfExists();

                        throw;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(retryIntervalInSeconds));

                    if (retryIntervalInSeconds < MaxRetryIntervalInSeconds)
                    {
                        retryIntervalInSeconds = retryIntervalInSeconds * 2;
                    }
                }
            }
            while (blob.Properties.LeaseStatus == LeaseStatus.Locked);
        }

        public async Task ExecuteTableOperationWithLockAsync(string leaseBlobName, Func<Task> action, TimeSpan? leaseExpirationTime, string proposedLeaseId)
        {
            int retryIntervalInSeconds = 1;
            const int MaxRetryIntervalInSeconds = 128;

            if (action == null)
            {
                throw new ArgumentException("action can not be null", "action");
            }

            if (!PessimisticLockingSpectrumDataProcessorStorage.ValidateLeaseExpirationTime(leaseExpirationTime))
            {
                throw new ArgumentOutOfRangeException("leaseExpirationTime", "Lease time should be grater than or equal to 15 seconds and less than or equal to 60 seconds");
            }

            CloudBlockBlob blob = this.leaseBlobContainer.Value.GetBlockBlobReference(leaseBlobName);
            string leaseId = string.Empty;

            TimeSpan maxLeaseWaitTime = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.ConcurrencyHandlerCategory, MaxLeaseWaitTimeConfigName, TimeSpan.FromHours(1));

            do
            {
                try
                {
                    if (!blob.Exists())
                    {
                        blob.UploadText(string.Empty);
                    }

                    // To ensure blob properties accessed are latest.
                    blob.FetchAttributes();

                    // Considering delay in acquiring lease because of retry, we should keep lastLeaseModified time as latest as possible.
                    DateTimeOffset lastLeaseModified = DateTimeOffset.UtcNow;

                    if (blob.Properties.LeaseStatus == LeaseStatus.Locked && (lastLeaseModified.Subtract(blob.Properties.LastModified.Value) >= maxLeaseWaitTime))
                    {
                        BreakLease(blob, leaseExpirationTime);
                    }

                    leaseId = blob.AcquireLease(leaseExpirationTime, proposedLeaseId);

                    // Idea is to keep LastModified property of blob latest by updating a Metadata field LastLeaseModified.
                    blob.Metadata[LastLeaseModifiedMetadata] = lastLeaseModified.ToString();
                    blob.SetMetadata(AccessCondition.GenerateLeaseCondition(leaseId));

                    await action();

                    blob.ReleaseLease(AccessCondition.GenerateLeaseCondition(leaseId));
                }
                catch (StorageException ex)
                {
                    blob.FetchAttributes();

                    if (ex.RequestInformation.HttpStatusCode != Conflict409 && ex.RequestInformation.ExtendedErrorInformation.ErrorCode != LeaseAlreadyPresentErrorCode)
                    {
                        if (string.IsNullOrEmpty(leaseId))
                        {
                            blob.ReleaseLease(AccessCondition.GenerateLeaseCondition(leaseId));
                        }
                        else if (blob.Properties.LeaseStatus == LeaseStatus.Locked)
                        {
                            BreakLease(blob, leaseExpirationTime);
                        }

                        blob.DeleteIfExists();

                        throw;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(retryIntervalInSeconds));

                    if (retryIntervalInSeconds < MaxRetryIntervalInSeconds)
                    {
                        retryIntervalInSeconds = retryIntervalInSeconds * 2;
                    }
                }
            }
            while (blob.Properties.LeaseStatus == LeaseStatus.Locked);
        }

        public void PersistScanFileProcessingError(ScanFileProcessingError scanFileProcessingError)
        {
            this.spectrumDataProcessorStorage.PersistScanFileProcessingError(scanFileProcessingError);
        }

        public IEnumerable<ScanFileProcessingError> GetAllSpectrumFileProcessingFailures(Guid measurementStationId, DateTime failuresSince)
        {
            return this.spectrumDataProcessorStorage.GetAllSpectrumFileProcessingFailures(measurementStationId, failuresSince);
        }

        public IEnumerable<ScanFileInformation> GetSpectralMetadataInfo(Guid measurementStationId, DateTime timeStart)
        {
            return this.spectrumDataProcessorStorage.GetSpectralMetadataInfo(measurementStationId, timeStart);
        }

        private static void BreakLease(CloudBlockBlob blob, TimeSpan? leaseExpirationTime)
        {
            // For Break Lease on an infinite lease, the default behavior is to break the lease immediately.
            TimeSpan waitTime = blob.BreakLease(leaseExpirationTime);

            // Wait until leaseExpirationTime
            Thread.Sleep(waitTime);
        }
         
        private static bool ValidateLeaseExpirationTime(TimeSpan? leaseExpirationTime)
        {
            // [Note] leaseExpirationTime having null value is valid, as it allows to acquire infinite lease.
            if (leaseExpirationTime.HasValue)
            {
                return leaseExpirationTime.Value.TotalSeconds >= 15 && leaseExpirationTime.Value.TotalSeconds <= 60;
            }

            return true;
        }
    }
}
