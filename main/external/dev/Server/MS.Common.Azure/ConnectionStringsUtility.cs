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

namespace Microsoft.Spectrum.Common.Azure
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using Microsoft.WindowsAzure.ServiceRuntime;

    /// <summary>
    /// Holds all configured values both in web.config and corresponding authority config file
    /// </summary>
    public static class ConnectionStringsUtility
    {
        /// <summary>
        /// Constant key for Wad Connection String 
        /// </summary>
        public const string WadConnectionStringKey = "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString";

        /// <summary>
        /// Constant for Lease Container.
        /// </summary>
        public const string LeaseContainer = "leaseobjects";

        /// <summary>
        /// Constant for HealthReportMessagingSubscription.
        /// </summary>
        public const string HealthReportMessagingSubscription = "StationHealthReport";

        /// <summary>
        /// Constant key for Storage Account Connection String
        /// </summary>
        private const string StorageAccountConnectionStringKey = "StorageAccountConnectionString";

        /// <summary>
        /// Constant key for Worker Threads count.
        /// </summary>
        private const string WorkerThreadsCountKey = "WorkerThreadsCount";

        /// <summary>
        /// Constant key for Worker Queue Name
        /// </summary>
        private const string WorkerQueueNameKey = "scanfile-queue";        

        /// <summary>
        /// Constant key for Health Status Queue Name
        /// </summary>
        private const string HealthStatusQueueNameKey = "health-status-queue";

        /// <summary>
        /// constant key for live client id
        /// </summary>
        private const string LiveClientIdKey = "LiveClientId";

        /// <summary>
        /// constant key for live secret id
        /// </summary>
        private const string LiveSecretClientIdKey = "LiveSecretClientId";

        /// <summary>
        /// constant key for Health Status Azure Service Bus ConnectionString key.
        /// </summary>
        private const string HealthStatusServiceBusConnectionStringKey = "HealthStatusServieBusConnectionString";

        private const string HighPriorityThreadCountKey = "HighPriorityThreadCount";

        private const string MediumPriorityThreadCountKey = "MediumPriorityThreadCount";

        private const string LowPriorityThreadCountKey = "LowPriorityThreadCount";




        private const string RequestScopesKey = "RequestScopes";

        /// <summary>
        /// Gets <see cref="StorageAccountConnectionString"/> from configuration
        /// </summary>
        public static string StorageAccountConnectionString
        {
            get
            {
                return RoleEnvironment.IsAvailable
                        ? RoleEnvironment.GetConfigurationSettingValue(StorageAccountConnectionStringKey)
                        : ConfigurationManager.AppSettings[StorageAccountConnectionStringKey];
            }
        }

        /// <summary>
        /// Gets <see cref="WadConnectionString"/> from configuration
        /// </summary>
        public static string WadConnectionString
        {
            get
            {
                return RoleEnvironment.IsAvailable
                        ? RoleEnvironment.GetConfigurationSettingValue(WadConnectionStringKey)
                        : ConfigurationManager.AppSettings[WadConnectionStringKey];
            }
        }

        /// <summary>
        /// Gets <see cref="WorkerThreadsCountKey"/> from configuration.
        /// </summary>
        public static int WorkerThreadsCount
        {
            get
            {
                // From our measuements, we should default to three worker threads on a small VM
                int threadsCount = 3;

                if (RoleEnvironment.IsAvailable)
                {
                    try
                    {
                        threadsCount = Convert.ToInt32(RoleEnvironment.GetConfigurationSettingValue(WorkerThreadsCountKey), CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        // Do nothing here.
                    }
                }

                return threadsCount;
            }
        }

        /// <summary>
        /// Gets <see cref="WorkerQueueName"/> from configuration
        /// </summary>
        public static string WorkerQueueName
        {
            get
            {
                return WorkerQueueNameKey;
            }
        }

        /// <summary>
        /// Gets <see cref="HealthReportQueueName"/> from configuration
        /// </summary>
        public static string HealthReportQueueName
        {
            get
            {
                return HealthStatusQueueNameKey;
            }
        }

        public static string HealthReportServiceBusConnectionString
        {
            get
            {
                return RoleEnvironment.IsAvailable
                    ? RoleEnvironment.GetConfigurationSettingValue(HealthStatusServiceBusConnectionStringKey)
                    : ConfigurationManager.AppSettings[HealthStatusServiceBusConnectionStringKey];
            }
        }

        public static int HighPriorityThreadCount
        {
            get
            {
                return RoleEnvironment.IsAvailable
                    ? int.Parse(RoleEnvironment.GetConfigurationSettingValue(HighPriorityThreadCountKey))
                    : int.Parse(ConfigurationManager.AppSettings[HighPriorityThreadCountKey].ToString());
            }
        }

        public static int MediumPriorityThreadCount
        {
            get
            {
                return RoleEnvironment.IsAvailable
                    ? int.Parse(RoleEnvironment.GetConfigurationSettingValue(MediumPriorityThreadCountKey))
                    : int.Parse(ConfigurationManager.AppSettings[MediumPriorityThreadCountKey].ToString());
            }
        }

        public static int LowPriorityThreadCount
        {
            get
            {
                return RoleEnvironment.IsAvailable
                    ? int.Parse(RoleEnvironment.GetConfigurationSettingValue(LowPriorityThreadCountKey))
                    : int.Parse(ConfigurationManager.AppSettings[LowPriorityThreadCountKey].ToString());
            }
        }

        /// <summary>
        /// Gets <see cref="LiveClientId"/> from configuration
        /// </summary>
        public static string LiveClientId
        {
            get
            {
                return RoleEnvironment.IsAvailable
                        ? RoleEnvironment.GetConfigurationSettingValue(LiveClientIdKey)
                        : ConfigurationManager.AppSettings[LiveClientIdKey];
            }
        }

        /// <summary>
        /// Gets <see cref="LiveSecretClientId"/> from configuration
        /// </summary>
        public static string LiveSecretClientId
        {
            get
            {
                return RoleEnvironment.IsAvailable
                        ? RoleEnvironment.GetConfigurationSettingValue(LiveSecretClientIdKey)
                        : ConfigurationManager.AppSettings[LiveSecretClientIdKey];
            }
        }

        /// <summary>
        /// Gets <see cref="RequestScopes"/> from configuration
        /// </summary>
        public static string RequestScopes
        {
            get
            {
                return RoleEnvironment.IsAvailable
                        ? RoleEnvironment.GetConfigurationSettingValue(RequestScopesKey)
                        : ConfigurationManager.AppSettings[RequestScopesKey];
            }
        }
    }
}
