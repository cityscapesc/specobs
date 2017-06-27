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
    using System.Threading;
    using Microsoft.Spectrum.Common;

    internal class RunAsExeCommand : ICommand
    {
        private readonly IConfigurationSource configurationSource;
        private readonly ImporterAgent importerAgent;
        private readonly ILogger logger;

        public RunAsExeCommand(IConfigurationSource configurationSource, ImporterAgent importerAgent, ILogger logger)
        {
            if (configurationSource == null)
            {
                throw new ArgumentNullException("configurationSource");
            }

            if (importerAgent == null)
            {
                throw new ArgumentNullException("importerAgent");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.configurationSource = configurationSource;
            this.importerAgent = importerAgent;
            this.logger = logger;
        }
    
        public void Execute()
        {
            Console.WriteLine("Press Enter to terminate ...");

            try
            {
                this.importerAgent.StartMonitoring();
                this.logger.Log(TraceEventType.Information, LoggingMessageId.ImporterRunAsExeCommand, "Monitoring started...");
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.ImporterRunAsExeCommand, ex.ToString());
                this.importerAgent.StopMonitoring();
            }
           
            Console.ReadLine();
        }
    }
}