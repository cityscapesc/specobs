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

    public class EventLogLogger : ILogger
    {
        private static Dictionary<TraceEventType, EventLogEntryType> eventTypes = new Dictionary<TraceEventType, EventLogEntryType>()
        {
            { TraceEventType.Critical,    EventLogEntryType.Error },
            { TraceEventType.Error,       EventLogEntryType.Error },
            { TraceEventType.Information, EventLogEntryType.Information },
            { TraceEventType.Resume,      EventLogEntryType.Information },
            { TraceEventType.Start,       EventLogEntryType.Information },
            { TraceEventType.Stop,        EventLogEntryType.Information },
            { TraceEventType.Suspend,     EventLogEntryType.Information },
            { TraceEventType.Transfer,    EventLogEntryType.Information },
            { TraceEventType.Verbose,     EventLogEntryType.Information },
            { TraceEventType.Warning,     EventLogEntryType.Warning }
        };

        private readonly string eventSourceName;

        public EventLogLogger(string eventSourceName)
        {
            if (string.IsNullOrWhiteSpace(eventSourceName))
            {
                throw new ArgumentNullException("eventSourceName", "The specified eventSourceName is not valid.");
            }

            this.eventSourceName = eventSourceName;

            if (!EventLog.SourceExists(this.eventSourceName))
            {
                EventLog.CreateEventSource(this.eventSourceName, "Application");
            }
        }

        public void Log(TraceEventType severity, LoggingMessageId messageId, string message)
        {
            try
            {
                EventLog.WriteEntry(this.eventSourceName, message, eventTypes[severity], (int)messageId);
            }
            catch (Exception)
            {
                // Logging event log messages should not throw exceptions, so eat it.
                // At least we tried.
            }
        }
    }
}