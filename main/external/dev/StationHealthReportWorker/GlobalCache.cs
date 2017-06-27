namespace Microsoft.Spectrum.StationHealthReportWorker
{
    using System;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Table.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Microsoft.WindowsAzure.Storage;

    internal class GlobalCache
    {
        private static GlobalCache instance;
        private CloudStorageAccount masterCloudStorageAccount;
        private AzureTableDbContext masterAzureTableDbContext;
        private RetryAzureTableOperations<Settings> settingsTable;
        private ILogger logger;
        private UserManager userManager;
        private MeasurementStationTableOperations measurementStationOperations;
        private MeasurementStationManager measurementStationManager;

        private GlobalCache()
        {
            this.masterCloudStorageAccount = CloudStorageAccount.Parse(ConnectionStringsUtility.StorageAccountConnectionString);

            this.masterAzureTableDbContext = new AzureTableDbContext(this.masterCloudStorageAccount, GlobalCache.GlobalRetryPolicy);

            SpectrumDataStorageAccountsTableOperations.Instance.Initialize(this.masterAzureTableDbContext);

            this.logger = new AzureLogger();

            this.settingsTable = this.masterAzureTableDbContext.SettingsOperations;
            this.settingsTable.GetTableReference(AzureTableHelper.SettingsTable);

            this.measurementStationOperations = new MeasurementStationTableOperations(this.masterAzureTableDbContext);
            this.measurementStationManager = new MeasurementStationManager(this.measurementStationOperations, SpectrumDataStorageAccountsTableOperations.Instance, this.logger);

            UserManagementTableOperations userManagementTableOperations = new UserManagementTableOperations(this.masterAzureTableDbContext);
            this.userManager = new UserManager(userManagementTableOperations);
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

        public ILogger Logger
        {
            get
            {
                return this.logger;
            }
        }

        public string HealthReportServiceBusConnectionString
        {
            get
            {
                return ConnectionStringsUtility.HealthReportServiceBusConnectionString;
            }
        }

        public RetryAzureTableOperations<Settings> SettingsTable
        {
            get
            {
                return this.settingsTable;
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
    }
}

