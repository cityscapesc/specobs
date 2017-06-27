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

namespace Microsoft.Spectrum.Storage.Blob
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    public class AzureSpectrumBlobStorage
    {
        private Lazy<CloudBlobContainer> blobContainer = null;

        public AzureSpectrumBlobStorage(CloudStorageAccount cloudStorageAccount, CloudBlobContainerName cloudBlobContainerName, string sas)
        {            
            if (string.IsNullOrWhiteSpace(sas))
            {
                if (cloudStorageAccount == null)
                {
                    throw new ArgumentNullException("cloudStorageAccount");
                }

                if (cloudBlobContainerName == null)
                {
                    throw new ArgumentNullException("cloudBlobContainerName");
                }

                // Calls to the Azure blob store should occur when the container is first used, not at object creation
                // Note: This constructor of Lazy<T> is thread-safe
                this.blobContainer = new Lazy<CloudBlobContainer>(() =>
                    {
                        CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
                        CloudBlobContainer blobContainer = blobClient.GetContainerReference(cloudBlobContainerName.Value);
                        blobContainer.CreateIfNotExists();
                        return blobContainer;
                    });
            }
            else
            {
                this.blobContainer = new Lazy<CloudBlobContainer>(() =>
                {
                    CloudBlobContainer blobContainer = new CloudBlobContainer(new Uri(sas + "&comp=list&restype=container"));
                    return blobContainer;
                });
            }
        }

        public Stream OpenRead(string absoluteUri)
        {
            CloudBlockBlob blob = this.blobContainer.Value.GetBlockBlobReference(absoluteUri);

            Stream downloadStream = blob.OpenRead();
            return downloadStream;
        }

        public Stream OpenWrite(string localPath, out string absoluteUri)
        {
            CloudBlockBlob blob = this.blobContainer.Value.GetBlockBlobReference(localPath);
            absoluteUri = blob.Uri.ToString();
            return blob.OpenWrite();
        }

        public string UploadFile(Stream file, string fileName, int retryCount, int serverTimeoutInMinutes, int retryDeltaBackOff)
        {
            BlobRequestOptions requestOptions = new BlobRequestOptions()
            {
                RetryPolicy = new Microsoft.WindowsAzure.Storage.RetryPolicies.LinearRetry(TimeSpan.FromMinutes(retryDeltaBackOff), retryCount),
                ServerTimeout = TimeSpan.FromMinutes(serverTimeoutInMinutes),
            };  

            CloudBlockBlob blob = this.blobContainer.Value.GetBlockBlobReference(fileName);

            blob.UploadFromStream(file, null, requestOptions, null);

            return blob.Uri.ToString();
        }

        public bool DeleteIfExists(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                return false;
            }

            CloudBlockBlob blob = this.blobContainer.Value.GetBlockBlobReference(blobName);

            return blob.DeleteIfExists();
        }
    }
}
