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

namespace Microsoft.Spectrum.IO.ScanFile
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using ProtoBuf;

    public class ScanFileReader : IDisposable
    {
        private Stream decompressedStream;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Target = "MemoryStream", Justification = "It will be closed when we close the BinaryReader")]
        public ScanFileReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            this.decompressedStream = new DeflateStream(stream, CompressionMode.Decompress);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ScanFile Read()
        {
            return Serializer.Deserialize<ScanFile>(this.decompressedStream);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.decompressedStream.Close();
            }
        }
    }
}
