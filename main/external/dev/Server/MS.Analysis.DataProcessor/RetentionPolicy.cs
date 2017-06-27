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

    public class RetentionPolicy
    {
        private static RetentionPolicy instance;        
        private ILogger logger;        

        private RetentionPolicy()
        {
            this.logger = GlobalCache.Instance.Logger;
        }

        public static RetentionPolicy Instance 
        {
            get
            {
                if (instance == null)
                {
                    instance = new RetentionPolicy();
                }

                return instance;
            }
        }                

        public void ExecuteContainerStorageRetentionPolicy()
        {
            this.logger.Log(TraceEventType.Start, LoggingMessageId.RetentionPolicy, string.Format(CultureInfo.InvariantCulture, "Retention policy started at {0}", DateTime.UtcNow));

            try
            {
                TimeSpan retentionTimeDsox = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.RetentionPolicyCategory, "retentionTimeDsox", TimeSpan.FromDays(365 * 2));
                TimeSpan retentionTimeDsor = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.RetentionPolicyCategory, "retentionTimeDsor", TimeSpan.FromDays(7));
                TimeSpan retentionTimeDsol = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.RetentionPolicyCategory, "retentionTimeDsol", TimeSpan.FromDays(7));
                TimeSpan retentionTimeLck = SettingsTableHelper.Instance.GetSetting(SettingsTableHelper.RetentionPolicyCategory, "retentionTimeLck", TimeSpan.FromDays(1));

                List<CloudStorageAccount> accounts = SpectrumDataStorageAccountsTableOperations.Instance.GetAllCloudStorageAccounts();

                foreach (CloudStorageAccount account in accounts)
                {
                    try
                    {
                        CloudBlobClient blobClient = account.CreateCloudBlobClient();
                        IEnumerable<CloudBlobContainer> containers = blobClient.ListContainers();

                        foreach (CloudBlobContainer container in containers)
                        {
                            // Now we will setup Blob access condition option which will filter all the blobs which are not modified for X (retentionTime) time
                            AccessCondition accessNotModifiedDsox = AccessCondition.GenerateIfNotModifiedSinceCondition(DateTime.UtcNow.Subtract(retentionTimeDsox));
                            AccessCondition accessNotModifiedDsor = AccessCondition.GenerateIfNotModifiedSinceCondition(DateTime.UtcNow.Subtract(retentionTimeDsor));
                            AccessCondition accessNotModifiedDsol = AccessCondition.GenerateIfNotModifiedSinceCondition(DateTime.UtcNow.Subtract(retentionTimeDsol));
                            AccessCondition accessNotModifiedLck = AccessCondition.GenerateIfNotModifiedSinceCondition(DateTime.UtcNow.Subtract(retentionTimeLck));

                            // Lets Query to do its job
                            IEnumerable<IListBlobItem> blobs = container.ListBlobs(string.Empty, true, BlobListingDetails.Metadata, null, null);

                            foreach (IListBlobItem blob in blobs)
                            {
                                try
                                {
                                    string blobName = blob.Uri.Segments[blob.Uri.Segments.Length - 1];                                  

                                    if (blobName.EndsWith(RawIqFile.Extension, StringComparison.OrdinalIgnoreCase))
                                    {
                                        container.GetBlockBlobReference(blobName).Delete(DeleteSnapshotsOption.None, accessNotModifiedDsor);
                                    }
                                    else if (blobName.EndsWith(ScanFile.Extension, StringComparison.OrdinalIgnoreCase))
                                    {
                                        container.GetBlockBlobReference(blobName).Delete(DeleteSnapshotsOption.None, accessNotModifiedDsox);
                                    }
                                    else if (blobName.EndsWith(".lck", StringComparison.OrdinalIgnoreCase))
                                    {
                                        container.GetBlockBlobReference(blobName).Delete(DeleteSnapshotsOption.None, accessNotModifiedLck);
                                    }
                                    else if (blobName.EndsWith(FileLogger.Extension, StringComparison.OrdinalIgnoreCase))
                                    {
                                        container.GetBlockBlobReference(blobName).Delete(DeleteSnapshotsOption.None, accessNotModifiedDsol);
                                    }                                    

                                    /*
                                    ICloudBlob cloudBlob = container.GetBlobReferenceFromServer(blob.Uri.ToString());                
                                    cloudBlob.FetchAttributes();

                                    if (cloudBlob.Properties.LastModified < deleteFilesOlderThanThisDate)
                                    {
                                        cloudBlob.Delete();
                                    }
                                     */
                                }
                                catch (StorageException)
                                {
                                    // This is expected for items that we want deleted, so don't log anything here
                                }                                                               
                            }
                        }

                        /* We can't do this since it could take longer to clean up this data than the time we have with a single thread
                        * This is bacause we will likely be inserting spectral data faster than we can clean it up out of the tebles
                        // Delete the aggregated data stored in various tables
                        CloudTableClient spectrumDataTableClient = account.CreateCloudTableClient();                              
                                
                        foreach (var table in spectrumDataTableClient.ListTables())
                        {
                            spectrumDataProcessorMetadataStorage.DeleteOldSpectralData(table.Name, retentionTimeDsox);                                    
                        }                                
                        */

                        // Now make sure to go and delete all of the entries our of the RawSpectralDataSchema table    
                        AzureTableDbContext spectrumDataAzureTableDbContext = new AzureTableDbContext(account, GlobalCache.GlobalRetryPolicy);
                        SpectrumDataProcessorMetadataStorage spectrumDataProcessorMetadataStorage = new SpectrumDataProcessorMetadataStorage(spectrumDataAzureTableDbContext, GlobalCache.Instance.Logger);
                        spectrumDataProcessorMetadataStorage.DeleteOldSpectralMetadata(retentionTimeDsox, retentionTimeDsor);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Log(TraceEventType.Error, LoggingMessageId.RetentionPolicy, string.Format(CultureInfo.InvariantCulture, "Unable to access storage account = {0}, {1}", account.Credentials.AccountName, ex.ToString()));
                    }                    
                }
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.RetentionPolicy, string.Format(CultureInfo.InvariantCulture, "Unable to access storage accounts table = {0}", ex.ToString()));
            }

            this.logger.Log(TraceEventType.Stop, LoggingMessageId.RetentionPolicy, string.Format(CultureInfo.InvariantCulture, "Retention policy ended at {0}", DateTime.UtcNow));
        }        
    }
}
