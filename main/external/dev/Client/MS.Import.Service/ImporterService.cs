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

namespace Microsoft.Spectrum.Import.Service
{
    using System;
    using System.Diagnostics;
    using System.ServiceProcess;
    using Microsoft.Spectrum.Common;

    internal partial class ImporterService : ServiceBase
    {
        private readonly ImporterAgent importerAgent;
        private readonly ILogger logger;

        public ImporterService(ImporterAgent importerAgent, ILogger importerLogger)
        {
            if (importerAgent == null)
            {
                throw new ArgumentNullException("importerAgent");
            }

            if (importerLogger == null)
            {
                throw new ArgumentNullException("importerLogger");
            }

            this.importerAgent = importerAgent;
            this.logger = importerLogger;

            this.InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.logger.Log(TraceEventType.Start, LoggingMessageId.ImporterService, "Service Preparing To Start");

            try
            {
                this.importerAgent.StartMonitoring();
                this.logger.Log(TraceEventType.Start, LoggingMessageId.ImporterService, "Service Started");
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.ImporterService, ex.ToString());
                this.Stop();
            }
        }

        protected override void OnStop()
        {
            this.logger.Log(TraceEventType.Stop, LoggingMessageId.ImporterService, "Service Stopping");
            this.importerAgent.StopMonitoring();
            this.logger.Log(TraceEventType.Stop, LoggingMessageId.ImporterService, "Service Stopped");
        }
    }
}
