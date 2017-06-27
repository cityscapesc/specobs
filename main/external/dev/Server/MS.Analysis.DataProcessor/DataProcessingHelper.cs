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

namespace Microsoft.Spectrum.Analysis.DataProcessor
{
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.Spectrum.IO.ScanFile;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.Blob;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Table.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using Microsoft.WindowsAzure.Storage;
    using StationInfo = Microsoft.Spectrum.Storage.Models.MeasurementStationInfo;

    internal class DataProcessingHelper
    {
        public const string AsyncProcessingKey = "EnableAsyncProcessing";

        public static RetrySpectrumBlobStorage GetStationBlobStorage(CloudStorageAccount storageAccount, string measurementStationId)
        {
            CloudBlobContainerName cloudContainerName = CloudBlobContainerName.Parse(measurementStationId);

            AzureSpectrumBlobStorage spectrumBlockStorage = new AzureSpectrumBlobStorage(storageAccount, cloudContainerName, string.Empty);
            RetrySpectrumBlobStorage blobStorage = new RetrySpectrumBlobStorage(spectrumBlockStorage, GlobalCache.GlobalRetryPolicy);

            return blobStorage;
        }

        public static PessimisticLockingSpectrumDataProcessorStorage InitializePessimisticSpectrumDataProcessorStorage(CloudStorageAccount spectrumDataCloudStorageAccount)
        {
            AzureTableDbContext spectrumDataAzureTableDbContext = new AzureTableDbContext(spectrumDataCloudStorageAccount, GlobalCache.GlobalRetryPolicy);
            SpectrumDataProcessorMetadataStorage spectrumDataProcessorMetadataStorage = new SpectrumDataProcessorMetadataStorage(spectrumDataAzureTableDbContext, GlobalCache.Instance.Logger);

            SpectrumDataProcessorStorage spectrumDataProcessorStorage = new SpectrumDataProcessorStorage(spectrumDataProcessorMetadataStorage, GlobalCache.Instance.Logger);

            PessimisticLockingSpectrumDataProcessorStorage pessimesticLockingSpectrumDataProcessorStorage = new PessimisticLockingSpectrumDataProcessorStorage(
                spectrumDataProcessorStorage,  
                GlobalCache.Instance.LeaseBlobContainerName, 
                GlobalCache.Instance.MasterStorageAccount);

            return pessimesticLockingSpectrumDataProcessorStorage;
        }

        public static SpectrumDataProcessorStorage InitializeSpectrumDataProcessorStorage(CloudStorageAccount spectrumDataCloudStorageAccount)
        {
            AzureTableDbContext spectrumDataAzureTableDbContext = new AzureTableDbContext(spectrumDataCloudStorageAccount, GlobalCache.GlobalRetryPolicy);
            SpectrumDataProcessorMetadataStorage spectrumDataProcessorMetadataStorage = new SpectrumDataProcessorMetadataStorage(spectrumDataAzureTableDbContext, GlobalCache.Instance.Logger);

            return new SpectrumDataProcessorStorage(spectrumDataProcessorMetadataStorage, GlobalCache.Instance.Logger);
        }
    }
}
