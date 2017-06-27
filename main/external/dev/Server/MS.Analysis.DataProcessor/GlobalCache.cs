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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.Spectrum.Storage.DataContracts;    
    using Microsoft.Spectrum.Storage.Queue.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Microsoft.WindowsAzure.Storage;   

    public class GlobalCache
    {
        private static GlobalCache instance;
        private CloudStorageAccount masterCloudStorageAccount;
        private AzureTableDbContext masterAzureTableDbContext;
        private RetryAzureTableOperations<Settings> settingsTable;
        private ILogger logger;
        private IMessageQueue azureWorkerQueue;
        private CloudBlobContainerName cloudLeaseBlobsContainerName;
        private SpectrumDataProcessorMetadataStorage masterSpectrumDataProcessorMetadataStorage;
        private UserManager userManager;
        private MeasurementStationTableOperations measurementStationOperations;
        private MeasurementStationManager measurementStationManager;
        private PortalTableOperations portalTableOperations;

        private GlobalCache()
        {
            bool resetQueues = false;

            this.masterCloudStorageAccount = CloudStorageAccount.Parse(ConnectionStringsUtility.StorageAccountConnectionString);

            this.masterAzureTableDbContext = new AzureTableDbContext(this.masterCloudStorageAccount, GlobalCache.GlobalRetryPolicy);

            SpectrumDataStorageAccountsTableOperations.Instance.Initialize(this.masterAzureTableDbContext);

            this.logger = new AzureLogger();

            this.settingsTable = this.masterAzureTableDbContext.SettingsOperations;
            this.settingsTable.GetTableReference(AzureTableHelper.SettingsTable);

            this.azureWorkerQueue = new AzureMessageQueue(ConnectionStringsUtility.WorkerQueueName, this.masterCloudStorageAccount, resetQueues);
            this.azureWorkerQueue = new RetryMessageQueue(this.azureWorkerQueue, GlobalCache.GlobalRetryPolicy);

            this.cloudLeaseBlobsContainerName = CloudBlobContainerName.Parse(ConnectionStringsUtility.LeaseContainer);

            this.masterSpectrumDataProcessorMetadataStorage = new SpectrumDataProcessorMetadataStorage(this.masterAzureTableDbContext, this.Logger);

            this.measurementStationOperations = new MeasurementStationTableOperations(this.masterAzureTableDbContext);
            this.measurementStationManager = new MeasurementStationManager(this.measurementStationOperations, SpectrumDataStorageAccountsTableOperations.Instance, this.logger);            

            UserManagementTableOperations userManagementTableOperations = new UserManagementTableOperations(this.masterAzureTableDbContext);
            this.userManager = new UserManager(userManagementTableOperations);

            this.portalTableOperations = new PortalTableOperations(this.masterAzureTableDbContext);
        }

        public static GlobalCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GlobalCache();
                }

                return instance;
            }
        }

        public static RetryPolicy GlobalRetryPolicy
        {
            get
            {
                RetryStrategy retryStrategy = new FixedInterval(3, TimeSpan.FromSeconds(1));
                RetryPolicy<StorageTransientErrorDetectionStrategy> retryPolicy = new RetryPolicy<StorageTransientErrorDetectionStrategy>(retryStrategy);

                return retryPolicy;
            }
        }

        public CloudStorageAccount MasterStorageAccount
        {
            get
            {
                return this.masterCloudStorageAccount;
            }
        }

        public AzureTableDbContext MasterAzureTableDbContext
        {
            get
            {
                return this.masterAzureTableDbContext;
            }
        }

        public IMessageQueue MainWorkerQueue
        {
            get
            {
                return this.azureWorkerQueue;
            }
        }

        public ILogger Logger
        {
            get
            {
                return this.logger;
            }
        }

        public CloudBlobContainerName LeaseBlobContainerName
        {
            get
            {
                return this.cloudLeaseBlobsContainerName;
            }
        }

        public RetryAzureTableOperations<Settings> SettingsTable
        {
            get
            {
                return this.settingsTable;
            }
        }

        public SpectrumDataProcessorMetadataStorage MasterSpectrumDataProcessorMetadataStorage
        {
            get
            {
                return this.masterSpectrumDataProcessorMetadataStorage;
            }
        }

        public UserManager UserManagementTableStorage
        {
            get
            {
                return this.userManager;
            }
        }

        public MeasurementStationTableOperations MeasurementStationOperations
        {
            get
            {
                return this.measurementStationOperations;
            }
        }

        public MeasurementStationManager MeasurementStationManager
        {
            get
            {
                return this.measurementStationManager;
            }
        }

        public PortalTableOperations PortalTableOperations
        {
            get
            {
                return this.portalTableOperations;
            }
        }
    }
}
