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

namespace Microsoft.Spectrum.IO.RawIqFile
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using ProtoBuf;
    using Common;

    public class RawIqFileWriter
    {
        private Stream output;
        private RawIqFile input;
        private ILogger logger;

        public RawIqFileWriter(Stream output, DateTime timeStamp)
        {
            this.TimeStamp = timeStamp;
            this.output = output;
            this.input = new RawIqFile();
        }

        public RawIqFileWriter(Stream output, DateTime timeStamp, ILogger logger)
            : this(output, timeStamp)
        {
            this.logger = logger;
        }

        public DateTime TimeStamp { get; private set; }

        public DateTime CurrentMinDataTimeStamp { get; set; }

        public void WriteBlock(DataBlock block)
        {
            if (block.GetType() == typeof(SpectralIqDataBlock))
            {
                this.input.SpectralIqData.Add(block as SpectralIqDataBlock);
            }
            else if (block.GetType() == typeof(ConfigDataBlock))
            {
                this.input.Config = block as ConfigDataBlock;
            }
        }

        public void Close()
        {
            //Console.WriteLine("{0}|Data block samples count per file:{1}", DateTime.Now, this.input.SpectralIqData.Count);

            if (this.logger != null)
            {
                this.logger.Log(System.Diagnostics.TraceEventType.Information, LoggingMessageId.Scanner, string.Format("Snapshot count:{0}", this.input.SpectralIqData.Count));
            }

            Serializer.Serialize<RawIqFile>(this.output, this.input);
            this.output.Dispose();
            this.output = null;
        }
    }
}