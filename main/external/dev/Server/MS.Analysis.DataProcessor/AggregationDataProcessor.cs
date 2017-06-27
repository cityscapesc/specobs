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
    using System.Globalization;
    using System.Linq;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.Model;

    /// <summary>
    /// This class contains the logic to aggregate input into average daily data.
    /// </summary>
    public class AggregationDataProcessor : DataProcessorBase<AggregationRequestPayload, AggregationResponsePayload>
    {
        private static Dictionary<ReadingKind, Func<IEnumerable<FixedShort>, FixedShort>> reducers = new Dictionary<ReadingKind, Func<IEnumerable<FixedShort>, FixedShort>>()
        {
            { ReadingKind.Minimum, FixedShortReducer.Min },
            { ReadingKind.Maximum, FixedShortReducer.Max },
            { ReadingKind.Average, FixedShortReducer.Average }
        };

        private readonly IBlockMatrixStorage blockMatrixStorage;

        public AggregationDataProcessor(IBlockMatrixStorage blockMatrixStorage, ILogger logger)
            : base(logger)
        {
            if (blockMatrixStorage == null)
            {
                throw new ArgumentNullException("blockMatrixStorage");
            }

            this.blockMatrixStorage = blockMatrixStorage;
        }

        public override AggregationResponsePayload Execute(AggregationRequestPayload request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            // Given a set of blobs, each blob contains a matrix of blocks of FixedShort numbers
            IEnumerable<string> blobUris = request.BlobUris;

            // Get the reducer func type by the ReadingKind
            Func<IEnumerable<FixedShort>, FixedShort> reducer = reducers[request.ReadingKind];

            // Each blob will be reduced as one row in the result matrix, so the number of blobs is the number of 
            // rows of the result matrix
            // Create the result matrix, which is blobUris.Count x (request.ReadingCount * request.TimeUnitCount)
            int rowCount = blobUris.Count();
            int colCount = request.FrequencyUnitCount * request.FrequencyUnitResolution;

            // NB: it is important to initialize the matrix as NaN, as some blobs may be dummy ones and will be ignored
            // therefore the corresponding row contains all NaN values.
            FixedShortBlock result = new FixedShortBlock(rowCount, colCount, FixedShort.NaN);

            // For each blob, get the matrix by loading and fusing, and reduce the matrix into one row vector, 
            // which is put back to the result matrix
            int row = 0;
            foreach (string blobUri in blobUris)
            {
                FixedShortBlockMatrix blobData = this.blockMatrixStorage.Read(
                    blobUri,
                    new BlockMatrixSizeInfo(
                        request.TimeUnitCount,
                        request.FrequencyUnitCount,
                        request.TimeUnitResolution,
                        request.FrequencyUnitResolution));

                // TODO: Fusing matrix should be merged into blobFormatter.Read logic
                FixedShortBlock matrix = blobData.Fuse();
                if (matrix.ColumnCount != colCount)
                {
                    throw new InvalidOperationException(
                        string.Format(CultureInfo.InvariantCulture, "The matrix contained in the blob ({0}) doesn't match the column count ({1})", blobUri, colCount));
                }

                IEnumerable<FixedShort> reducedResult = matrix.ReduceColumns<FixedShort>(reducer);

                // copy the reduced result into result matrix;
                int j = 0;
                foreach (FixedShort v in reducedResult)
                {
                    result[row, j++] = v;
                }

                row++;
            }

            // Splitting matrix into blocks, and then store the block matrix into blob;
            // TODO: The splitting can be merged into the store data function for better performance
            FixedShortBlockMatrix splittedResult = result.Split(request.TimeUnitCount, request.FrequencyUnitCount);
            string localPath = Guid.NewGuid().ToString() + ".bin";
            string resultBlobUri = this.blockMatrixStorage.Write(splittedResult, localPath);

            return new AggregationResponsePayload(request, resultBlobUri);
        }

        protected override AggregationResponsePayload CreateExceptionResponse(Exception exception, AggregationRequestPayload inputRequest)
        {
            Logger.Log(TraceEventType.Error, EventIds.AggregatingDataProcessorEventId, exception);
            return new AggregationResponsePayload(inputRequest, exception);
        }
    }
}
