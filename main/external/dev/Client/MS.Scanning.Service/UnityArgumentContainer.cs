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
    using System.Configuration;
    using System.ServiceProcess;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.Configuration;
    using Microsoft.Spectrum.Common;    

    public class UnityArgumentContainer : IArgumentContainer
    {
        public IExecute ResolveArguments(string[] args)
        {
            using (IUnityContainer container = new UnityContainer())
            {
                // Register the correct scanner from the config file
                container.LoadConfiguration();

                if (args == null)
                {
                    container.RegisterType<IExecute, DisplayHelpCommand>();
                }
                else
                {
                    EventLogLogger eventlogLogger = new EventLogLogger("SpectrumScanningService");

                    LoggingConfigurationSection loggingDetails = (LoggingConfigurationSection)ConfigurationManager.GetSection("LoggingConfiguration");
                    FileLogger fileLogger = new FileLogger(Environment.ExpandEnvironmentVariables(loggingDetails.LoggingDirectory), "ScanningService");

                    // Then register the rest of the types
                    if (args.Length == 0)
                    {
                        container.RegisterType<IExecute, RunAsServiceCommand>();
                        container.RegisterType<ServiceBase, ScanningService>();
                        container.RegisterInstance<ILogger>(new CompositeLogger(fileLogger, eventlogLogger));
                    }
                    else if (args.Length == 1)
                    {
                        if (string.Compare(args[0], "RunAsExe", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            container.RegisterType<IExecute, RunAsExeCommand>();
                            container.RegisterInstance<ILogger>(new CompositeLogger(new ConsoleLogger(), eventlogLogger, fileLogger));
                        }
                        else
                        {
                            container.RegisterType<IExecute, DisplayHelpCommand>();
                        }
                    }
                    else
                    {
                        container.RegisterType<IExecute, DisplayHelpCommand>();
                    }
                }

                return container.Resolve<IExecute>();
            }
        }
    }
}