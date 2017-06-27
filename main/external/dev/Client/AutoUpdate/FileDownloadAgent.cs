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

namespace Microsoft.Spectrum.AutoUpdate
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Threading;
    using Microsoft.Spectrum.Common;

    public class FileDownloadAgent : IDownloadAgent, IDisposable
    {
        private static int lastProgressPercentage = 0;

        private readonly ILogger logger;

        private readonly WebClient webClient;

        public FileDownloadAgent(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;
            this.webClient = new WebClient();
            this.webClient.Proxy = null;
        }

        public void DownloadFile(Uri address, string fileName)
        {
            FileDownloadAgent.lastProgressPercentage = 0;
            this.logger.Log(TraceEventType.Information, LoggingMessageId.FileDownloadAgent, "Downloading the updates");

            this.webClient.DownloadProgressChanged += this.DownloadProgressChanged;
            this.webClient.DownloadFileCompleted += this.DownloadFileCompleted;

            this.webClient.DownloadFileTaskAsync(address, fileName).Wait();
        }

        public Stream OpenRead(Uri address)
        {
            Stream stream = null;

            try
            {
                stream = this.webClient.OpenRead(address);
            }
            catch (WebException ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.FileDownloadAgent, ex.ToString());
            }

            return stream;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.webClient.Dispose();
            }
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // To avoid same progress values being displayed multiple times.
            if (e.ProgressPercentage > FileDownloadAgent.lastProgressPercentage)
            {
                if (e.ProgressPercentage % 20 == 0)
                {
                    this.logger.Log(TraceEventType.Information, LoggingMessageId.DownloadingUpdates, string.Format(CultureInfo.InvariantCulture, "Downloaded {0} of {1} bytes. {2} % completed", e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage));
                    FileDownloadAgent.lastProgressPercentage = e.ProgressPercentage;
                }
            }
        }

        private void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.logger.Log(TraceEventType.Information, LoggingMessageId.DownloadedUpdates, "Download completed successfully.");

            this.webClient.DownloadProgressChanged -= this.DownloadProgressChanged;
            this.webClient.DownloadFileCompleted -= this.DownloadFileCompleted;
        }
    }
}
