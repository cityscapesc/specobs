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

namespace Microsoft.Spectrum.Analysis.DataProcessorRole
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Spectrum.Analysis.DataProcessor;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using WindowsAzure.Storage;
    public class WorkerRole : RoleEntryPoint
    {
        private CancellationTokenSource cancelWorkerRole;
        private DataProcessorAgent dataProcessorAgent;
        private AzureLogger logger = new AzureLogger();
        private bool returnedFromRunMethod = false;

        public override void Run()
        {
            this.logger.Log(TraceEventType.Information, LoggingMessageId.DataProcessorWorkerRoleLogEventId, "Entering the data processor worker role");

            this.cancelWorkerRole = new CancellationTokenSource();
            this.dataProcessorAgent = new DataProcessorAgent();

            int workerThreadsCount = ConnectionStringsUtility.WorkerThreadsCount;

            Task[] workerTasks = new Task[workerThreadsCount];

            for (int i = 0; i < workerThreadsCount; i++)
            {
                workerTasks[i] = Task.Factory.StartNew(
                    () =>
                    {
                        this.dataProcessorAgent.Run(this.cancelWorkerRole.Token, false);
                    });
            }

            try
            {
                Task.WaitAll(workerTasks, this.cancelWorkerRole.Token);
            }
            catch (Exception ex)
            {
                // Log the exception. Nothing else to be done.
                this.logger.Log(TraceEventType.Error, LoggingMessageId.DataProcessorWorkerRoleLogEventId, ex);
            }

            returnedFromRunMethod = true;
        }

        /// <returns>True if started successfully.</returns>
        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            //CloudStorageAccount.SetConfigurationSettingPublisher(
            //   (configName, configSettingPublisher) =>
            //   {
            //       var connectionString =
            //           RoleEnvironment.GetConfigurationSettingValue(configName);
            //       configSettingPublisher(connectionString);
            //   });

            //// Initialize azure diagnostics.
            //DiagnosticMonitorConfiguration diagnosticConfiguration = DiagnosticMonitor.GetDefaultInitialConfiguration();

            //// The log level filtering is imposed at the trace source, so set it to the max
            //// level here.
            //diagnosticConfiguration.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
            //diagnosticConfiguration.Logs.ScheduledTransferPeriod = TimeSpan.FromMinutes(1);

            //DiagnosticMonitor.Start(ConnectionStringsUtility.WadConnectionStringKey, diagnosticConfiguration);

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Cleans up all the objects used during the execution of the data processor.                        
            if (this.cancelWorkerRole != null)
            {
                this.cancelWorkerRole.Cancel();
            }

            while (this.returnedFromRunMethod == false)
            {
                Thread.Sleep(1000);
            }

            this.dataProcessorAgent = null;

            base.OnStop();
        }
    }
}
