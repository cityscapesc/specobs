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
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// /// -----------------------------------------------------------------
    /// Namespace:      MS.Storage.AzureTables
    /// Class:          RetryAzureTableOperations 
    /// Description:    RetryAzureTableOperations Class for Azure Table Operations
    /// -----------------------------------------------------------------     
    /// </summary>
    /// <typeparam name="TElement">TElement</typeparam>
    public class RetryAzureTableOperations<TElement> where TElement : TableEntity
    {
        /// <summary>
        /// variable to hold <see cref="RetryPolicy"/> value
        /// </summary>
        private readonly RetryPolicy retryPolicy;

        /// <summary>
        /// variable to hold <see cref="AzureTableOperations"/> value
        /// </summary>
        private AzureTableOperations<TElement> azureTableOperations;

        /// <summary>
        /// Initializes a new instance of the <see cref="{TElement}"/> class
        /// </summary>
        /// <param name="retryPolicy">Retry Policy</param>
        public RetryAzureTableOperations(RetryPolicy retryPolicy, AzureTableOperations<TElement> azureTableOperations)
        {
            if (retryPolicy == null)
            {
                throw new ArgumentNullException("retryPolicy");
            }

            if (azureTableOperations == null)
            {
                throw new ArgumentNullException("azureTableOperations");
            }

            this.azureTableOperations = azureTableOperations;
            this.retryPolicy = retryPolicy;
        }

        /// <summary>
        /// Initialize Azure Table Operations
        /// </summary>
        /// <param name="tableName">Table Name</param>
        public void GetTableReference(string tableName)
        {
            this.retryPolicy.ExecuteAction(() =>
            this.azureTableOperations.GetTableReference(tableName));
        }

        /// <summary>
        /// Safe Create Table If Not Exists
        /// </summary>
        /// <returns>true or false</returns>
        public bool SafeCreateTableIfNotExists()
        {
            return this.retryPolicy.ExecuteAction(() =>
             this.azureTableOperations.SafeCreateTableIfNotExists());
        }

        /// <summary>
        /// Delete Entity
        /// </summary>
        /// <param name="entity">Table Entity</param>
        public void DeleteEntity(TElement entity)
        {
            this.retryPolicy.ExecuteAction(() =>
                this.azureTableOperations.DeleteEntity(entity));
        }

        /// <summary>
        /// Delete Entities
        /// </summary>
        /// <param name="entities">Table Entities</param>
        public void DeleteEntities(IList<TElement> entities)
        {
            this.retryPolicy.ExecuteAction(() =>
                this.azureTableOperations.DeleteEntities(entities));
        }

        /// <summary>
        /// Insert Entity
        /// </summary>
        /// <param name="entity">Table Entity</param>
        public void InsertEntity(TElement entity)
        {
            this.retryPolicy.ExecuteAction(() =>
                this.azureTableOperations.InsertEntity(entity));
        }

        /// <summary>
        /// Insert Or Replace Entity
        /// </summary>        
        /// <param name="entity">Table Entity</param>
        /// <param name="handleConcurrencyConflict">Boolean to enable Optimistic Concurrency Conflicts handling</param>
        public void InsertOrReplaceEntity(TElement entity, bool handleConcurrencyConflict)
        {
            this.retryPolicy.ExecuteAction(() =>
            this.azureTableOperations.InsertOrReplaceEntity(entity, handleConcurrencyConflict));
        }


        /// <summary>
        /// Insert Or Merge Entity
        /// </summary>        
        /// <param name="entity">Table Entity</param>
        /// <param name="handleConcurrencyConflict">Boolean to enable Optimistic Concurrency Conflicts handling</param>

        public void InsertOrMergeEntity(TElement entity, bool handleConcurrencyConflict)
        {
            this.retryPolicy.ExecuteAction(() =>
            this.azureTableOperations.InsertOrMerge(entity, handleConcurrencyConflict));
        }

        /// <summary>
        /// Insert Entities
        /// </summary>
        /// <param name="entities">Table Entities</param>
        public void InsertEntities(IList<TElement> entities)
        {
            this.retryPolicy.ExecuteAction(() =>
                this.azureTableOperations.InsertEntities(entities));
        }

        /// <summary>
        /// Insert Or Replace Entities
        /// </summary>
        /// <param name="entity">Table Entities</param>
        /// <param name="handleConcurrencyConflict">Boolean to enable Optimistic Concurrency Conflicts handling</param>
        public void InsertOrReplaceEntities(IList<TElement> entities, bool handleConcurrencyConflict)
        {
            this.retryPolicy.ExecuteAction(() =>
                this.azureTableOperations.InsertOrReplaceEntities(entities, handleConcurrencyConflict));
        }

        public async Task InsertOrReplaceEntitiesAsync(IList<TElement> entities, bool handleConcurrencyConflict)
        {
            await this.retryPolicy.ExecuteAsync(() =>
                this.azureTableOperations.InsertOrReplaceEntitiesAsync(entities, handleConcurrencyConflict));
        }

        /// <summary>
        /// Get By Keys
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="partitionKey">Partition Key</param>
        /// <param name="rowKey">Row Key</param>
        /// <returns>Enumerable Generic Type</returns>
        public IEnumerable<T> GetByKeys<T>(string partitionKey, string rowKey) where T : TableEntity, new()
        {
            return this.retryPolicy.ExecuteAction(() =>
               this.azureTableOperations.GetByKeys<T>(partitionKey, rowKey));
        }

        /// <summary>
        /// Get entities by partition keys
        /// </summary>
        /// <typeparam name="T">type of TableEntity</typeparam>
        /// <param name="partitionKey">partition key</param>
        /// <returns>list of entities</returns>
        public IEnumerable<T> GetByKeys<T>(string partitionKey) where T : TableEntity, new()
        {
            return this.retryPolicy.ExecuteAction(() =>
               this.azureTableOperations.GetByKeys<T>(partitionKey));
        }

        /// <summary>
        /// Get Data Table Entity By Time.
        /// </summary>
        /// <typeparam name="T">T</typeparam>        
        /// <param name="measurementTime">Measurement Time</param>        
        /// <returns>Enumerable Generic Type</returns>
        public IEnumerable<T> GetDataByTime<T>(DateTime measurementTime) where T : SpectralDataSchema, new()
        {
            return this.retryPolicy.ExecuteAction(() =>
                this.azureTableOperations.GetDataByTime<T>(measurementTime));
        }

        /// <summary>
        /// Get SpectralData Table Entity By Time and Frequency.
        /// </summary>
        /// <typeparam name="T">T</typeparam>        
        /// <param name="measurementTime">Measurement Time</param>
        /// <param name="startFrequency">Start Frequency</param>
        /// <param name="endFrequency">End Frequency</param>
        /// <returns>Enumerable Generic Type</returns>
        public IEnumerable<T> GetDataByTimeAndFrequency<T>(DateTime measurementTime, long startFrequency, long endFrequency) where T : SpectralDataSchema, new()
        {
            return this.retryPolicy.ExecuteAction(() =>
                this.azureTableOperations.GetDataByTimeAndFrequency<T>(measurementTime, startFrequency, endFrequency));
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
            return this.retryPolicy.ExecuteAction(() =>
                this.azureTableOperations.GetByKeysWithTableQueryForMeasurementStationByFrequency<T>(accessId, startDate, endDate, frequency));
        }

        /// <summary>
        /// Get the entities in the table matching the query passed in. Also do the continuation queries until there are no more items left
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<T> ExecuteQueryWithContinuation<T>(string query) where T : TableEntity, new()
        {
            return this.retryPolicy.ExecuteAction(() =>
                this.azureTableOperations.ExecuteQueryWithContinuation<T>(query));
        }

        /// <summary>
        /// Get the entities in the table matching the query passed in. DO NOT do the continuation queries.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public IEnumerable<T> QueryEntities<T>(Func<T, bool> query) where T : ITableEntity, new()
        {
            return this.retryPolicy.ExecuteAction(() =>
                this.azureTableOperations.QueryEntities<T>(query));
        }
    }
}
