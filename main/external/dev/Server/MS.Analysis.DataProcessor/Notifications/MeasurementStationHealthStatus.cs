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

namespace Microsoft.Spectrum.Analysis.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.Models;    

    public class MeasurementStationHealthStatus
    {
        public MeasurementStationHealthStatus(MeasurementStationIdentifier measurementStationIdentifier, IEnumerable<string> adminEmailAddress, string healthStatusMessage, DateTime monitoringStartTime, int fileDueInProcessingQueue)
        {
            if (measurementStationIdentifier == null)
            {
                throw new ArgumentNullException("measurementStationIdentifier", "MeasurementStation Identifier can not be null");
            }

            if (adminEmailAddress == null)
            {
                throw new ArgumentNullException("adminEmailAddress", "Station Administrator email address can not be null");
            }

            if (string.IsNullOrWhiteSpace(healthStatusMessage))
            {
                throw new ArgumentException("MeasurementStation health status message can not be empty", "healthStatusMessage");
            }

            this.StationIdentifier = measurementStationIdentifier;
            this.AdminEmailAddress = adminEmailAddress;
            this.StatusMessage = healthStatusMessage;
            this.MonitoringStartTime = monitoringStartTime;
            this.FilesDueInProcessingQueue = fileDueInProcessingQueue;
        }

        public MeasurementStationHealthStatus(MeasurementStationIdentifier measurementStationIdentifier, IEnumerable<string> adminEmailAddress, string healthStatusMessage, DateTime monitoringStartTime, int fileDueInProcessingQueue, IEnumerable<ScanFileProcessingError> unprocessedFiles)
            : this(measurementStationIdentifier, adminEmailAddress, healthStatusMessage, monitoringStartTime, fileDueInProcessingQueue)
        {
            if (unprocessedFiles == null)
            {
                throw new ArgumentNullException("unprocessedFiles", "Unprocessed files can not be null");
            }

            this.FileProcessingErrors = unprocessedFiles;
        }

        public MeasurementStationIdentifier StationIdentifier { get; private set; }

        public IEnumerable<string> AdminEmailAddress { get; private set; }

        public IEnumerable<ScanFileProcessingError> FileProcessingErrors { get; private set; }

        public DateTime MonitoringStartTime { get; private set; }

        public string StatusMessage { get; private set; }

        public int FilesDueInProcessingQueue { get; private set; }

        public IEnumerable<IssueReport> IssuesReported { get; set; }
    }
}
