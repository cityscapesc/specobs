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
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Runtime.InteropServices; 
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;    
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;
    using UFWM = Microsoft.Spectrum.IO.ScanFile.ScanFileWriterManager; // An alias so that we can meet style cop requirements of prepending statics with class name

    public delegate void DataBlockWrittenCallback(DataBlock dataBlock);

    /// <summary>
    /// This class manages the creation of binary data files (creating a new one at the top of every hour),
    /// and has a queue for receiving data from a scan to write to the file.
    /// It is a static, as we only want a singleton for this purpose.
    /// </summary>
    public static class ScanFileWriterManager
    {
        private static object queueLock = new object();
        private static Queue<DataBlock> dataBlockQueue = new Queue<DataBlock>();
        private static ManualResetEvent mreDataAvailable = new ManualResetEvent(false);
        private static ScanFileWriter fileWriter;
        private static bool fileWriterThreadRunning;
        private static string scanDirectory;
        private static CancellationToken cancellationToken;
        private static DataBlockWrittenCallback dataBlockWrittenCallback;
        private static TimeSpan minutesOfDataPerScanFile;
        private static string filePath;
        private static EventWaitHandle latch = new EventWaitHandle(false, EventResetMode.AutoReset);

        public static string HardwareInformation { get; set; }

        public static MeasurementStationConfigurationEndToEnd EndToEndConfiguration { get; set; }

        public static void Initialize(string scanDirectory, TimeSpan minutesOfDataPerScanFile, DataBlockWrittenCallback dataBlockWrittenCallback, CancellationToken cancellationToken)
        {
            UFWM.scanDirectory = scanDirectory;

            if (!Directory.Exists(scanDirectory))
            {
                Directory.CreateDirectory(scanDirectory);
            }

            UFWM.minutesOfDataPerScanFile = minutesOfDataPerScanFile;
            UFWM.dataBlockWrittenCallback = dataBlockWrittenCallback;

            UFWM.cancellationToken = cancellationToken;
            UFWM.cancellationToken.Register(UnblockDataWritingThread);

            Task.Factory.StartNew(UFWM.DataWritingThread, cancellationToken);

            // need to wait on this handle to make sure that the thread has started before we set the variable
            EventWaitHandle.WaitAny(new WaitHandle[] { latch });
            fileWriterThreadRunning = true;
        }

        public static void AddDataBlockToQueue(DataBlock dataBlock)
        {
            if (!fileWriterThreadRunning)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    // Ignore data passed in, we are shutting down
                    return;
                }

                throw new InvalidOperationException("The file writing thread is not running!  This indicates a fatal program error.  Check the event log for more details.");
            }

            lock (UFWM.queueLock)
            {
                dataBlockQueue.Enqueue(dataBlock);
                UFWM.mreDataAvailable.Set();
            }
        }

        public static void ForceFlush()
        {
            CloseFile(UFWM.fileWriter, UFWM.filePath);

            if (UFWM.fileWriter != null)
            {
                UFWM.fileWriter = null;
            }

            if (!string.IsNullOrEmpty(UFWM.filePath))
            {
                UFWM.filePath = null;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Protect the thread")]
        private static void DataWritingThread(object state)
        {
            latch.Set();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    UFWM.mreDataAvailable.WaitOne();

                    int count;

                    lock (UFWM.queueLock)
                    {
                        count = dataBlockQueue.Count;
                    }

                    while (count > 0)
                    {
                        DataBlock dataBlock;

                        lock (UFWM.queueLock)
                        {
                            dataBlock = dataBlockQueue.Dequeue();
                        }

                        CreateFileIfNeeded(dataBlock);

                        UFWM.fileWriter.WriteBlock(dataBlock);

                        dataBlockWrittenCallback(dataBlock);

                        lock (UFWM.queueLock)
                        {
                            count = dataBlockQueue.Count;

                            if (count == 0)
                            {
                                UFWM.mreDataAvailable.Reset();
                            }
                        }

                        // TODO: Turn into a performance counter
                        // Console.WriteLine("DataBlockQueue count: {0}", count);
                    }
                }

                CloseFile(UFWM.fileWriter, UFWM.filePath);
            }
            catch (Exception ex)
            {
                // TODO: Log failure to event log
                Console.WriteLine(ex);
            }
            finally
            {
                fileWriterThreadRunning = false;                
            }
        }

        /// <summary>
        /// In case the scanning thread has quit adding data to the queue, and we are trying to shut down.
        /// We don't want to be blocked at UFWM.mreDataAvailable.WaitOne();
        /// </summary>
        private static void UnblockDataWritingThread()
        {
            UFWM.mreDataAvailable.Set();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Target = "stream",
            Justification = "The stream is embedded in the ScanFileWriter.  The stream will be disposed when the ScanFileWriter is disposed.")]
        private static void CreateFileIfNeeded(DataBlock dataBlock)
        {
            // Create a new ScanFileWriter if we need one, based on the timestamp of the dataBlock
            if (dataBlock != null)
            {
                // Integer division to remove any remainder and get a clean, natural boundary.
                DateTime roundedTimeStamp = new DateTime((dataBlock.Timestamp.Ticks / minutesOfDataPerScanFile.Ticks) * minutesOfDataPerScanFile.Ticks);

                if (UFWM.fileWriter == null || roundedTimeStamp > UFWM.fileWriter.TimeStamp)
                {
                    ScanFileWriter tempFileWriter = UFWM.fileWriter;
                    string tempFilePath = UFWM.filePath;
                    Stream stream = CreateFile(roundedTimeStamp);
                    UFWM.fileWriter = new ScanFileWriter(stream, roundedTimeStamp);
                    Task.Factory.StartNew(() => UFWM.CloseFile(tempFileWriter, tempFilePath));                    

                    fileWriter.WriteBlock(new Microsoft.Spectrum.IO.ScanFile.ConfigDataBlock(UFWM.HardwareInformation, UFWM.EndToEndConfiguration));
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Target = "fs", Justification = "We are returning the file stream as an inner stream of the compressed stream")]
        private static Stream CreateFile(DateTime hourRoundedTimeStamp)
        {
            string sortableDateTime = hourRoundedTimeStamp.ToString("s", CultureInfo.InvariantCulture);
            sortableDateTime = sortableDateTime.Replace(":", null);
            sortableDateTime = sortableDateTime.Remove(sortableDateTime.Length - 2); // Remove seconds

            UFWM.filePath = Path.Combine(UFWM.scanDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.tmp", sortableDateTime));

            FileStream fs = File.Open(UFWM.filePath, FileMode.Create);
            DeflateStream compressedStream = new DeflateStream(fs, CompressionLevel.Optimal);

            return compressedStream;
        }        

        private static void CloseFile(ScanFileWriter fileWriter, string filePath)
        {
            if (fileWriter != null)
            {               
                fileWriter.Close();

                if (File.Exists(filePath))
                {
                    try
                    {
                        string destName = FileHelper.GetUniqueFileName(Path.ChangeExtension(filePath, ScanFile.Extension));

                        // Ensure that the target does not exist. 
                        if (File.Exists(destName))
                        {
                            File.Delete(destName);
                        }

                        File.Move(filePath, destName);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }
    }
}