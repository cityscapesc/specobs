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
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Storage;

    /// <summary>
    /// This class contains the logic to normalize the raw spectrum data into hourly data.
    /// </summary>
    public class NormalizationDataProcessor : DataProcessorBase<NormalizationRequestPayload, NormalizationResponsePayload>
    {
        private readonly IBlockStorage blockStorage;
        private readonly IBlockMatrixStorage blockMatrixStorage;

        /// <summary>
        /// Creates an instance of the NormalizationDataProcessor.
        /// </summary>
        /// <param name="blockStorage">The block storage where the raw PSD data is read from.</param>
        /// <param name="blockMatrixStorage">The block matrix storage where the normalized data is written to.</param>
        /// <param name="logger">The logger where messages are written.</param>
        public NormalizationDataProcessor(IBlockStorage blockStorage, IBlockMatrixStorage blockMatrixStorage, ILogger logger) : base(logger)
        {
            if (blockStorage == null)
            {
                throw new ArgumentNullException("blockStorage");
            }

            if (blockMatrixStorage == null)
            {
                throw new ArgumentNullException("blockMatrixStorage");
            }

            this.blockStorage = blockStorage;
            this.blockMatrixStorage = blockMatrixStorage;
        }

        /// <summary>
        /// Overridden method to execute the data processing logic.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The response.</returns>
        public override NormalizationResponsePayload Execute(NormalizationRequestPayload request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            // 1: generate the frequency samples
            float[] freqSamples = MathLibrary.GetLogarithmicSpace(
                request.StartFrequency, 
                request.StopFrequency,
                request.FrequencyUnitCount * request.FrequencyUnitResolution).ToArray();

            // 2: create a matrix for normalized data which is (ReadingCount * TimeUnitCount) * (FrequencyChunkCount * FrequencyBandsSamplingPointsCount)
            // NOTE: initializing each number in matrix as NaN is important as the value for each sample which is in the Gap area is NaN
            FixedShortBlock normalizedMatrix = new FixedShortBlock(
                request.TimeUnitCount * request.TimeUnitResolution,
                request.FrequencyUnitCount * request.FrequencyUnitResolution,
                FixedShort.NaN);

            // 3: for each raw psd blob, normalize it into the matrix
            foreach (RawPsdBlobInfo rawPsdBlobInfo in request.RawPsdBlobs)
            {
                int rowCount = rawPsdBlobInfo.TimeRange.IntervalCount;
                int colCount = rawPsdBlobInfo.FrequencyRanges.Select(fr => fr.IntervalCount).Sum();

                // skip the blank blob
                if (KnownBlobUris.IsBlankBlobUri(rawPsdBlobInfo.BlobUri))
                {
                    continue;
                }

                FixedShortBlock rawDataMatrix = this.blockStorage.Read(rawPsdBlobInfo.BlobUri, rowCount, colCount, request.StationCompressesData);

                NormalizationHelper.NormalizeRawPSD(
                    rawDataMatrix,
                    rawPsdBlobInfo.FrequencyRanges,
                    rawPsdBlobInfo.TimeRange,
                    DateTimeLinearRange.Create(request.StartDateTime, request.StopDateTime, request.TimeUnitCount * request.TimeUnitResolution),
                    freqSamples, 
                    normalizedMatrix);
            }

            // 4: split the whole matrix into FrequencyChunkCount * TimeUnitCount chunks
            // TODO: The splitting can be merged in the StoreData to save the memory.
            FixedShortBlockMatrix output = normalizedMatrix.Split(request.TimeUnitCount, request.FrequencyUnitCount);

            // 5: store the split matrix data into blobs;
            string localPath = Guid.NewGuid().ToString() + ".bin";
            string resultBlobUri = this.blockMatrixStorage.Write(output, localPath);

            return new NormalizationResponsePayload(request, resultBlobUri);
        }

        /// <summary>
        /// Overridden method to create the response with the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="inputRequest">AverageHourDataRequestPayload</param>
        /// <returns>The response.</returns>        
        protected override NormalizationResponsePayload CreateExceptionResponse(Exception exception, NormalizationRequestPayload inputRequest)
        {
            Logger.Log(TraceEventType.Error, EventIds.NormalizingDataProcessorEventId, exception);

            return new NormalizationResponsePayload(inputRequest, exception);
        }
    }
}
