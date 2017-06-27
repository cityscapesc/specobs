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
    using System.Linq;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Table;    

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage.Table.Azure
    /// Class:          SpectrumDataStorageAccountsTableOperations
    /// Description:    class containing operations to deal with user related tables
    /// ----------------------------------------------------------------- 
    public class SpectrumDataStorageAccountsTableOperations : ISpectrumDataStorageAccountsTableOperations
    {
        private static readonly object Mutex = new object();
        private static SpectrumDataStorageAccountsTableOperations instance;        
        private static bool initialized = false;

        private RetryAzureTableOperations<SpectrumDataStorageAccounts> storageAccountsTableContext;
        private Dictionary<string, CloudStorageAccount> cloudStorageAccounts;        

        /// <summary>
        /// Creates a new instance of the <see cref="SpectrumDataStorageAccountsTableOperations"/> class
        /// </summary>
        /// <param name="dataContext">data context containing table references</param>
        public SpectrumDataStorageAccountsTableOperations()
        {            
        }

        public static SpectrumDataStorageAccountsTableOperations Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SpectrumDataStorageAccountsTableOperations();
                }

                return instance;
            }
        }

        /// <summary>
        /// Initializes an instance of the <see cref="SpectrumDataStorageAccountsTableOperations"/> class
        /// </summary>
        /// <param name="dataContext">data context containing table references</param>
        public void Initialize(AzureTableDbContext master)
        {
            if (master == null)
            {
                throw new ArgumentNullException("master", "The azure table db context can not be null");
            }

            lock (SpectrumDataStorageAccountsTableOperations.Mutex)
            {
                if (!initialized)
                {                    
                    this.storageAccountsTableContext = master.SpectrumDataStorageAccountsOperations;
                    this.storageAccountsTableContext.GetTableReference(AzureTableHelper.SpectrumDataStorageAccountsTable);

                    this.CacheStorageAccountsFromTable();

                    initialized = true;
                }
            }
        }

        public List<CloudStorageAccount> GetAllCloudStorageAccounts()
        {
            return this.cloudStorageAccounts.Values.ToList();
        }

        public CloudStorageAccount GetCloudStorageAccountByName(string name)
        {
            return this.cloudStorageAccounts[name];
        }

        public string AddStationToAccount()
        {
            IEnumerable<SpectrumDataStorageAccounts> accounts = this.GetAllSpectrumDataStorageAccounts();
            string accountAssigned = string.Empty;

            foreach (SpectrumDataStorageAccounts account in accounts)
            {
                if (account.StationCount < account.MaxStationCount)
                {
                    accountAssigned = account.Name;
                    account.StationCount++;
                    this.storageAccountsTableContext.InsertOrReplaceEntity(account, true);
                    break;
                }
            }

            if (string.IsNullOrEmpty(accountAssigned))
            {
                throw new Exception("Out of space for new measurement stations in the storage accounts available, either add new storage accounts, or increase the maximum number of stations per storage account.");
            }

            return accountAssigned;
        }

        public void InsertEntity(SpectrumDataStorageAccounts entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity", "entity can not be null");
            }

            this.storageAccountsTableContext.InsertOrReplaceEntity(entity, true);
            this.cloudStorageAccounts.Add(entity.Name, new CloudStorageAccount(new StorageCredentials(entity.Name, entity.AccountKey), true));
        }

        private void CacheStorageAccountsFromTable()
        {            
            IEnumerable<SpectrumDataStorageAccounts> accounts = this.GetAllSpectrumDataStorageAccounts();

            this.cloudStorageAccounts = new Dictionary<string, CloudStorageAccount>();

            foreach (SpectrumDataStorageAccounts account in accounts)
            {
                this.cloudStorageAccounts.Add(account.Name, new CloudStorageAccount(new WindowsAzure.Storage.Auth.StorageCredentials(account.Name, account.AccountKey), true));
            }
        }

        private IEnumerable<SpectrumDataStorageAccounts> GetAllSpectrumDataStorageAccounts()
        {
            string query = TableQuery.GenerateFilterCondition(Constants.PartitionKey, QueryComparisons.Equal, Constants.DummyPartitionKey);
            return this.storageAccountsTableContext.ExecuteQueryWithContinuation<SpectrumDataStorageAccounts>(query);
        }
    }
}
