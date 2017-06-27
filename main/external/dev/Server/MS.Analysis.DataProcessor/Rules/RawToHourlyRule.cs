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

namespace Microsoft.Spectrum.Analysis.DataProcessor.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.ScanFile;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Models;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;

    public class RawToHourlyRule : AggregationRule
    {
        public RawToHourlyRule(SpectrumDataProcessorStorage spectrumDataProcessStorage)
            : base(spectrumDataProcessStorage)
        {
        }

        public override void Run(IEnumerable<SpectralPsdDataBlock> spectralData, Guid measurementStationKey)
        {
            IEnumerable<SpectrumCalibration> aggregatedSpectralData = this.NormalizeSpectralDataByTimeGranularity(spectralData, TimeRangeKind.Hourly, measurementStationKey);

            foreach (SpectrumCalibration spectralDataModel in aggregatedSpectralData)
            {
                this.SpectrumDataProcessStorage.InsertOrUpdateSpectralData(spectralDataModel);
            }
        }

        public override async Task RunAsync(IEnumerable<SpectralPsdDataBlock> spectralData, Guid measurementStationKey)
        {
            IEnumerable<SpectrumCalibration> aggregatedSpectralData = this.NormalizeSpectralDataByTimeGranularity(spectralData, TimeRangeKind.Hourly, measurementStationKey);

            IEnumerable<Task> insertOrReplaceOperations = aggregatedSpectralData.Select(
                spectrumCalibration =>
                {
                    return this.SpectrumDataProcessStorage.InsertOrUpdateSpectralDataAsync(spectrumCalibration);
                });

            await Task.WhenAll(insertOrReplaceOperations);
        }
    }
}
