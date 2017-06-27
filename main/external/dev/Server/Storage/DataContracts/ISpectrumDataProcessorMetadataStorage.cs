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

namespace Microsoft.Spectrum.Storage.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Storage.Enums;
    using Microsoft.Spectrum.Storage.Models;

    public interface ISpectrumDataProcessorMetadataStorage
    {
        IEnumerable<SpectrumFrequency> GetSpectralData(Guid measurementStationId, TimeRangeKind timeGranularity, DateTime timeStart);

        IEnumerable<SpectrumFrequency> GetSpectralDataByFrequency(Guid measurementStationId, TimeRangeKind timeGranularity, DateTime timeStart, long startFrequency, long endFrequency);

        void InsertOrUpdateSpectralData(SpectrumCalibration spectralData);

        Task InsertOrUpdateSpectralDataAsync(SpectrumCalibration spectralData);

        void InsertOrUpdateScanFileInformation(ScanFileInformation scanFileInformation);

        void PersistScanFileProcessingError(ScanFileProcessingError scanFileProcessingError);

        IEnumerable<ScanFileProcessingError> GetAllSpectrumFileProcessingFailures(Guid measurementStationId, DateTime failuresSince);

        IEnumerable<ScanFileInformation> GetSpectralMetadataInfo(Guid measurementStationId, DateTime timeStart);

        IEnumerable<ScanFileInformation> GetSpectralMetadataInfoByTimeRange(Guid measurementStationId, DateTime timeStart, DateTime timeEnd, int rawDataType);

        IEnumerable<RawSpectralDataInfo> GetRawSpectralDataSchema(Guid measurementStationId,FileType fileType);
    }
}
