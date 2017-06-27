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
    using System.Globalization;

    public class ConsoleLogger : ILogger
    {
        private static Dictionary<TraceEventType, ConsoleColor> eventColors = new Dictionary<TraceEventType, ConsoleColor>()
        {
            { TraceEventType.Critical,    ConsoleColor.Red },
            { TraceEventType.Error,       ConsoleColor.Red },
            { TraceEventType.Information, ConsoleColor.White },
            { TraceEventType.Resume,      ConsoleColor.Green },
            { TraceEventType.Start,       ConsoleColor.Green },
            { TraceEventType.Stop,        ConsoleColor.Green },
            { TraceEventType.Suspend,     ConsoleColor.Green },
            { TraceEventType.Transfer,    ConsoleColor.Green },
            { TraceEventType.Verbose,     ConsoleColor.Cyan },
            { TraceEventType.Warning,     ConsoleColor.Yellow }
        };

        public void Log(TraceEventType severity, LoggingMessageId messageId, string message)
        {
            ConsoleColor oldColor = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = eventColors[severity];
                string formattedMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}|{1}|{2}|{3}",
                    DateTime.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                    severity,
                    messageId,
                    message);

                Console.WriteLine(formattedMessage);
            }
            catch (Exception)
            {
                // If the message fails to write, ignore it and move on
            }
            finally
            {
                Console.ForegroundColor = oldColor;
            }
        }
    }
}