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
    using RFWM = Microsoft.Spectrum.IO.RawIqFile.RawIqFileWriterManager; // An alias so that we can meet style cop requirements of prepending statics with class name

    /// <summary>
    /// This class manages the creation of binary data files (creating a new one at the top of every hour),
    /// and has a queue for receiving data from a scan to write to the file.
    /// It is a static, as we only want a singleton for this purpose.
    /// </summary>
    public static class RawIqFileWriterManager
    {
        private static object queueLock = new object();
        private static Queue<DataBlock> dataBlockQueue = new Queue<DataBlock>();
        private static ManualResetEvent mreDataAvailable = new ManualResetEvent(false);
        private static RawIqFileWriter fileWriter;
        private static bool fileWriterThreadRunning;
        private static string rawiqDirectory;
        private static CancellationToken cancellationToken;
        private static TimeSpan totalSecondsOfDataPerRawIqFile;
        private static TimeSpan retentionTime;
        private static string filePath;
        private static TimeSpan dutycyleOnTimeInMilliSec;
        private static TimeSpan dutyCycleTimeStamp;
        private static bool startOfScanner = true;
        private static DateTime nextDutyCycleOnTimeStamp;
        private static DateTime currentDutyCycleOnTimeStamp;

        //[NOTE:] For the purpose of debugging
        //private static bool displayOfftime = true;

        private static ILogger filelogger;

        public static DateTime CurrentDutyCycleOffStartTime
        {
            get
            {
                return currentDutyCycleOnTimeStamp.AddTicks(dutycyleOnTimeInMilliSec.Ticks);
            }
        }

        public static DateTime NextDutyCycleOnTimeStamp
        {
            get
            {
                return nextDutyCycleOnTimeStamp;
            }
        }

        public static string HardwareInformation { get; set; }

        public static MeasurementStationConfigurationEndToEnd EndToEndConfiguration { get; set; }


        public static void SetLogger(ILogger logger)
        {
            filelogger = logger;
        }

        public static void Initialize(string rawiqDirectory, TimeSpan totalSecondsOfDataPerRawIqFile, TimeSpan dutycycleOnTimeInMilliSec, TimeSpan dutycycleTimeInMilliSec, TimeSpan retentionTime, CancellationToken cancellationToken)
        {
            RFWM.rawiqDirectory = rawiqDirectory;

            if (!Directory.Exists(rawiqDirectory))
            {
                Directory.CreateDirectory(rawiqDirectory);
            }

            RFWM.totalSecondsOfDataPerRawIqFile = totalSecondsOfDataPerRawIqFile;
            RFWM.retentionTime = retentionTime;
            RFWM.dutycyleOnTimeInMilliSec = dutycycleOnTimeInMilliSec;
            RFWM.dutyCycleTimeStamp = dutycycleTimeInMilliSec;
            RFWM.cancellationToken = cancellationToken;
            RFWM.cancellationToken.Register(UnblockDataWritingThread);

            Task.Factory.StartNew(RFWM.DataWritingThread, cancellationToken);
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

            lock (RFWM.queueLock)
            {
                dataBlockQueue.Enqueue(dataBlock);
                RFWM.mreDataAvailable.Set();
            }
        }

        public static void ForceFlush()
        {
            CloseFile(RFWM.fileWriter, RFWM.filePath);

            if (RFWM.fileWriter != null)
            {
                RFWM.fileWriter = null;
            }

            if (!string.IsNullOrEmpty(RFWM.filePath))
            {
                RFWM.filePath = null;
            }
        }             

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Protect the thread")]
        private static void DataWritingThread(object state)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    RFWM.mreDataAvailable.WaitOne();

                    int count;

                    lock (RFWM.queueLock)
                    {
                        count = dataBlockQueue.Count;
                    }

                    while (count > 0)
                    {
                        DataBlock dataBlock;

                        lock (RFWM.queueLock)
                        {
                            dataBlock = dataBlockQueue.Dequeue();
                        }

                        CreateFileIfNeeded(dataBlock);                     

                        if (dataBlock.Timestamp.Ticks <= currentDutyCycleOnTimeStamp.AddTicks(dutycyleOnTimeInMilliSec.Ticks).Ticks)
                        {
                            RFWM.fileWriter.WriteBlock(dataBlock);                           
                        }
                        //else
                        //{
                        //    if (displayOfftime)
                        //    {
                        //        Console.WriteLine("RawIQ OFF time start:{0}", dataBlock.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff"));
                        //        displayOfftime = false;
                        //    }
                        //}

                        lock (RFWM.queueLock)
                        {
                            count = dataBlockQueue.Count;

                            if (count == 0)
                            {
                                RFWM.mreDataAvailable.Reset();
                            }
                        }

                        // TODO: Turn into a performance counter
                        // Console.WriteLine("DataBlockQueue count: {0}", count);
                    }
                }

                RFWM.CloseFile(RFWM.fileWriter, RFWM.filePath);
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
            RFWM.mreDataAvailable.Set();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Target = "stream",
            Justification = "The stream is embedded in the ScanFileWriter.  The stream will be disposed when the ScanFileWriter is disposed.")]
        private static void CreateFileIfNeeded(DataBlock dataBlock)
        {
            // Create a new ScanFileWriter if we need one, based on the timestamp of the dataBlock
            if (dataBlock != null)
            {
                // Integer division to remove any remainder and get a clean, natural boundary.
                DateTime roundedTimeStamp = new DateTime((dataBlock.Timestamp.Ticks / totalSecondsOfDataPerRawIqFile.Ticks) * totalSecondsOfDataPerRawIqFile.Ticks);
                DateTime minuteRoundOfTime = new DateTime((dataBlock.Timestamp.Ticks / dutyCycleTimeStamp.Ticks) * dutyCycleTimeStamp.Ticks);

                if (RFWM.fileWriter == null || roundedTimeStamp > RFWM.fileWriter.TimeStamp)
                {
                    RawIqFileWriter tempFileWriter = RFWM.fileWriter;
                    string tempFilePath = RFWM.filePath;
                    Stream stream = CreateFile(roundedTimeStamp);

                    if (RFWM.fileWriter == null && startOfScanner)
                    {
                        currentDutyCycleOnTimeStamp = dataBlock.Timestamp;
                        nextDutyCycleOnTimeStamp = currentDutyCycleOnTimeStamp.AddTicks(dutyCycleTimeStamp.Ticks);                        

                        //Console.WriteLine("RawIQ ON time starts now|Start|{0}", dataBlock.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff"));
                        startOfScanner = false;
                    }

                    RFWM.fileWriter = new RawIqFileWriter(stream, roundedTimeStamp);

                    Task.Factory.StartNew(() => RFWM.CloseFile(tempFileWriter, tempFilePath));

                    fileWriter.WriteBlock(new Microsoft.Spectrum.IO.RawIqFile.ConfigDataBlock(RFWM.HardwareInformation, RFWM.EndToEndConfiguration));
                }

                if (dataBlock.Timestamp.Ticks > nextDutyCycleOnTimeStamp.Ticks)
                {
                    currentDutyCycleOnTimeStamp = nextDutyCycleOnTimeStamp;

                    // [NOTE:]If we have to wait many duty cycles to receive a DataBlock due high frequency tune delay set in TuneToFrequency method of  UsrpDevice class, 
                    // then it also means that we don't receive samples in every duty cycle and rate at which we receive data will by determined by tune delay and we have to keep up dutycycle time
                    // with the delay. As a temporary fix in that case we consider all the incoming data.
                    // [TODO:] RawIQFileWriterManger class has to have context about frequency tune delay.
                    var dutyCyclePerDataBlock = (dataBlock.Timestamp.Ticks - nextDutyCycleOnTimeStamp.Ticks) / dutyCycleTimeStamp.Ticks;

                    if(dutyCyclePerDataBlock > 1)
                    {
                        currentDutyCycleOnTimeStamp = dataBlock.Timestamp;
                    }

                    nextDutyCycleOnTimeStamp = currentDutyCycleOnTimeStamp.AddTicks(dutyCycleTimeStamp.Ticks);

                    //Console.WriteLine("RawIQ ON time starts now|Running|{0}, Current DataBlock timestamp :{1}", currentDutyCycleOnTimeStamp.ToString("yyyy-MM-dd hh:mm:ss.fff"),dataBlock.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff"));
                    //displayOfftime = true;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Target = "fs", Justification = "We are returning the file stream as an inner stream of the compressed stream")]
        private static Stream CreateFile(DateTime hourRoundedTimeStamp)
        {
            string sortableDateTime = hourRoundedTimeStamp.ToString("s", CultureInfo.InvariantCulture);
            sortableDateTime = sortableDateTime.Replace(":", null);

            RFWM.filePath = Path.Combine(RFWM.rawiqDirectory, string.Format(CultureInfo.InvariantCulture, "{0}.bin.tmp", sortableDateTime));

            FileStream fs = File.Open(RFWM.filePath, FileMode.Create);
            DeflateStream compressedStream = new DeflateStream(fs, CompressionLevel.Optimal);

            return compressedStream;
        }

        private static void CloseFile(RawIqFileWriter fileWriter, string filePath)
        {
            if (fileWriter != null)
            {
                fileWriter.Close();

                if (File.Exists(filePath))
                {
                    try
                    {
                        string destName = FileHelper.GetUniqueFileName(Path.ChangeExtension(filePath, RawIqFile.Extension));

                        // Ensure that the target does not exist. 
                        if (File.Exists(destName))
                        {
                            File.Delete(destName);
                        }

                        File.Move(filePath, destName);

                        // when we write a new file, check to see if there are any old files we should delete
                        // that fail the retention policy
                        string[] files = Directory.GetFiles(RFWM.rawiqDirectory, "*" + RawIqFile.Extension);

                        foreach (string file in files)
                        {
                            DateTime writeTime = File.GetLastWriteTime(file);
                            DateTime retentionTime = DateTime.Now.Subtract(RFWM.retentionTime);

                            if (retentionTime > writeTime)
                            {
                                try
                                {
                                    File.Delete(file);
                                }
                                catch (IOException ex)
                                {
                                    Console.WriteLine(ex.ToString());
                                }
                            }
                        }
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