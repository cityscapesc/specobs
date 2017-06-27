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
    using System.Diagnostics;   
    using System.Globalization;
    using System.Text;

    // TODO: Add a constructor that takes the WAD configuration as a parameter

    /// <summary>
    /// Directs log messages to Logger.Log, which puts the messages in Azure.
    /// </summary>
    public class AzureLogger : ILogger
    {
        private const string ConfigurationName = "AzureLog";
        private TraceSource traceSource;

        /// <summary>
        /// Prevents a default instance of the Logger class from being created.
        /// </summary>
        public AzureLogger()
        {
            this.traceSource = new TraceSource(ConfigurationName);
            this.AddDefaultTraceListeners();
        }

        // Hook for the unit tests to plug in a stub.
        internal Action<TraceEventType, int, string> LogHook { get; set; }

        public void Log(TraceEventType severity, LoggingMessageId messageId, string message)
        {
            // Add a log hook
            if (this.LogHook != null)
            {
                this.LogHook(severity, (int)messageId, message);
            }
            else
            {
                try
                {
                    string formattedMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Event: {0} | Timestamp: {1} | Message: {2} |",
                        messageId.ToString(),
                        DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                        message);

                    this.traceSource.TraceEvent(severity, (int)messageId, formattedMessage);
                }
                catch
                {
                    // If there is an error while logging, ignore it.
                }
            }
        }

        public void Log(TraceEventType eventType, LoggingMessageId logEventId, Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(CultureInfo.InvariantCulture, "EventId: {0} |", logEventId);
                sb.AppendFormat(CultureInfo.InvariantCulture, "Timestamp: {0} |", DateTime.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture));
                sb.AppendFormat(CultureInfo.InvariantCulture, "Exception: {0} |", exception.ToString());

                // Log the additional data in the exception if it exists
                if (exception.Data != null && exception.Data.Keys.Count != 0)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, "Data: ");
                    foreach (object key in exception.Data.Keys)
                    {
                        object value = exception.Data[key];
                        sb.AppendFormat(CultureInfo.InvariantCulture, "'{0}'='{1}'; ", key.ToString(), value.ToString());
                    }
                }

                this.traceSource.TraceEvent(eventType, (int)logEventId, sb.ToString());
            }
            catch
            {
                // If there is an error while logging, ignore it.
            }
        }

        private void AddDefaultTraceListeners()
        {
            foreach (TraceListener listener in Trace.Listeners)
            {
                bool alreadyAdded = false;

                foreach (var item in this.traceSource.Listeners)
                {
                    if (item.GetType() == listener.GetType())
                    {
                        alreadyAdded = true;
                        break;
                    }
                }

                if (!alreadyAdded)
                {
                    this.traceSource.Listeners.Add(listener);
                }
            }
        }        
    }
}