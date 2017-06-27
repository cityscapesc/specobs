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

namespace Microsoft.Spectrum.Scanning.Service
{
    using System;
    using System.Diagnostics;
    using System.ServiceProcess;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Scanning.Scanners;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "Instantiated by Unity")]
    internal partial class ScanningService : ServiceBase
    {
        private readonly IScanner scanningService;
        private readonly ILogger logger;

        public ScanningService(IScanner scanningService, ILogger logger)
        {
            if (scanningService == null)
            {
                throw new ArgumentNullException("scanningService");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.scanningService = scanningService;
            this.logger = logger;

            this.InitializeComponent();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Protect the thread")]
        protected override void OnStart(string[] args)
        {
            try
            {
                this.logger.Log(TraceEventType.Start, LoggingMessageId.ScanningRunAsService, "Service Starting");
                this.scanningService.StartScanning();
                this.logger.Log(TraceEventType.Start, LoggingMessageId.ScanningRunAsService, "Service Started");
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.ScanningRunAsService, ex.ToString());
                this.Stop();
            }
        }

        protected override void OnStop()
        {
            this.logger.Log(TraceEventType.Stop, LoggingMessageId.ScanningRunAsService, "Service Stopping");
            this.scanningService.StopScanning();
            this.logger.Log(TraceEventType.Stop, LoggingMessageId.ScanningRunAsService, "Service Stopped");
        }
    }
}
