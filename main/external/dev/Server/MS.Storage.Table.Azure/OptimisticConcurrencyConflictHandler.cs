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
    using System.Threading;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class OptimisticCurrencyConflictHandler
    {
        private const int OptimisticConcurrencyViolationStatusCode = 412;

        private readonly CloudTable cloudTable;

        public OptimisticCurrencyConflictHandler(CloudTable cloudTable)
        {
            if (cloudTable == null)
            {
                throw new ArgumentNullException("cloudTable");
            }

            this.cloudTable = cloudTable;
        }

        public void InsertOrReplace<T>(T entity) where T : TableEntity
        {
            TableOperation operation = TableOperation.Retrieve<T>(entity.PartitionKey, entity.RowKey);
            T latestEntity = (T)this.cloudTable.Execute(operation).Result;

            // Identify the given entity has to be inserted or replaced.
            if (latestEntity == null)
            {
                TableOperation insert = TableOperation.Insert(entity);
                this.cloudTable.Execute(insert);
            }
            else
            {
                entity.ETag = latestEntity.ETag;
                this.HandleConcurrencyForReplaceOperation<T>(entity);
            }
        }

        public void InsertOrMerge<T>(T entity) where T:TableEntity
        {
            TableOperation operation = TableOperation.Retrieve<T>(entity.PartitionKey, entity.RowKey);
            T latestEntity = (T)this.cloudTable.Execute(operation).Result;

            if(latestEntity == null)
            {
                TableOperation insert = TableOperation.Insert(entity);
                this.cloudTable.Execute(insert);
            }
            else
            {
                entity.ETag = latestEntity.ETag;
                this.HandleConcurrencyForMergeOperation<T>(entity);
            }
        }

        public void InsertOrReplaceBatch<T>(IList<T> entities) where T : TableEntity
        {
            if (entities == null)
            {
                throw new ArgumentException("entities can not be null", "entities");
            }

            TableBatchOperation batchInsert = new TableBatchOperation();
            List<T> latestEntities = new List<T>();

            // Categorize entities for BatchInsert and BatchReplace operations.
            for (int i = 0; i < entities.Count; i++)
            {
                T entity = entities[i];

                TableOperation retrive = TableOperation.Retrieve<T>(entity.PartitionKey, entity.RowKey);
                T latestEntity = (T)this.cloudTable.Execute(retrive).Result;

                if (latestEntity == null)
                {
                    batchInsert.Add(TableOperation.Insert(entity));
                }
                else
                {
                    entity.ETag = latestEntity.ETag;
                    latestEntities.Add(entity);
                }

                bool endOfCollection = i == (entities.Count - 1);

                if (batchInsert.Count == 100 || (endOfCollection || batchInsert.Count > 0))
                {
                    this.cloudTable.ExecuteBatch(batchInsert);
                    batchInsert.Clear();
                }

                if (latestEntities.Count == 100 || (endOfCollection || latestEntities.Count > 0))
                {
                    this.HandleConcurrencyForReplaceBatchOperation<T>(latestEntities);
                    latestEntities.Clear();
                }
            }
        }

        private void HandleConcurrencyForMergeOperation<T>(T entity) where T:TableEntity
        {
            bool optimisticConcurrencyViolation = false;

            do
            {
                try
                {
                    TableOperation replace = TableOperation.Merge(entity);

                    if (optimisticConcurrencyViolation)
                    {
                        // Update entity ETag to its latest ETag.
                        this.UpdateToLatestEtag<T>(entity);
                    }

                    this.cloudTable.Execute(replace);
                    optimisticConcurrencyViolation = false;
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode != OptimisticConcurrencyViolationStatusCode)
                    {
                        throw;
                    }

                    optimisticConcurrencyViolation = true;

                    // Do we need to wait for sometime here?
                    Thread.Sleep(1000);
                }
            }
            while (optimisticConcurrencyViolation);
        }

        private void HandleConcurrencyForReplaceOperation<T>(T entity) where T : TableEntity
        {
            bool optimisticConcurrencyViolation = false;

            do
            {
                try
                {
                    TableOperation replace = TableOperation.Replace(entity);

                    if (optimisticConcurrencyViolation)
                    {
                        // Update entity ETag to its latest ETag.
                        this.UpdateToLatestEtag<T>(entity);
                    }

                    this.cloudTable.Execute(replace);
                    optimisticConcurrencyViolation = false;
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode != OptimisticConcurrencyViolationStatusCode)
                    {
                        throw;
                    }

                    optimisticConcurrencyViolation = true;

                    // Do we need to wait for sometime here?
                    Thread.Sleep(1000);
                }
            }
            while (optimisticConcurrencyViolation);
        }

        private void HandleConcurrencyForReplaceBatchOperation<T>(IList<T> entities) where T : TableEntity
        {
            bool optimisticConcurrencyViolation = false;

            do
            {
                TableBatchOperation batchReplace = new TableBatchOperation();

                foreach (T entity in entities)
                {
                    TableOperation replace = TableOperation.Replace(entity);
                    batchReplace.Add(replace);
                }

                try
                {
                    if (optimisticConcurrencyViolation)
                    {
                        // Update entities ETags to their latest ETags.
                        this.UpdateToLastestEtags<T>(entities);
                    }

                    // [Investigation Required:] Not sure, is it a good idea to update entities Etags and execute batch replace operation? This might probably
                    // lead to an infinite loop, if different entities in a batch are updated by different threads.
                    this.cloudTable.ExecuteBatch(batchReplace);
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode != OptimisticConcurrencyViolationStatusCode)
                    {
                        throw;
                    }

                    optimisticConcurrencyViolation = true;

                    // Do we need to wait for sometime here?
                    Thread.Sleep(1000);
                }
            }
            while (optimisticConcurrencyViolation);
        }

        private void UpdateToLatestEtag<T>(T entity) where T : TableEntity
        {
            TableOperation operation = TableOperation.Retrieve<T>(entity.PartitionKey, entity.RowKey);
            T lastestEntity = (T)this.cloudTable.Execute(operation).Result;

            entity.ETag = lastestEntity.ETag;
        }

        private void UpdateToLastestEtags<T>(IList<T> entities) where T : TableEntity
        {
            TableBatchOperation batchRetrive = new TableBatchOperation();
            foreach (T entity in entities)
            {
                TableOperation retrive = TableOperation.Retrieve<T>(entity.PartitionKey, entity.RowKey);
                batchRetrive.Add(retrive);
            }

            IList<TableResult> latestEntities = this.cloudTable.ExecuteBatch(batchRetrive);

            for (int i = 0; i < latestEntities.Count; i++)
            {
                entities[i].ETag = latestEntities[i].Etag;
            }
        }
    }
}
