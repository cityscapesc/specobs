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

    public class ScanFileProcessingError
    {
        public ScanFileProcessingError(Guid measurementStationId, string absoluteFilePath, string error, DateTime timeOfFailure)
        {
            if (measurementStationId == null)
            {
                throw new ArgumentNullException("measurementStationId", "MeasurmentStationId parameter can not be null");
            }

            if (string.IsNullOrEmpty(absoluteFilePath))
            {
                throw new ArgumentException("ScanFile absoluteFilePath can not be empty", "absoluteFilePath");
            }

            if (string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentException("Error details parameter can not be empty", "error");
            }

            this.MeasurementStationId = measurementStationId;
            this.AbsoluteFilePath = absoluteFilePath;
            this.TimeOfFailure = timeOfFailure;
            this.Error = error;
        }

        public Guid MeasurementStationId { get; private set; }

        public string AbsoluteFilePath { get; private set; }

        public DateTime TimeOfFailure { get; private set; }

        public string Error { get; private set; }
    }
}
