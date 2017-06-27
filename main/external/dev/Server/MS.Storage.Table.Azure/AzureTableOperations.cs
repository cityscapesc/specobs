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

namespace Microsoft.Spectrum.Storage.Table.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Spectrum.Common;
    using Microsoft.WindowsAzure.Storage.Table;
    using CloudTableClient = Microsoft.WindowsAzure.Storage.Table.CloudTableClient;
    using StorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount;
    using StorageException = Microsoft.WindowsAzure.Storage.StorageException;

    /// <summary>
    /// /// -----------------------------------------------------------------
    /// Namespace:      MS.Storage.AzureTables
    /// Class:          AzureTableOperations 
    /// Description:    AzureTableOperations Class for Azure Table Operations
    /// -----------------------------------------------------------------     
    /// </summary>
    /// <typeparam name="TElement">TElement</typeparam>
    public class AzureTableOperations<TElement> where TElement : TableEntity
    {
        /// <summary>
        /// For info <see cref="http://msdn.microsoft.com/library/azure/dd179438.aspx"/>
        /// </summary>
        private const string TableBeingDeletedErrorCode = "TableBeingDeleted";

        private OptimisticCurrencyConflictHandler concurrencyHandler;

        /// <summary>
        /// variable to hold <see cref="CloudTableClient"/> value
        /// </summary>
        private CloudTableClient tableClient;

        public AzureTableOperations(StorageAccount storageAccount)
        {
            if (storageAccount == null)
            {
                throw new ArgumentNullException("storageAccount");
            }

            this.tableClient = storageAccount.CreateCloudTableClient();
        }

        /// <summary>
        /// variable to hold <see cref="CloudTable"/> value
        /// </summary>
        protected CloudTable Table { get; set; }

        /// <summary>
        /// Initialize Azure Table Operations
        /// </summary>
        /// <param name="tableName">Table Name</param>
        public void GetTableReference(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Invalid tableName", tableName);
            }

            this.Table = this.tableClient.GetTableReference(tableName);
            this.Table.CreateIfNotExists();
            this.concurrencyHandler = new OptimisticCurrencyConflictHandler(this.Table);

            this.CreateTableIfNotExist();
        }

        /// <summary>
        /// Safe Create Table If Not Exists
        /// </summary>
        /// <returns>true or false</returns>
        public bool SafeCreateTableIfNotExists()
        {
            do
            {
                try
                {
                    return this.Table.CreateIfNotExists();
                }
                catch (StorageException e)
                {
                    if (e.RequestInformation.HttpStatusCode == 409 && e.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableBeingDeletedErrorCode))
                    {
                        Thread.Sleep(1000); // The table is currently being deleted. Try again until it works.
                    }
                }
            }
            while (true);
        }

        /// <summary>
        /// Delete Entity
        /// </summary>
        /// <param name="entity">Table Entity</param>      
        public void DeleteEntity(TElement entity)
        {
            this.Table.Execute(TableOperation.Delete(entity));
        }

        /// <summary>
        /// Delete Entities
        /// </summary>
        /// <param name="entities">Table Entities</param>
        public void DeleteEntities(IList<TElement> entities)
        {
            if (entities == null)
            {
                throw new ArgumentException("entities can not be null", "entities");
            }

            TableBatchOperation batchOperation = new TableBatchOperation();

            for (int i = 0; i < entities.Count; i++)
            {
                batchOperation.Add(TableOperation.Delete(entities[i]));

                bool endOfCollection = i == (entities.Count - 1);

                if (batchOperation.Count == 100 || endOfCollection)
                {
                    this.Table.ExecuteBatch(batchOperation);
                    batchOperation.Clear();
                }
            }
        }

        /// <summary>
        /// Insert Entity
        /// </summary>
        /// <param name="entity">Table Entity</param>      
        public void InsertEntity(TElement entity)
        {
            this.Table.Execute(TableOperation.Insert(entity));
        }

        /// <summary>
        /// Insert Or Replace Entity
        /// </summary>
        /// <param name="entity">Table Entity</param>
        /// <param name="handleConcurrencyConflict">Boolean to enable Optimistic Concurrency Conflicts handling</param>
        public void InsertOrReplaceEntity(TElement entity, bool handleConcurrencyConflict)
        {
            if (entity == null)
            {
                throw new ArgumentException("entity can not be null", "entity");
            }

            TableOperation operation = TableOperation.InsertOrReplace(entity);

            if (handleConcurrencyConflict)
            {
                this.concurrencyHandler.InsertOrReplace(entity);
            }
            else
            {
                this.Table.Execute(operation);
            }
        }

        public void InsertOrMerge(TElement entity, bool handleConcurrencyConflict)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Entity can't be null", "entity");
            }

            TableOperation operation = TableOperation.InsertOrMerge(entity);

            if (handleConcurrencyConflict)
            {
                this.concurrencyHandler.InsertOrMerge(entity);
            }
        }

        /// <summary>
        /// Insert Entities
        /// </summary>
        /// <param name="entities">Table Entities</param>
        public void InsertEntities(IList<TElement> entities)
        {
            if (entities == null)
            {
                throw new ArgumentException("entities can not be null", "entities");
            }

            TableBatchOperation batchOperation = new TableBatchOperation();

            for (int i = 0; i < entities.Count; i++)
            {
                batchOperation.Add(TableOperation.Insert(entities[i]));

                bool endOfCollection = i == (entities.Count - 1);

                if (batchOperation.Count == 100 || endOfCollection)
                {
                    this.Table.ExecuteBatch(batchOperation);
                    batchOperation.Clear();
                }
            }
        }

        /// <summary>
        /// Insert Or Replace Entities
        /// </summary>
        /// <param name="entities">Table Entities</param>
        /// <param name="handleConcurrencyConflict">Boolean to enable Optimistic Concurrency Conflicts handling</param>
        public void InsertOrReplaceEntities(IList<TElement> entities, bool handleConcurrencyConflict)
        {
            if (entities == null)
            {
                throw new ArgumentException("entities can not be null", "entities");
            }

            if (handleConcurrencyConflict)
            {
                this.concurrencyHandler.InsertOrReplaceBatch(entities);
            }
            else
            {
                TableBatchOperation batchOperation = new TableBatchOperation();

                for (int i = 0; i < entities.Count; i++)
                {
                    batchOperation.Add(TableOperation.InsertOrReplace(entities[i]));

                    bool endOfCollection = i == (entities.Count - 1);

                    if (batchOperation.Count == 100 || endOfCollection)
                    {
                        this.Table.ExecuteBatch(batchOperation);

                        batchOperation.Clear();
                    }
                }
            }
        }

        public async Task InsertOrReplaceEntitiesAsync(IList<TElement> entities, bool handleConcurrencyConflict)
        {
            if (handleConcurrencyConflict)
            {
                // TODO: asynchronous call to handle concurrency conflict.
            }
            else
            {
                TableBatchOperation batchOperation = new TableBatchOperation();

                for (int i = 0; i < entities.Count; i++)
                {
                    batchOperation.Add(TableOperation.InsertOrReplace(entities[i]));

                    bool endOfCollection = i == (entities.Count - 1);

                    if (batchOperation.Count == 100 || endOfCollection)
                    {
                        IList<TableResult> tableResults = await this.Table.ExecuteBatchAsync(batchOperation);

                        batchOperation.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// GetByKeys
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="partitionKey">Partition Key</param>
        /// <param name="rowKey">Row Key</param>
        /// <returns>Enumerable Generic Type</returns>
        public IEnumerable<T> GetByKeys<T>(string partitionKey, string rowKey) where T : TableEntity, new()
        {
            return this.Table.CreateQuery<T>().Where(x => x.PartitionKey == partitionKey && x.RowKey == rowKey);
        }

        /// <summary>
        /// Get entities by partition keys
        /// </summary>
        /// <typeparam name="T">type of TableEntity</typeparam>
        /// <param name="partitionKey">partition key</param>
        /// <returns>list of entities</returns>
        public IEnumerable<T> GetByKeys<T>(string partitionKey) where T : TableEntity, new()
        {
            return this.Table.CreateQuery<T>().Where(x => x.PartitionKey == partitionKey);
        }

        /// <summary>
        /// Get Table Entities By Time.
        /// </summary>
        /// <typeparam name="T">T</typeparam>        
        /// <param name="measurementTime">Measurement Time</param>        
        /// <returns>Enumerable Generic Type</returns>
        public IEnumerable<T> GetDataByTime<T>(DateTime time) where T : SpectralDataSchema, new()
        {
            string ticks = time.ToUniversalTime().Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);

            string filterPartitionKey = TableQuery.GenerateFilterCondition(Constants.PartitionKey, QueryComparisons.Equal, ticks);
            string query = filterPartitionKey;

            return this.ExecuteQueryWithContinuation<T>(query);
        }

        /// <summary>
        /// Get Data Table Entity By Time and Frequency.
        /// </summary>
        /// <typeparam name="T">T</typeparam>        
        /// <param name="measurementTime">Measurement Time</param>
        /// <param name="startFrequency">Start Frequency</param>
        /// <param name="endFrequency">End Frequency</param>
        /// <returns>Enumerable Generic Type</returns>
        public IEnumerable<T> GetDataByTimeAndFrequency<T>(DateTime measurementTime, long startFrequency, long stopFrequency) where T : SpectralDataSchema, new()
        {
            string ticks = measurementTime.ToUniversalTime().Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);

            string filterPartitionKey = TableQuery.GenerateFilterCondition(Constants.PartitionKey, QueryComparisons.Equal, ticks);
            string query = filterPartitionKey;

            string startFreq = startFrequency.ToString(Constants.ElevenDigit, CultureInfo.InvariantCulture);
            string endFreq = stopFrequency.ToString(Constants.ElevenDigit, CultureInfo.InvariantCulture);

            string rowStartQuery = TableQuery.GenerateFilterCondition(Constants.RowKey, QueryComparisons.GreaterThanOrEqual, startFreq);
            string rowEndQuery = TableQuery.GenerateFilterCondition(Constants.RowKey, QueryComparisons.LessThanOrEqual, endFreq);

            query = TableQuery.CombineFilters(filterPartitionKey, TableOperators.And, rowStartQuery);
            query = TableQuery.CombineFilters(query, TableOperators.And, rowEndQuery);

            return this.ExecuteQueryWithContinuation<T>(query);
        }

        /// <summary>
        /// Get Table Entity by Keys with Table Query For Measurement Station By Frequency 
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="accessId">Access Id</param>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="frequency">Frequency</param>
        /// <returns>Enumerable Generic Type</returns>
        public IEnumerable<T> GetByKeysWithTableQueryForMeasurementStationByFrequency<T>(string accessId, DateTime startDate, DateTime endDate, long frequency) where T : TableEntity, new()
        {
            if (string.IsNullOrEmpty(accessId))
            {
                // Table Schema2
                string startDateTicks = startDate.ToUniversalTime().Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);
                string endDateTicks = endDate.ToUniversalTime().Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);
                string freq = frequency.ToString(Constants.ElevenDigit, CultureInfo.InvariantCulture);
                string partitionStartQuery = TableQuery.GenerateFilterCondition(Constants.PartitionKey, QueryComparisons.GreaterThanOrEqual, startDateTicks);
                string partitionEndQuery = TableQuery.GenerateFilterCondition(Constants.PartitionKey, QueryComparisons.LessThanOrEqual, endDateTicks);
                string rowStartQuery = TableQuery.GenerateFilterCondition(Constants.RowKey, QueryComparisons.Equal, freq);
                string query = TableQuery.CombineFilters(partitionStartQuery, TableOperators.And, partitionEndQuery);
                query = TableQuery.CombineFilters(query, TableOperators.And, rowStartQuery);
                TableQuery<T> tableQuery = new TableQuery<T>().Where(query);
                TableContinuationToken continuationToken = null;
                do
                {
                    var segementQuery = this.Table.ExecuteQuerySegmented(tableQuery, continuationToken, null);

                    foreach (var item in segementQuery.Results)
                    {
                        yield return item;
                    }

                    continuationToken = segementQuery.ContinuationToken;
                }
                while (continuationToken != null);
            }
            else
            {
                // Table Schema1               
                long startDateTicks = Convert.ToInt64(startDate.ToUniversalTime().Ticks);
                long endDateTicks = Convert.ToInt64(endDate.ToUniversalTime().Ticks);
                string freq = frequency.ToString(Constants.ElevenDigit, CultureInfo.InvariantCulture);
                long minuteTick = new TimeSpan(0, 1, 0).Ticks;

                for (long i = startDateTicks; i < endDateTicks; i = i + minuteTick)
                {
                    string partitionKey = TableQuery.GenerateFilterCondition(Constants.PartitionKey, QueryComparisons.Equal, accessId.ToLower(CultureInfo.InvariantCulture));
                    string rowKey = i.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture) + freq;
                    string rowQuery = TableQuery.GenerateFilterCondition(Constants.RowKey, QueryComparisons.Equal, rowKey);
                    string query = TableQuery.CombineFilters(partitionKey, TableOperators.And, rowQuery);
                    TableQuery<T> tableQuery = new TableQuery<T>().Where(query);
                    var queryResults = this.Table.ExecuteQuery(tableQuery);

                    foreach (var item in queryResults)
                    {
                        yield return item;
                    }
                }
            }
        }

        public IEnumerable<T> ExecuteQueryWithContinuation<T>(string query) where T : TableEntity, new()
        {
            TableQuery<T> tableQuery = new TableQuery<T>().Where(query);
            TableContinuationToken continuationToken = null;

            do
            {
                var segementQuery = this.Table.ExecuteQuerySegmented(tableQuery, continuationToken, null);

                foreach (var item in segementQuery.Results)
                {
                    yield return item;
                }

                continuationToken = segementQuery.ContinuationToken;
            }
            while (continuationToken != null);
        }

        /// <summary>
        /// Query entities by condition
        /// </summary>
        /// <typeparam name="T">type of ITableEntity</typeparam>
        /// <param name="query">condition</param>
        /// <returns>list of entities</returns>
        public IEnumerable<T> QueryEntities<T>(Func<T, bool> query) where T : ITableEntity, new()
        {
            return this.Table.CreateQuery<T>().Where(query);
        }

        /// <summary>
        /// Create Table If Not Exist
        /// </summary>       
        private void CreateTableIfNotExist()
        {   // Create the table if it doesn't exist.
            this.Table.CreateIfNotExists();
        }
    }
}