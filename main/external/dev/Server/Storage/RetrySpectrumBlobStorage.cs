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

namespace Microsoft.Spectrum.Storage
{
    using System;
    using System.IO;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Microsoft.Spectrum.Storage.Blob;

    public class RetrySpectrumBlobStorage
    {
        private readonly AzureSpectrumBlobStorage blobStorage;
        private readonly RetryPolicy retryPolicy;

        public RetrySpectrumBlobStorage(AzureSpectrumBlobStorage blobStorage, RetryPolicy retryPolicy)
        {
            if (blobStorage == null)
            {
                throw new ArgumentNullException("blobStorage");
            }

            if (retryPolicy == null)
            {
                throw new ArgumentNullException("retryPolicy");
            }

            this.blobStorage = blobStorage;
            this.retryPolicy = retryPolicy;
        }

        public Stream OpenWrite(string localPath, out string absoluteUri)
        {
            Tuple<Stream, string> results = this.retryPolicy.ExecuteAction(() =>
                {
                    string absoluteUriOutput;
                    Stream stream = blobStorage.OpenWrite(localPath, out absoluteUriOutput);
                    return Tuple.Create(stream, absoluteUriOutput);
                });
            absoluteUri = results.Item2;
            return results.Item1;
        }

        public Stream OpenRead(string absoluteUri)
        {
            return this.retryPolicy.ExecuteAction(() =>
                this.blobStorage.OpenRead(absoluteUri));
        }

        public bool DeleteIfExists(string blobName)
        {
            return this.retryPolicy.ExecuteAction(() =>
                this.blobStorage.DeleteIfExists(blobName));
        }
    }
}