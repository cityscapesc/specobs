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

namespace Microsoft.Spectrum.AutoUpdate.Service
{
    using System;
    using System.Diagnostics;
    using System.ServiceProcess;
    using Microsoft.Spectrum.Common;   

    public partial class AutoUpdateService : ServiceBase
    {
        private readonly ILogger logger;
        private readonly IUpdateAgent updateAgent;

        public AutoUpdateService(ILogger logger, IUpdateAgent updateAgent)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (updateAgent == null)
            {
                throw new ArgumentNullException("updateAgent");
            }

            this.logger = logger;
            this.updateAgent = updateAgent;

            this.InitializeComponent();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "To help debug service start up")]
        protected override void OnStart(string[] args)
        {
            try
            {
                this.logger.Log(TraceEventType.Start, LoggingMessageId.AutoUpdateRunAsService, "Service Starting");
                this.updateAgent.TurnAutomaticUpdatesOn();
                this.logger.Log(TraceEventType.Start, LoggingMessageId.AutoUpdateRunAsService, "Service Started");
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.AutoUpdateRunAsService, ex.ToString());
            }
        }

        protected override void OnStop()
        {
            this.logger.Log(TraceEventType.Stop, LoggingMessageId.AutoUpdateRunAsService, "Service Stopping");
            this.updateAgent.TurnAutomaticUpdatesOff();
            this.logger.Log(TraceEventType.Stop, LoggingMessageId.AutoUpdateRunAsService, "Service Stopped");
        }
    }
}
