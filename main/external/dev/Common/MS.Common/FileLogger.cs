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
    using System.Globalization;
    using System.IO;
    using System.Text;

    public class FileLogger : ILogger
    {
        public static readonly string Extension = ".dsol";

        private string filePath;
        private string logType = string.Empty;

        public FileLogger(string filePath, string logType)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("filePath can not be null", "filePath");
            }

            this.filePath = filePath;
            this.logType = logType;
        }

        public void Log(TraceEventType severity, LoggingMessageId messageId, string message)
        {
            try
            {
                string formattedMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}|{1}|{2}|{3}",
                    DateTime.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                    severity,
                    messageId,
                    message);

                string writeDateTime = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture);
                writeDateTime = writeDateTime.Replace(":", null);

                File.AppendAllLines(this.filePath + "\\" + this.logType + writeDateTime + FileLogger.Extension, new[] { formattedMessage });
            }
            catch (Exception)
            {
                // If the message fails to write, ignore it and move on
            }
        }        
    }
}