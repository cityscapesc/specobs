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
    using ProtoBuf;    

    public class ScanFileWriter
    {
        private Stream output;
        private ScanFile input;

        public ScanFileWriter(Stream output, DateTime timestamp)
        {
            this.TimeStamp = timestamp;
            this.output = output;
            this.input = new ScanFile();
        }
        
        public DateTime TimeStamp { get; private set; }

        public void WriteBlock(DataBlock block)
        {
            if (block.GetType() == typeof(SpectralPsdDataBlock))
            {
                this.input.SpectralPsdData.Add(block as SpectralPsdDataBlock);
            }
            else if (block.GetType() == typeof(ConfigDataBlock))
            {
                this.input.Config = block as ConfigDataBlock;
            }
        }

        public void Close()
        {
            Serializer.Serialize<ScanFile>(this.output, this.input);

            this.output.Dispose();
            this.output = null;
        }
    }
}