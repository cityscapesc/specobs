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
    using System.Diagnostics;

    /// <summary>
    /// Allows a number of individual loggers to consume the same log message.
    /// </summary>
    public class CompositeLogger : ILogger
    {
        private ILogger[] loggers;

        public CompositeLogger(params ILogger[] loggers)
        {
            if (loggers == null)
            {
                throw new ArgumentNullException("loggers");
            }

            this.loggers = loggers;
        }

        public void Log(TraceEventType severity, LoggingMessageId messageId, string message)
        {
            foreach (ILogger logger in this.loggers)
            {
                try
                {
                    logger.Log(severity, messageId, message);
                }
                catch (Exception)
                {
                    // Intentionally eat the exception so that one poorly-behaving logger cannot prevent the message
                    // from reaching other loggers in the collection
                }
            }
        }       
    }
}