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
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.Enums;
    using Microsoft.Spectrum.Storage.Models;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage
    /// Class:          IMeasurementStationTableOperations
    /// Description:    interface for <see cref="MeasurementStationTableOperations"/> class
    /// ----------------------------------------------------------------- 
    public interface IMeasurementStationTableOperations
    {
        IEnumerable<MeasurementStationInfo> GetAllMeasurementStationInfoPublic();

        IEnumerable<MeasurementStationInfo> GetAllMeasurementStationInfoPrivate();

        MeasurementStationInfo GetMeasurementStationInfoPublic(Guid measurementStationId);

        MeasurementStationInfo GetMeasurementStationInfoPrivate(Guid measurementStationId);

        void AddMeasurementStation(MeasurementStationInfo station);

        void UpdateStationData(MeasurementStationInfo latestMeasurementStationInfo);

        void UpdateStationHardwareInformation(Guid measurementStationId, string hardwareInformation);

        void UpdateStationAvailabilityStatus(Guid measurementStationId, StationAvailability availabilityStatus);

        void UpdatePSDDataAvailabilityTimestamp(Guid measurementStationId, DateTime startTime, DateTime endTime);

        void UpdateRawIQDataAvailabilityTimestamp(Guid measurementStationId, DateTime startTime, DateTime endTime);

        IEnumerable<RawIQPolicy> GetOverlappingIQBands(long rawIQStartFrequency, long rawIQStopFrequency);

        double GetFFTBinWidth(Guid measurementStationId, DateTime startTime, DateTime endTime);
    }
}
