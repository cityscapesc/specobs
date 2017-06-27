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
    using System;
    using System.Diagnostics;
    using System.Threading;   
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;     

    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            //// Initialize azure diagnostics.
            ////DiagnosticMonitorConfiguration diagnosticConfiguration = DiagnosticMonitor.GetDefaultInitialConfiguration();

            //// The log level filtering is imposed at the trace source, so set it to the max
            //// level here.
            ////diagnosticConfiguration.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
            ////diagnosticConfiguration.Logs.ScheduledTransferPeriod = TimeSpan.FromMinutes(1);

            ////DiagnosticMonitor.Start(ConnectionStringsUtility.WadConnectionStringKey, diagnosticConfiguration);

            //// For information on handling configuration changes
            //// see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            return base.OnStart();
        }

        public override void OnStop()
        {
            Trace.TraceInformation("OnStop called for MS.MeasurementStation.Service");

            //// Propertly handle the web role restart
            var pcrc = new PerformanceCounter("ASP.NET", "Requests Current", "");

            while (true)
            {
                var rc = pcrc.NextValue();
                Trace.TraceInformation("ASP.NET Requests Current =" + rc.ToString());
                if (rc <= 0)
                {
                    break;
                }

                Thread.Sleep(1000);
            }

            base.OnStop();
        }
    }
}
