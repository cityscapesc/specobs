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

namespace Microsoft.Spectrum.Storage.Models
{    
    using System;
    using Microsoft.Spectrum.Common;

    public class ScanFileInformation
    {
        public ScanFileInformation(Guid measurementStationKey, DateTime timeStart, int compressionType, FileType scanFileType, string blobUri, double startFrequency, double endFrequency)
        {
            if (measurementStationKey == null)
            {
                throw new ArgumentNullException("measurementStationKey", "measurementStationKey can not be null");
            }

            if (string.IsNullOrWhiteSpace(blobUri))
            {
                throw new ArgumentException("Invalid blobUri paramater name.", "blobUri");
            }

            this.MeasurementStationKey = measurementStationKey;
            this.TimeStart = timeStart;
            this.CompressionType = compressionType;
            this.TypeId = scanFileType;
            this.BlobUri = blobUri;
            this.StartFrequency = startFrequency;
            this.EndFrequency = endFrequency;
        }

        public Guid MeasurementStationKey { get; private set; }

        public DateTime TimeStart { get; private set; }

        public int CompressionType { get; private set; }

        public FileType TypeId { get; private set; }

        public string BlobUri { get; private set; }

        public double StartFrequency { get; private set; }

        public double EndFrequency { get; private set; }
    }
}
