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

namespace Microsoft.Spectrum.Portal.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Table.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Microsoft.WindowsAzure.Storage;

    public class PortalGlobalCache
    {
        private static PortalGlobalCache instance;
        private CloudStorageAccount masterCloudStorageAccount;
        private AzureTableDbContext masterAzureTableDbContext;
        private RetryAzureTableOperations<Settings> settingsTable;
        private ILogger logger;
        private IUserManagementTableOperations userManagementTableOperations;
        private UserManager userManager;
        private IPortalTableOperations portalTableOperations;
        private PortalManager portalManager;
        private IMeasurementStationTableOperations measurementStationTableOperations;
        private IEnumerable<string> countries;
        private MeasurementStationManager measurementStationManager;

        private PortalGlobalCache()
        {
            this.masterCloudStorageAccount = CloudStorageAccount.Parse(ConnectionStringsUtility.StorageAccountConnectionString);

            this.masterAzureTableDbContext = new AzureTableDbContext(this.masterCloudStorageAccount, PortalGlobalCache.GlobalRetryPolicy);

            this.logger = new AzureLogger();

            this.settingsTable = this.masterAzureTableDbContext.SettingsOperations;

            this.settingsTable.GetTableReference(AzureTableHelper.SettingsTable);

            SpectrumDataStorageAccountsTableOperations.Instance.Initialize(this.masterAzureTableDbContext);

            this.measurementStationManager = new MeasurementStationManager(this.MeasurementStationTableOperation, SpectrumDataStorageAccountsTableOperations.Instance, this.Logger);
        }

        public static PortalGlobalCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PortalGlobalCache();
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

        public ILogger Logger
        {
            get
            {
                return this.logger;
            }
        }

        public RetryAzureTableOperations<Settings> SettingsTable
        {
            get
            {
                return this.settingsTable;
            }
        }

        public IUserManagementTableOperations UserManagementTableOperations
        {
            get
            {
                if (this.userManagementTableOperations == null)
                {
                    this.userManagementTableOperations = new UserManagementTableOperations(this.masterAzureTableDbContext);
                }

                return this.userManagementTableOperations;
            }
        }

        public UserManager UserManager
        {
            get
            {
                if (this.userManager == null)
                {
                    this.userManager = new UserManager(this.UserManagementTableOperations);
                }

                return this.userManager;
            }
        }

        public IPortalTableOperations PortalTableOperations
        {
            get
            {
                if (this.portalTableOperations == null)
                {
                    this.portalTableOperations = new PortalTableOperations(this.masterAzureTableDbContext);
                }

                return this.portalTableOperations;
            }
        }

        public PortalManager PortalManager
        {
            get
            {
                if (this.portalManager == null)
                {
                    this.portalManager = new PortalManager(this.PortalTableOperations);
                }

                return this.portalManager;
            }
        }

        public IMeasurementStationTableOperations MeasurementStationTableOperation
        {
            get
            {
                if (this.measurementStationTableOperations == null)
                {
                    this.measurementStationTableOperations = new MeasurementStationTableOperations(this.masterAzureTableDbContext);
                }

                return this.measurementStationTableOperations;
            }
        }

        public MeasurementStationManager StationManger
        {
            get
            {
                return this.measurementStationManager;
            }
        }

        public IEnumerable<string> Countries
        {
            get
            {
                if (this.countries == null || (this.countries != null && !this.countries.Any()))
                {
                    this.countries = Utility.GetCountries();
                }

                return this.countries;
            }
        }
    }
}