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

namespace Microsoft.Spectrum.Storage.Table.Azure.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Spectrum.Common;

    public class SettingsTableHelper
    {
        public const string SchedulerCategory = "Scheduler";
        public const string RetentionPolicyCategory = "RetentionPolicy";
        public const string DataProcessorCategory = "DataProcessor";
        public const string ConcurrencyHandlerCategory = "PessimisticConcurrencyHandler";
        public const string TableSharedAccess = "TableSharedAccess";
        public const string SpectrumObservatoriesMonitoringService = "SpectrumObservatoriesMonitoringService";
        public const string AdminNotificationService = "AdminNotificationService";
        public const string SpectrumDataStorageAccountsTableOperations = "SpectrumDataStorageAccountsTableOperations";
        public const string DeviceSetupCategory = "DeviceSetup";

        private static readonly object Mutex = new object();
        private static SettingsTableHelper instance;
        private static bool initialized = false;
        private RetryAzureTableOperations<Settings> settingsTable;
        private ILogger logger;

        private SettingsTableHelper()
        {
        }

        public static SettingsTableHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SettingsTableHelper();
                }

                return instance;
            }
        }

        public void Initialize(RetryAzureTableOperations<Settings> settingsTableContext, ILogger loggerInput)
        {
            if (settingsTableContext == null)
            {
                throw new ArgumentNullException("settingsTableContext");
            }

            if (loggerInput == null)
            {
                throw new ArgumentNullException("loggerInput");
            }

            lock (SettingsTableHelper.Mutex)
            {
                if (!initialized)
                {
                    this.settingsTable = settingsTableContext;
                    this.logger = loggerInput;

                    this.settingsTable.GetTableReference(AzureTableHelper.SettingsTable);

                    initialized = true;
                }
            }
        }

        public TimeSpan GetSetting(string category, string name, TimeSpan defaultResult)
        {
            try
            {
                Settings setting = this.settingsTable.GetByKeys<Settings>(category, name).SingleOrDefault();

                if (setting != null)
                {
                    return TimeSpan.Parse(setting.Value, CultureInfo.InvariantCulture);
                }
                else
                {
                    // Insert a setting to Settings table, its value is initialized with defaultResult
                    this.SetSetting(category, name, defaultResult);
                }
            }
            catch
            {
                this.logger.Log(System.Diagnostics.TraceEventType.Error, LoggingMessageId.Settings, string.Format(CultureInfo.InvariantCulture, "Failed to get settings {0}:{1}", category, name));
            }

            return defaultResult;
        }

        public string GetSetting(string category, string name, string defaultResult)
        {
            try
            {
                Settings setting = this.settingsTable.GetByKeys<Settings>(category, name).SingleOrDefault();

                if (setting != null)
                {
                    return setting.Value;
                }
                else
                {
                    // Insert a setting to Settings table, its value is initialized with defaultResult
                    this.SetSetting(category, name, defaultResult);
                }
            }
            catch
            {
                this.logger.Log(System.Diagnostics.TraceEventType.Error, LoggingMessageId.Settings, string.Format(CultureInfo.InvariantCulture, "Failed to get settings {0}:{1}", category, name));
            }

            return defaultResult;
        }

        public int GetSetting(string category, string name, int defaultResult)
        {
            try
            {
                Settings setting = this.settingsTable.GetByKeys<Settings>(category, name).SingleOrDefault();

                if (setting != null)
                {
                    return Convert.ToInt32(setting.Value, CultureInfo.InvariantCulture);
                }
                else
                {
                    // Insert a setting to Settings table, its value is initialized with defaultResult
                    this.SetSetting(category, name, defaultResult);
                }
            }
            catch
            {
                this.logger.Log(System.Diagnostics.TraceEventType.Error, LoggingMessageId.Settings, string.Format(CultureInfo.InvariantCulture, "Failed to get settings {0}:{1}", category, name));
            }

            return defaultResult;
        }

        public bool GetSetting(string category, string name, bool defaultResult)
        {
            try
            {
                Settings setting = this.settingsTable.GetByKeys<Settings>(category, name).SingleOrDefault();

                if (setting != null)
                {
                    int byteValue = Convert.ToByte(setting.Value, CultureInfo.InvariantCulture);
                    return Convert.ToBoolean(byteValue);
                }
                else
                {
                    // Insert a setting to Settings table, its value is initialized with defaultResult
                    this.SetSetting(category, name, defaultResult);
                }
            }
            catch
            {
                this.logger.Log(System.Diagnostics.TraceEventType.Error, LoggingMessageId.Settings, string.Format(CultureInfo.InvariantCulture, "Failed to get settings {0}:{1}", category, name));
            }

            return defaultResult;
        }

        public void SetSetting(string category, string name, int value)
        {
            try
            {
                Settings entity = new Settings(category, name, Convert.ToString(value, CultureInfo.InvariantCulture));
                this.settingsTable.InsertOrReplaceEntity(entity, true);
            }
            catch
            {
                this.logger.Log(System.Diagnostics.TraceEventType.Error, LoggingMessageId.Settings, string.Format(CultureInfo.InvariantCulture, "Failed to set settings {0}:{1} to {2}", category, name, value));
            }
        }

        public void SetSetting(string category, string name, TimeSpan value)
        {
            try
            {
                Settings entity = new Settings(category, name, Convert.ToString(value, CultureInfo.InvariantCulture));
                this.settingsTable.InsertOrReplaceEntity(entity, true);
            }
            catch
            {
                this.logger.Log(System.Diagnostics.TraceEventType.Error, LoggingMessageId.Settings, string.Format(CultureInfo.InvariantCulture, "Failed to set settings {0}:{1} to {2}", category, name, value));
            }
        }

        public void SetSetting(string category, string name, string value)
        {
            try
            {
                Settings entity = new Settings(category, name, value);
                this.settingsTable.InsertOrReplaceEntity(entity, true);
            }
            catch
            {
                this.logger.Log(System.Diagnostics.TraceEventType.Error, LoggingMessageId.Settings, string.Format(CultureInfo.InvariantCulture, "Failed to set settings {0}:{1} to {2}", category, name, value));
            }
        }

        public void SetSetting(string category, string name, bool value)
        {
            try
            {
                byte byteValue = Convert.ToByte(value);

                Settings entity = new Settings(category, name, Convert.ToString(byteValue, CultureInfo.InvariantCulture));
                this.settingsTable.InsertOrReplaceEntity(entity, true);
            }
            catch
            {
                this.logger.Log(System.Diagnostics.TraceEventType.Error, LoggingMessageId.Settings, string.Format(CultureInfo.InvariantCulture, "Failed to set settings {0}:{1} to {2}", category, name, value));
            }
        }
    }
}
