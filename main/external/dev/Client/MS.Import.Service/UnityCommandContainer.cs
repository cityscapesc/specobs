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
    using System.Configuration;
    using System.Globalization;
    using System.ServiceProcess;
    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.Configuration;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.MeasurementStation.Client;    

    public class UnityCommandContainer : ICommandContainer
    {
        public ICommand ResolveCommand(string[] args)
        {
            IUnityContainer container = new UnityContainer();            

            EventLogLogger eventlogLogger = new EventLogLogger("SpectrumImportService");

            LoggingConfigurationSection loggingDetails = (LoggingConfigurationSection)ConfigurationManager.GetSection("LoggingConfiguration");            
            FileLogger fileLogger = new FileLogger(Environment.ExpandEnvironmentVariables(loggingDetails.LoggingDirectory), "ImportService");

            if (args.Length == 0)
            {
                container.RegisterType<ICommand, RunAsServiceCommand>();
                container.RegisterType<ServiceBase, ImporterService>();
                container.RegisterInstance<ILogger>(new CompositeLogger(fileLogger, eventlogLogger));

                EventExtensions.Logger = eventlogLogger;
            }
            else if (args.Length == 1)
            {
                if (string.Compare(args[0], "RunAsExe", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    CompositeLogger compositeLogger = new CompositeLogger(new ConsoleLogger(), eventlogLogger, fileLogger);

                    container.RegisterType<ICommand, RunAsExeCommand>();
                    container.RegisterInstance<ILogger>(compositeLogger);

                    EventExtensions.Logger = compositeLogger;
                }
                else
                {
                    container.RegisterType<ICommand, DisplayHelpCommand>();
                }
            }
            else
            {
                container.RegisterType<ICommand, DisplayHelpCommand>();
            }

            container.RegisterType<IConfigurationSource, AppConfigConfigurationSource>();

            return container.Resolve<ICommand>();
        }
    }
}