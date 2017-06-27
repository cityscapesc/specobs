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

namespace Microsoft.Spectrum.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;    

    /// <summary>
    /// Enumerations of all the different message Ids.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <description>Each major component within the whitespace DB has a range of message Ids which is
    /// defined as:</description>
    /// <item><description>1 to 99 range reserved generic log messages that are not specific to a 
    /// particular component.</description></item>
    /// <item><description>100 to 199 range reserved for the DataProcessor.</description></item>
    /// <item><description>200 to 299 range reserved for the Upload Service.</description></item>
    /// <item><description>300 to 399 range reserved for the Portal.</description></item>
    /// <item><description>400 to 499 range reserved for the Blob Storage.</description></item>
    /// <item><description>500 to 699 reserved for the Storage Model.</description></item>
    /// <item><description>600 to 699 reserved for the Queue Storage.</description></item>
    /// <item><description>700 to 799 reserved for the Table Storage.</description></item>
    /// <item><description>800 to 899 reserved for the Import Service.</description></item>
    /// <item><description>900 to 999 reserved for the Scanning Service.</description></item>
    /// <item><description>1000 to 1099 reserved for the Autoupdate Service.</description></item>
    /// </list>
    /// </remarks>
    public enum LoggingMessageId
    {
        // 1 to 99 contain generic messages that are not tied to a particular component.

        /// <summary>
        /// A Generic log message that is not tied to any component.
        /// </summary>
        GenericMessage = 0,

        DataProcessorBaseId = 100,
        DataProcessorAgentId = 101,              
        NormalizingDataProcessorEventId = 110,
        AggregatingDataProcessorEventId = 111,
        DataProcessorWorkerRoleLogEventId = 112,
        RetentionPolicy = 113,
        Scheduler = 114,
        Settings = 115,
        UpdateTableSharedData = 116,
        SpectrumObservatoriesMonitoringService = 117,
        AdminNotificationService = 118,

        MeasurementStationService = 200,

        ImporterRunAsExeCommand = 800,
        ImporterService = 801,
        ImporterAgent = 802,
        MessageBufferEventId = 803,

        ScanningRunAsExe = 900,
        ScanningRunAsService = 901,
        Scanner = 902,   
        ScanningStarting = 910,
        ScanningStarted = 911,
        ScanningStopping = 912,
        ScanningStopped = 913,
        ScanningConfig = 920,
        ScanningBadFrequency = 921,
        ScanningError = 998,

        AutoUpdateRunAsExe = 1000,
        AutoUpdateRunAsService = 1001,
        AutoUpdateAgent = 1002,
        FileDownloadAgent = 1003,
        AutoUpdatesOn = 1010,
        UpdatesCheck = 1011,
        AutoUpdatesOff = 1012,
        NoUpdates = 1020,       
        NewUpdates = 1021,
        DownloadingUpdates = 1030,
        DownloadedUpdates = 1031,
        InstallingUpdates = 1032,
        InstalledUpdates = 1033,
        AutoUpdateError = 1098,

        PortalUnhandledError = 2000,
        StationRegistrationFailure = 2001,

        RawIQPolicyUploadClient = 3000
    }

    /// <summary>
    /// Interface used to log messages.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Writes the specified message to the log file.
        /// </summary>
        /// <param name="severity">The log message level.</param>
        /// <param name="messageId">The message id.</param>
        /// <param name="message">The message being logged.</param>
        void Log(TraceEventType severity, LoggingMessageId messageId, string message);
    }
}