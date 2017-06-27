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
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;    
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading;    
    using System.Threading.Tasks;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.RawIqFile;
    using Microsoft.Spectrum.IO.ScanFile;
    using Microsoft.Spectrum.Storage.Queue.Azure;    
    using Microsoft.Spectrum.Storage.Table.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;          
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Queue.Protocol;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;    

    public class UpdateTableSharedAccess
    {
        private static UpdateTableSharedAccess instance;

        private ILogger logger;
        private RetryAzureTableOperations<TableSharedAccessSignatures> sharedAccessTableContext;

        private UpdateTableSharedAccess()
        {
            this.logger = GlobalCache.Instance.Logger;
            this.sharedAccessTableContext = GlobalCache.Instance.MasterAzureTableDbContext.TableSharedAccessSignaturesTableOperations;
            this.sharedAccessTableContext.GetTableReference(AzureTableHelper.TableSharedAccessSignaturesTable);
        }

        public static UpdateTableSharedAccess Instance 
        {
            get
            {
                if (instance == null)
                {
                    instance = new UpdateTableSharedAccess();                    
                }

                return instance;
            }
        }        

        public void ExecuteUpdateTableSharedAccess()
        {
            this.logger.Log(TraceEventType.Start, LoggingMessageId.UpdateTableSharedData, string.Format(CultureInfo.InvariantCulture, "Update table shared access started at {0}", DateTime.UtcNow));

            try
            {               
                List<CloudStorageAccount> accounts = SpectrumDataStorageAccountsTableOperations.Instance.GetAllCloudStorageAccounts();                

                // Make the policy basically indefinite
                SharedAccessTablePolicy policy = new SharedAccessTablePolicy();
                policy.Permissions = SharedAccessTablePermissions.Query;
                policy.SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1000);

                foreach (CloudStorageAccount account in accounts)
                {
                    try
                    {
                        List<TableSharedAccessSignatures> existingSignatures = this.sharedAccessTableContext.GetByKeys<TableSharedAccessSignatures>(account.Credentials.AccountName).ToList();
                        CloudTableClient blobClient = account.CreateCloudTableClient();
                        IEnumerable<CloudTable> tables = blobClient.ListTables();

                        foreach (CloudTable table in tables)
                        {
                            // filter out any private tables
                            if (!AzureTableHelper.IsPrivateTable(table.Name))
                            {
                                if (!UpdateTableSharedAccess.ContainsTableName(existingSignatures, account.Credentials.AccountName, table.Name))
                                {
                                    // if we don't have the table name in our table yet, then create the shared access signature
                                    string sharedAccessSignature = table.GetSharedAccessSignature(policy, "PublicTableSharedAccessReadOnlyPolicy");

                                    // add this table to our table of shared access signatures
                                    this.sharedAccessTableContext.InsertEntity(new TableSharedAccessSignatures(account.Credentials.AccountName, table.Name, sharedAccessSignature));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.Log(TraceEventType.Error, LoggingMessageId.UpdateTableSharedData, string.Format(CultureInfo.InvariantCulture, "Unable to access storage account = {0}, {1}", account.Credentials.AccountName, ex.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.UpdateTableSharedData, string.Format(CultureInfo.InvariantCulture, "Unable to access storage accounts table = {0}", ex.ToString()));
            }

            this.logger.Log(TraceEventType.Stop, LoggingMessageId.UpdateTableSharedData, string.Format(CultureInfo.InvariantCulture, "Update table shared access ended at {0}", DateTime.UtcNow));
        }    
    
        private static bool ContainsTableName(List<TableSharedAccessSignatures> existingSignatures, string storageAccountName, string tableName)
        {
            for (int i = 0; i < existingSignatures.Count; i++)
            {
                if ((existingSignatures[i].StorageAccountName == storageAccountName) && (existingSignatures[i].TableName == tableName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
