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

namespace Microsoft.Spectrum.Storage
{
    using System;

    public class RawSpectralDataInfo
    {
        public RawSpectralDataInfo(Guid stationId, DateTime timeStart, int compressionType, int typeId, string blobUri)
        {
            this.StationId = stationId;
            this.TimeStart = timeStart;
            this.CompressionType = compressionType;
            this.TypeId = typeId;
            this.BlobUri = blobUri;
        }

        /// <summary>
        /// Gets and sets measurement station Id of RawSpectralDataInfo
        /// </summary>
        public Guid StationId { get; set; }

        /// <summary>
        /// Gets the TimeStart of the RawSpectralDataInfo. 
        /// </summary>
        public DateTime TimeStart { get; set; }

        /// <summary> 
        /// Gets and sets the CompressionType of the RawSpectralDataInfo 
        /// </summary>
        public int CompressionType { get; set; }

        /// <summary> 
        /// Gets and sets the TypeId of the RawSpectralDataInfo 
        /// </summary>
        public int TypeId { get; set; }

        /// <summary> 
        /// Gets the BlobUri of the RawSpectralDataInfo
        /// </summary>
        public string BlobUri { get; set; }
    }
}
