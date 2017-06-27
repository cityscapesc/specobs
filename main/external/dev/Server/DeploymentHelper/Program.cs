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

namespace Microsoft.Spectrum.DeploymentHelper
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Xml.Linq;
    using CsvHelper;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.Spectrum.Storage.Blob;
    using Microsoft.Spectrum.Storage.Queue.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Microsoft.Azure;
    using Microsoft.WindowsAzure.Management;
    using Microsoft.WindowsAzure.Management.Models;
    using Microsoft.WindowsAzure.Management.Scheduler;
    using Microsoft.WindowsAzure.Management.Scheduler.Models;
    using Microsoft.WindowsAzure.Management.Storage;
    using Microsoft.WindowsAzure.Management.Storage.Models;
    using Microsoft.WindowsAzure.Scheduler;
    using Microsoft.WindowsAzure.Scheduler.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Queue.Protocol;
    using Newtonsoft.Json;

    public enum PerformDeploymentOperation
    {
        All,
        AffinityGroupCreate,
        StorageCreate,
        CloudServiceCreate,
        QuestionsUpdate,
        SettingsUpdate,
        SchedulerUpdate
    }

    public class Program
    {
        // the maximum number of storage accounts in a single subscription is 50
        private const int NumberStorageAccounts = 10;
        private const string MasterStorageAccountName = "somaster";
        private const string AffinityGroupName = "SOAffinityGroup";
        //private const string CloudServiceName = "SOCloudService";
        private const string CloudServiceName = "cityscape";
        private const string SchedulerCloudServiceName = "SchedulerCloudService";
        private const string SchedulerJobCollectionName = "soSchedulerCollection";
        private const string Version = "2013-08-01";

        private const string Installer = "Installer";
        private const string InstallerContainerName = "msidownload";
        private const string InstallerFileName = "ImporterSetup.msi";
        private const string DeviceSetupGuide = "SetupManual";

        private static Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryStrategy retryStrategy = new Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.FixedInterval(3, TimeSpan.FromSeconds(1));
        private static Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryPolicy<StorageTransientErrorDetectionStrategy> retryPolicy = new Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryPolicy<StorageTransientErrorDetectionStrategy>(retryStrategy);
        private static CloudStorageAccount cloudStorageAccount = null;
        private static AzureTableDbContext azureDbTableContext = null;
        private static string startDateTime;

        public static void Main(string[] args)
        {
            retryStrategy = new Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.FixedInterval(3, TimeSpan.FromSeconds(1));
            retryPolicy = new Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling.RetryPolicy<StorageTransientErrorDetectionStrategy>(retryStrategy);
            startDateTime = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
            startDateTime = startDateTime.Replace(":", null);
            startDateTime = startDateTime.Replace("-", null);
            startDateTime = startDateTime.Remove(startDateTime.Length - 2).ToLower(); // Remove seconds

            PerformDeploymentOperation operationsToPerform = PerformDeploymentOperation.All;

            if (args.Length != 0)
            {
                operationsToPerform = (PerformDeploymentOperation)Enum.Parse(typeof(PerformDeploymentOperation), args[0]);

                if (operationsToPerform == PerformDeploymentOperation.All || operationsToPerform == PerformDeploymentOperation.AffinityGroupCreate)
                {
                    CreateAffinityGroup();
                }

                if (operationsToPerform == PerformDeploymentOperation.All || operationsToPerform == PerformDeploymentOperation.CloudServiceCreate)
                {
                    CreateCloudService();
                }

                if (operationsToPerform == PerformDeploymentOperation.All || operationsToPerform == PerformDeploymentOperation.StorageCreate)
                {
                    CreateStorageAccounts();
                }

                // This means that none of the operations above were performed
                if (cloudStorageAccount == null || azureDbTableContext == null)
                {
                    cloudStorageAccount = CloudStorageAccount.Parse(ConnectionStringsUtility.StorageAccountConnectionString);
                    azureDbTableContext = new AzureTableDbContext(cloudStorageAccount, retryPolicy);
                    SpectrumDataStorageAccountsTableOperations.Instance.Initialize(azureDbTableContext);
                }

                string deploymentGuideUri;
                string installerUri = SetupInstallerAndDocumentation(out deploymentGuideUri);

                if (operationsToPerform == PerformDeploymentOperation.All || operationsToPerform == PerformDeploymentOperation.QuestionsUpdate)
                {
                    string filename = Directory.GetCurrentDirectory() + "\\QuestionsUpdate.csv";
                    UpdateQuestions(cloudStorageAccount, azureDbTableContext, filename);
                }

                if (operationsToPerform == PerformDeploymentOperation.All || operationsToPerform == PerformDeploymentOperation.SettingsUpdate)
                {
                    string filename = Directory.GetCurrentDirectory() + "\\SettingsUpdate.csv";
                    UpdateSettings(azureDbTableContext, filename, installerUri, deploymentGuideUri);
                }

                if (operationsToPerform == PerformDeploymentOperation.All)
                {
                    // This isn't set up to happen independently since it needs to know the created cloud service name
                    UpdateScheduler();
                }
            }
            else
            {
                Console.WriteLine("Invalid Arguments");
                Console.WriteLine("Usage - DeploymentHelper.exe [Operation]");
                Console.WriteLine("Operation Choices: All, AffinityGroupCreate, CloudServiceCreate, StorageCreate, QuestionsUpdate, SettingsUpdate");
            }

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        public static string SetupInstallerAndDocumentation(out string deploymentGuideUri)
        {
            Console.WriteLine("Creating the blob continer for the installer and uploading the latest version from the devbins directory.");

            // Create the blob container
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(InstallerContainerName);

            blobContainer.CreateIfNotExists(BlobContainerPublicAccessType.Container);

            //string installerUri = string.Format("{0}/{1}", blobContainer.Uri.ToString(), InstallerFileName);
            string installerUri = string.Empty;

            CloudBlobContainerName containerName = new CloudBlobContainerName(InstallerContainerName);

            // Upload the latest MSI
            const string ImporterSetupMsi = "ImporterSetup.msi";
            const string DevBinsDirectory = "\\..\\..\\..\\..\\..\\..\\devbins\\";
            AzureSpectrumBlobStorage blobStorage = new AzureSpectrumBlobStorage(cloudStorageAccount, containerName, string.Empty);
            using (FileStream importerMsi = File.Open(Environment.CurrentDirectory + DevBinsDirectory + ImporterSetupMsi, FileMode.Open))
            {
                installerUri = blobStorage.UploadFile(importerMsi, ImporterSetupMsi, 2, 10, 0);
                Console.WriteLine("MSI File Uploaded to " + installerUri);
            }

            const string DeploymentGuide = "Spectrum Observatory Station Deployment Guide.pdf";
            const string DocsDirectory = "\\..\\..\\..\\..\\..\\docs\\";
            using (FileStream deploymentGuide = File.Open(Environment.CurrentDirectory + DocsDirectory + DeploymentGuide, FileMode.Open))
            {
                deploymentGuideUri = blobStorage.UploadFile(deploymentGuide, DeploymentGuide, 2, 10, 0);
                Console.WriteLine("Deployment Guide Uploaded to " + deploymentGuideUri);
            }

            return installerUri;
        }

        public static void UpdateSettings(AzureTableDbContext azureDbTableContext, string filename, string installerUri, string deploymentGuideUri)
        {
            Console.WriteLine("Updating Settings...");
            IEnumerable<SettingsRecord> records = ReadAllSettingsFromCsv(filename);

            RetryAzureTableOperations<Settings> settingsContext = azureDbTableContext.SettingsOperations;
            settingsContext.GetTableReference(AzureTableHelper.SettingsTable);
            SettingsTableHelper.Instance.Initialize(settingsContext, new FileLogger(Directory.GetCurrentDirectory(), "deployment"));

            // We can set whatever settings we would like here
            foreach (SettingsRecord record in records)
            {
                if (record.SettingType == "int")
                {
                    SettingsTableHelper.Instance.SetSetting(record.Category, record.Name, Convert.ToInt32(record.Value));
                }
                else if (record.SettingType == "TimeSpan")
                {
                    SettingsTableHelper.Instance.SetSetting(record.Category, record.Name, TimeSpan.FromDays(Convert.ToInt32(record.Value)));
                }
                else if (record.SettingType == "string")
                {
                    SettingsTableHelper.Instance.SetSetting(record.Category, record.Name, record.Value);
                }
                else if (record.SettingType == "bool")
                {
                    SettingsTableHelper.Instance.SetSetting(record.Category, record.Name, Convert.ToBoolean(record.Value));
                }

                Console.WriteLine(string.Format("Updated setting Category: {0}, Name: {1}, Value: {2}", record.Category, record.Name, record.Value));
            }

            SettingsTableHelper.Instance.SetSetting(SettingsTableHelper.DeviceSetupCategory, Installer, installerUri);
            Console.WriteLine(string.Format("Updated setting Category: {0}, Name: {1}, Value: {2}", SettingsTableHelper.DeviceSetupCategory, Installer, installerUri));

            SettingsTableHelper.Instance.SetSetting(SettingsTableHelper.DeviceSetupCategory, DeviceSetupGuide, deploymentGuideUri);
            Console.WriteLine(string.Format("Updated setting Category: {0}, Name: {1}, Value: {2}", SettingsTableHelper.DeviceSetupCategory, DeviceSetupGuide, deploymentGuideUri));
        }

        public static void UpdateQuestions(CloudStorageAccount cloudStorageAccount, AzureTableDbContext azureDbTableContext, string filename)
        {
            Console.WriteLine("Updating Questions...");
            IEnumerable<QuestionsRecord> records = ReadAllQuestionsFromCsv(filename);

            // delete everything already in this table
            try
            {
                cloudStorageAccount.CreateCloudTableClient().GetTableReference(AzureTableHelper.QuestionsTable).DeleteIfExists();
            }
            catch (StorageException ex)
            {
                Console.WriteLine(ex.Message);
            }

            // now recreate the table with the new entries
            RetryAzureTableOperations<Questions> questionsContext = azureDbTableContext.QuestionsTableOperations;
            questionsContext.GetTableReference(AzureTableHelper.QuestionsTable);
            questionsContext.SafeCreateTableIfNotExists();

            foreach (QuestionsRecord record in records)
            {
                Questions entity = new Questions(record.Section, record.Ask, record.Answer, record.Order);

                questionsContext.InsertEntity(entity);

                Console.WriteLine(string.Format("Inserting question Section: {0}, Ask: {1}, Answer: {2}", record.Section, record.Ask, record.Answer));
            }
        }

        public static void UpdateScheduler()
        {
            string jobCollectionName = SchedulerJobCollectionName + startDateTime;
            const string SasQueueCollectionPolicy = "schedulercollectionpolicy";

            Console.WriteLine("Updating the scheduler...");

            // delete the existing collection in the scheduler
            var schedulerServiceClient = new SchedulerManagementClient(GetCredentials());

            try
            {
                var resultRegister = schedulerServiceClient.RegisterResourceProvider();

                Console.WriteLine(resultRegister.RequestId);
                Console.WriteLine(resultRegister.StatusCode);
            }
            catch (Exception ex)
            {
                // It is expected that this may fail because the resource may already be registered with our subscription
                Console.WriteLine(ex.ToString());
            }

            var resultResource = schedulerServiceClient.GetResourceProviderProperties();

            foreach (var prop in resultResource.Properties)
            {
                Console.WriteLine(prop.Key + ": " + prop.Value);
            }

            // This will work for us until we need to schedule more than 5 things, then we will have to go to the paid version
            JobCollectionIntrinsicSettings collectionSettings = new JobCollectionIntrinsicSettings()
            {
                Plan = JobCollectionPlan.Free,
                Quota = new JobCollectionQuota()
                {
                    MaxJobCount = 5,
                    MaxJobOccurrence = 1,
                    MaxRecurrence = new JobCollectionMaxRecurrence()
                    {
                        Frequency = JobCollectionRecurrenceFrequency.Hour,
                        Interval = 1
                    }
                }
            };

            JobCollectionCreateParameters jobCollectionCreate = new JobCollectionCreateParameters()
            {
                IntrinsicSettings = collectionSettings,
                Label = jobCollectionName
            };

            // Creating a cloud service for the scheduler
            CloudServiceManagementClient cloudServiceClient = new CloudServiceManagementClient(GetCredentials());
            var result = cloudServiceClient.CloudServices.Create(
                SchedulerCloudServiceName + startDateTime,
                new CloudServiceCreateParameters()
                {
                    Description = "Microsoft Spectrum Observatory Scheduler Service " + startDateTime,
                    GeoRegion = "Central US",
                    Label = "Microsoft Spectrum Observatory Scheduler Service " + startDateTime
                });

            Console.WriteLine(string.Format("Creating Job Collection - Name: {0}, Account {1}", jobCollectionName, cloudStorageAccount.Credentials.AccountName));
            SchedulerOperationStatusResponse response = schedulerServiceClient.JobCollections.Create(SchedulerCloudServiceName + startDateTime, jobCollectionName, jobCollectionCreate);
            Console.WriteLine(string.Format("Create HTTP Status Code: {0}", response.StatusCode));

            // Create or get a reference to the worker queue
            CloudQueueClient queueClient = cloudStorageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(ConnectionStringsUtility.WorkerQueueName);
            queue.CreateIfNotExists();

            // Create a shared access policy for the queue
            QueuePermissions perm = new QueuePermissions();
            var policy = new SharedAccessQueuePolicy { SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1000), Permissions = SharedAccessQueuePermissions.Add };
            perm.SharedAccessPolicies.Add(SasQueueCollectionPolicy, policy);

            queue.SetPermissions(perm);
            var sas = queue.GetSharedAccessSignature(new SharedAccessQueuePolicy(), SasQueueCollectionPolicy);
            Console.WriteLine(string.Format("Created Shared Access Policy - Queue {0}, SAS Key {1}", SasQueueCollectionPolicy, sas));

            SchedulerClient schedulerClient = new SchedulerClient(SchedulerCloudServiceName + startDateTime, jobCollectionName, GetCredentials());

            // Add a scheduled event starting midnight UTC time to do the Retention Policy forever
            WorkerQueueMessage queueRetentionPolicyuMessage = new WorkerQueueMessage(MessageType.RetentionPolicy, string.Empty, string.Empty, true);
            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.TypeNameHandling = TypeNameHandling.All;

            Console.WriteLine("Creating a schedule for the retention policy...");
            JobCreateResponse jobresponse = schedulerClient.Jobs.Create(new JobCreateParameters()
            {
                Action = new JobAction()
                {
                    Type = JobActionType.StorageQueue,
                    QueueMessage = new JobQueueMessage()
                    {
                        Message = JsonConvert.SerializeObject(queueRetentionPolicyuMessage, jss),
                        QueueName = ConnectionStringsUtility.WorkerQueueName,
                        SasToken = sas,
                        StorageAccountName = cloudStorageAccount.Credentials.AccountName
                    }
                },
                StartTime = DateTime.UtcNow.Date.AddDays(1),
                Recurrence = new JobRecurrence()
                {
                    Frequency = JobRecurrenceFrequency.Hour,
                    Interval = 24
                }
            });
            Console.WriteLine(string.Format("Create HTTP Status Code: {0}", jobresponse.StatusCode));

            // Add a scheduled event starting midnight UTC time to do the Health Monitoring forever
            WorkerQueueMessage queueHealthMonitoringMessage = new WorkerQueueMessage(MessageType.SpectrumObservatoriesHealthMonitoring, string.Empty, string.Empty, true);

            Console.WriteLine("Creating a schedule for the health monitoring...");
            jobresponse = schedulerClient.Jobs.Create(new JobCreateParameters()
            {
                Action = new JobAction()
                {
                    Type = JobActionType.StorageQueue,
                    QueueMessage = new JobQueueMessage()
                    {
                        Message = JsonConvert.SerializeObject(queueHealthMonitoringMessage, jss),
                        QueueName = ConnectionStringsUtility.WorkerQueueName,
                        SasToken = sas,
                        StorageAccountName = cloudStorageAccount.Credentials.AccountName
                    }
                },
                StartTime = DateTime.UtcNow.Date.AddDays(1),
                Recurrence = new JobRecurrence()
                {
                    Frequency = JobRecurrenceFrequency.Hour,
                    Interval = 24
                }
            });
            Console.WriteLine(string.Format("Create HTTP Status Code: {0}", jobresponse.StatusCode));

            // Add a scheduled event starting midnight UTC time to do the Station Availablilty every two hours
            WorkerQueueMessage queueStationAvailabilityMessage = new WorkerQueueMessage(MessageType.SpectrumObservatoriesAvailability, string.Empty, string.Empty, true);

            Console.WriteLine("Creating a schedule for the station availablilty...");
            jobresponse = schedulerClient.Jobs.Create(new JobCreateParameters()
            {
                Action = new JobAction()
                {
                    Type = JobActionType.StorageQueue,
                    QueueMessage = new JobQueueMessage()
                    {
                        Message = JsonConvert.SerializeObject(queueStationAvailabilityMessage, jss),
                        QueueName = ConnectionStringsUtility.WorkerQueueName,
                        SasToken = sas,
                        StorageAccountName = cloudStorageAccount.Credentials.AccountName
                    }
                },
                StartTime = DateTime.UtcNow,
                Recurrence = new JobRecurrence()
                {
                    Frequency = JobRecurrenceFrequency.Hour,
                    Interval = 2
                }
            });
            Console.WriteLine(string.Format("Create HTTP Status Code: {0}", jobresponse.StatusCode));

            // Add a scheduled event starting now to do the Table Shared Access every two hours
            WorkerQueueMessage queueTableSharedAccessMessage = new WorkerQueueMessage(MessageType.TablesSharedAccessSignatureUpdate, string.Empty, string.Empty, true);

            Console.WriteLine("Creating a schedule for the table shared access...");
            jobresponse = schedulerClient.Jobs.Create(new JobCreateParameters()
            {
                Action = new JobAction()
                {
                    Type = JobActionType.StorageQueue,
                    QueueMessage = new JobQueueMessage()
                    {
                        Message = JsonConvert.SerializeObject(queueTableSharedAccessMessage, jss),
                        QueueName = ConnectionStringsUtility.WorkerQueueName,
                        SasToken = sas,
                        StorageAccountName = cloudStorageAccount.Credentials.AccountName
                    }
                },
                StartTime = DateTime.UtcNow,
                Recurrence = new JobRecurrence()
                {
                    Frequency = JobRecurrenceFrequency.Hour,
                    Interval = 2
                }
            });
            Console.WriteLine(string.Format("Create HTTP Status Code: {0}", jobresponse.StatusCode));
        }

        public static void CreateCloudService()
        {
            SubscriptionConfigurationSection subscriptionConfiguration = (SubscriptionConfigurationSection)ConfigurationManager.GetSection("Subscription");
            XNamespace wa = "http://schemas.microsoft.com/windowsazure";

            // Create the URI for the request
            string uriFormat = "https://management.core.windows.net/{0}/services/hostedservices";
            Uri uri = new Uri(string.Format(uriFormat, subscriptionConfiguration.Id));

            // Base-64 encode the label of the cloud service
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes("Spectrum Observatory Data Processor Worker Role");
            string base64Label = Convert.ToBase64String(bytes);

            // Create the request body
            XDocument requestBody = new XDocument(
                new XDeclaration("1.0", "UTF-8", "no"),
                new XElement(
                    wa + "CreateHostedService",
                    //new XElement(wa + "ServiceName", CloudServiceName + startDateTime),
                    new XElement(wa + "ServiceName", CloudServiceName),
                    new XElement(wa + "Label", base64Label),
                    new XElement(wa + "AffinityGroup", AffinityGroupName + startDateTime)));

            // Submit the request and get the response
            Console.WriteLine(string.Format("Creating Affinity Group - Name: {0}, AffinityGroup: {1}", CloudServiceName + startDateTime, AffinityGroupName + startDateTime));
            HttpWebResponse response = InvokeRequest(uri, "POST", requestBody, subscriptionConfiguration.ManagementCertificate);
            HttpStatusCode statusCode = response.StatusCode;
            Console.WriteLine(string.Format("Create HTTP Status Code: {0}", response.StatusCode));
        }

        public static HttpWebResponse InvokeRequest(Uri uri, string method, XDocument requestBody, string managementCertificate)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = method;
            request.Headers.Add("x-ms-version", Version);
            request.ClientCertificates.Add(new X509Certificate2(@"E:\CityScape\cityscape.cer"));//Convert.FromBase64String(managementCertificate)));
            request.ContentType = "application/xml";

            HttpWebResponse response;

            try
            {
                byte[] byteArray = null;
                byteArray = Encoding.UTF8.GetBytes(requestBody.ToString());
                Stream stream = request.GetRequestStream();
                stream.Write(byteArray, 0, byteArray.Length);
                stream.Flush();
                stream.Close();

                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
            }

            response.Close();
            return response;
        }

        public static void CreateAffinityGroup()
        {
            // We want to make sure that everything is deployed using this affinity group, we will also have to do this for the cloud compute
            AffinityGroupCreateParameters affinity = new AffinityGroupCreateParameters();
            affinity.Location = "Central US";
            affinity.Name = AffinityGroupName + startDateTime;
            affinity.Label = "Spectrum Observatory Affinity Group";
            affinity.Description = "This is the affinity group for all Spectrum Observatory elements in Azure.";

            ManagementClient management = new ManagementClient(GetCredentials());

            Console.WriteLine(string.Format("Creating Affinity Group - Name: {0}, Location: {1}", affinity.Name, affinity.Location));
            AzureOperationResponse response = management.AffinityGroups.Create(affinity);
            Console.WriteLine(string.Format("Create HTTP Status Code: {0}", response.StatusCode));
        }

        public static void CreateStorageAccounts()
        {
            Console.WriteLine("Creating Storage Accounts...");
            StorageManagementClient storageClient = new StorageManagementClient(GetCredentials());

            // First go in and create all of the storage accounts for this deployment
            for (int i = 0; i < NumberStorageAccounts; i++)
            {
                CreateStorageAccount(storageClient, i);
            }

            // initialize the master storage account
            foreach (var account in storageClient.StorageAccounts.List())
            {
                if (account.Name == MasterStorageAccountName + startDateTime)
                {
                    StorageAccountGetKeysResponse keyResponse = storageClient.StorageAccounts.GetKeys(account.Name);

                    cloudStorageAccount = new CloudStorageAccount(new WindowsAzure.Storage.Auth.StorageCredentials(account.Name, keyResponse.PrimaryKey), true);
                    azureDbTableContext = new AzureTableDbContext(cloudStorageAccount, retryPolicy);
                    SpectrumDataStorageAccountsTableOperations.Instance.Initialize(azureDbTableContext);

                    break;
                }
            }

            // Next add all of the created storage accounts to the storage accounts table                        
            foreach (var account in storageClient.StorageAccounts.List())
            {
                StorageAccountGetKeysResponse keyResponse = storageClient.StorageAccounts.GetKeys(account.Name);

                if (!account.Name.EndsWith(startDateTime))
                {
                    continue;
                }

                // now insert it into the table
                SpectrumDataStorageAccounts entity = new SpectrumDataStorageAccounts(account.Name);
                entity.AccountKey = keyResponse.PrimaryKey;
                if (account.Name == MasterStorageAccountName + startDateTime)
                {
                    // set up the master storage account so that no stations are assigned to it to start. If we decide to adjust this 
                    // in the future, then we can do it by lowering this number down, but it may lower our web site's responsiveness
                    entity.StationCount = 0;
                    entity.MaxStationCount = 0;
                }
                else
                {
                    entity.StationCount = 0;
                    entity.MaxStationCount = 50;
                }

                entity.Uri = account.Uri.ToString();

                SpectrumDataStorageAccountsTableOperations.Instance.InsertEntity(entity);

                Console.WriteLine(string.Format("Inserted account {0} with key {1} into SpectrumDataStorageAccounts table.", entity.Name, entity.AccountKey));
            }
        }

        public static void CreateStorageAccount(StorageManagementClient storageClient, int spectrumAccountNum)
        {
            StorageAccountCreateParameters createParameters = new StorageAccountCreateParameters();
            string label;
            string name;

            createParameters.AccountType = StorageAccountTypes.StandardLRS;

            // need to use the list locations call here
            createParameters.AffinityGroup = AffinityGroupName + startDateTime;

            if (spectrumAccountNum == 0)
            {
                label = "Spectrum Observatory Master Metadata Storage Account";
                name = MasterStorageAccountName + startDateTime;
            }
            else
            {
                label = string.Format("Spectrum Observatory Spectrum Data Storage Account {0}", spectrumAccountNum);
                name = string.Format("so{0}stor{1}", spectrumAccountNum, startDateTime);
            }

            createParameters.Name = name;
            createParameters.Label = label;

            Console.WriteLine(string.Format("Creating Storage Account - Name: {0}, Location: {1}, Account Type: {2}", createParameters.Name, createParameters.Location, createParameters.AccountType));
            Azure.OperationStatusResponse response = storageClient.StorageAccounts.Create(createParameters);
            Console.WriteLine(string.Format("Create HTTP Status Code: {0}", response.StatusCode));
        }

        public static string EncodeToBase64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public static Microsoft.Azure.SubscriptionCloudCredentials GetCredentials()
        {
            SubscriptionConfigurationSection subscriptionConfiguration = (SubscriptionConfigurationSection)ConfigurationManager.GetSection("Subscription");

            return new CertificateCloudCredentials(subscriptionConfiguration.Id, new X509Certificate2(@"E:\CityScape\cityscape.cer"));//Convert.FromBase64String(subscriptionConfiguration.ManagementCertificate)));
        }

        private static IEnumerable<SettingsRecord> ReadAllSettingsFromCsv(string filename)
        {
            TextReader fileReader = File.OpenText(filename);
            CsvReader reader = new CsvReader(fileReader);

            return reader.GetRecords<SettingsRecord>();
        }

        private static IEnumerable<QuestionsRecord> ReadAllQuestionsFromCsv(string filename)
        {
            TextReader fileReader = File.OpenText(filename);
            CsvReader reader = new CsvReader(fileReader);

            return reader.GetRecords<QuestionsRecord>();
        }

        private class QuestionsRecord
        {
            public string Section { get; set; }

            public string Ask { get; set; }

            public string Answer { get; set; }

            public int Order { get; set; }
        }

        private class SettingsRecord
        {
            public string SettingType { get; set; }

            public string Category { get; set; }

            public string Name { get; set; }

            public string Value { get; set; }
        }
    }
}
