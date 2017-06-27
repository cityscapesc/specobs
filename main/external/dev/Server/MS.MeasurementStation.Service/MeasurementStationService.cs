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

namespace Microsoft.Spectrum.MeasurementStation.Service
{
    using Common.Enums;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.Spectrum.MeasurementStation.Contract;
    using Microsoft.Spectrum.Storage.Enums;
    using Microsoft.Spectrum.Storage.Queue.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading.Tasks;

    public class MeasurementStationService : IMeasurementStationService
    {
        private RetryMessageQueue workerQueue = null;
        private RetryMessageQueue healthReportQueue = null;
        private RetryAzureTableOperations<MeasurementStationsPublic> measurementStationPublicTableContext;
        private ILogger logger;

        // NB: There is no default constructor necessary since an instance of the object will be provided by the container (via the MeasurementStationServiceInstanceProvider).
        public MeasurementStationService(CloudStorageAccount storageAccount, string queueName, AzureServiceBusQueue azureServiceBusQueue)
        {
            if (storageAccount == null)
            {
                throw new ArgumentNullException("storageAccount");
            }

            if (azureServiceBusQueue == null)
            {
                throw new ArgumentNullException("AzureServiceBusQueue", "AzureServiceBusQueue instance can't be null");
            }

            this.logger = new AzureLogger();

            var queueRetryStrategy = new FixedInterval(3, TimeSpan.FromSeconds(1));
            var queueRetryPolicy = new RetryPolicy<StorageTransientErrorDetectionStrategy>(queueRetryStrategy);
            IMessageQueue azureInputQueue = new AzureMessageQueue(queueName, storageAccount, false);
            this.workerQueue = new RetryMessageQueue(azureInputQueue, queueRetryPolicy);

            IMessageQueue azureHealthReportQueue = azureServiceBusQueue;
            this.healthReportQueue = new RetryMessageQueue(azureHealthReportQueue, queueRetryPolicy);

            var tableRetryStrategy = new FixedInterval(3, TimeSpan.FromSeconds(1));
            var tableRetryPolicy = new RetryPolicy<StorageTransientErrorDetectionStrategy>(tableRetryStrategy);
            AzureTableDbContext azureTableDbContext = new AzureTableDbContext(storageAccount, tableRetryPolicy);

            this.measurementStationPublicTableContext = azureTableDbContext.PublicMeasurementStationsOperations;
            this.measurementStationPublicTableContext.GetTableReference(AzureTableHelper.MeasurementStationsPublicTable);

            SpectrumDataStorageAccountsTableOperations.Instance.Initialize(azureTableDbContext);
        }

        public int GetStationAvailability(string measurementStationAccessId)
        {
            if (string.IsNullOrWhiteSpace(measurementStationAccessId))
            {
                throw new ArgumentException("MeasurementStationAccessId can't be null or empty", "measurementStationAccessId");
            }

            MeasurementStationsPublic publicStation = this.measurementStationPublicTableContext.GetByKeys<MeasurementStationsPublic>(Constants.DummyPartitionKey, measurementStationAccessId).FirstOrDefault();

            if (publicStation == null)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.MeasurementStationService, string.Format("Invalid station access id :{0}, couldn't able to obtain information for this station", measurementStationAccessId));
                throw new ArgumentException("Invalid MeasurementStationAccessId");
            }

            return publicStation.StationAvailability;
        }

        public void GetUpdatedSettings(string measurementStationAccessId, out string storageAccountName, out string storageAccessKey, out byte[] measurementStationConfiguration)
        {
            MeasurementStationsPublic publicStation = this.measurementStationPublicTableContext.GetByKeys<MeasurementStationsPublic>(Constants.DummyPartitionKey, measurementStationAccessId).FirstOrDefault();

            if (publicStation != null)
            {
                CloudStorageAccount account = SpectrumDataStorageAccountsTableOperations.Instance.GetCloudStorageAccountByName(publicStation.SpectrumDataStorageAccountName);

                if (account != null)
                {
                    try
                    {
                        // create the blob container for this measurement station if it doesn't exist
                        CloudBlobClient blob = account.CreateCloudBlobClient();
                        CloudBlobContainer container = blob.GetContainerReference(measurementStationAccessId);

                        container.CreateIfNotExists(BlobContainerPublicAccessType.Container);

                        // create a write only shared access policy that lasts for one year
                        SharedAccessBlobPolicy policy = new SharedAccessBlobPolicy
                        {
                            Permissions = SharedAccessBlobPermissions.Write,
                            SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1),
                        };

                        BlobContainerPermissions blobPermissions = new BlobContainerPermissions();
                        blobPermissions.PublicAccess = BlobContainerPublicAccessType.Container;
                        blobPermissions.SharedAccessPolicies.Add("measurement station write policy", policy);

                        container.SetPermissions(blobPermissions);

                        // return both the account name and the SAS key so that the measurement station can then write to the blob storage
                        storageAccountName = container.Uri.AbsoluteUri;
                        storageAccessKey = container.GetSharedAccessSignature(null, "measurement station write policy");
                        measurementStationConfiguration = publicStation.ClientEndToEndConfiguration;

                        // Set the station to be online
                        if (publicStation.StationAvailability != (int)Microsoft.Spectrum.Storage.Enums.StationAvailability.Decommissioned)
                        {
                            publicStation.StationAvailability = (int)Microsoft.Spectrum.Storage.Enums.StationAvailability.Online;
                        }

                        this.measurementStationPublicTableContext.InsertOrReplaceEntity(publicStation, true);
                    }
                    catch (Exception ex)
                    {
                        this.logger.Log(TraceEventType.Error, LoggingMessageId.MeasurementStationService, string.Format(CultureInfo.InvariantCulture, "There was an error in GetUpdatedSettings for measurement station {0} in storage account {1}, Exception: {2}", measurementStationAccessId, account.Credentials.AccountName, ex.ToString()));
                        throw;
                    }
                }
                else
                {
                    this.logger.Log(TraceEventType.Error, LoggingMessageId.MeasurementStationService, string.Format(CultureInfo.InvariantCulture, "The storage account for this measurement station {0} can not be found.", measurementStationAccessId));
                    throw new InvalidOperationException("The storage account for this measurement station can not be found.");
                }
            }
            else
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.MeasurementStationService, string.Format(CultureInfo.InvariantCulture, "The measurement station {0} can not be found.", measurementStationAccessId));
                throw new ArgumentException("The measurement station id can not be found.", "measurementStationAccessId");
            }
        }

        public void ScanFileUploaded(string measurementStationAccessId, string blobUri, bool success)
        {
            try
            {
                MeasurementStationsPublic publicStation = this.measurementStationPublicTableContext.GetByKeys<MeasurementStationsPublic>(Constants.DummyPartitionKey, measurementStationAccessId).FirstOrDefault();

                if (publicStation == null)
                {
                    System.ServiceModel.OperationContext context = System.ServiceModel.OperationContext.Current;
                    MessageProperties oMessageProperties = context.IncomingMessageProperties;
                    RemoteEndpointMessageProperty clientMachine = (RemoteEndpointMessageProperty)oMessageProperties[RemoteEndpointMessageProperty.Name];

                    string error = string.Format("Invalid station id:{0} reported, blob uri:{1}, blob upload status :{2}, Message sent by Client:{3},Port:{4}", measurementStationAccessId, blobUri, success, clientMachine.Address, clientMachine.Port);
                    this.logger.Log(TraceEventType.Error, LoggingMessageId.MeasurementStationService, error);

                    throw new InvalidOperationException(string.Format("Invalid MeasurementStationAccessId :{0}", measurementStationAccessId));
                }

                // Do a little validation on the incoming parameters before even queueing them up, so that we catch these errors as early as possible
                Uri blob = new Uri(blobUri);
                Guid measurementStation = Guid.Parse(measurementStationAccessId);

                // Queue up this file for processing
                WorkerQueueMessage queueMessage = new WorkerQueueMessage(MessageType.FileProcessing, measurementStationAccessId, blobUri, success);

                JsonSerializerSettings jss = new JsonSerializerSettings();
                jss.TypeNameHandling = TypeNameHandling.All;
                this.workerQueue.PutMessage(JsonConvert.SerializeObject(queueMessage, jss));
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.MeasurementStationService, string.Format(CultureInfo.InvariantCulture, "There was an error in ScanFileUploaded,  Measurement station: {0}, Blob Uri: {1}, Success: {2}, Exception: {3}", measurementStationAccessId, blobUri, success, ex.ToString()));
                throw;
            }
        }

        public void ReportHealthStatus(string measurementStationAccessId, int healthStatus, int messagePriority, string title, string description, DateTime occurredAt)
        {
            try
            {
                HealthReportQueueMessage queueMessage = new HealthReportQueueMessage((StationHealthStatus)healthStatus, measurementStationAccessId, title, description, occurredAt);

                JsonSerializerSettings jss = new JsonSerializerSettings();
                jss.TypeNameHandling = TypeNameHandling.All;
                this.healthReportQueue.PutMessage(JsonConvert.SerializeObject(queueMessage, jss), messagePriority);
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.MeasurementStationService, string.Format(CultureInfo.InvariantCulture, "There was an error processing HealtReport message, Measurement station {0}, Title:{1}, Descripton :{2}, Exception:{3}", measurementStationAccessId, title, description, ex.ToString()));
                throw;
            }
        }

        public int GetHealthStatusCheckInterval(string meaurementStationAccessId)
        {
            try
            {
                MeasurementStationsPublic publicStation = this.measurementStationPublicTableContext.GetByKeys<MeasurementStationsPublic>(Constants.DummyPartitionKey, meaurementStationAccessId).FirstOrDefault();

                return publicStation != null ? publicStation.ClientHealthStatusCheckIntervalInMin : 0;
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.MeasurementStationService, string.Format(CultureInfo.InvariantCulture, "There was an while obtaining Health Check Interval, Measurement station {0},Exception:{2}", meaurementStationAccessId, ex.ToString()));
                throw;
            }
        }
    }
}
