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

namespace Microsoft.Spectrum.Scanning.Scanners
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Numerics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using FftwInterop;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;
    using Microsoft.Spectrum.IO.RawIqFile;
    using Microsoft.Spectrum.IO.ScanFile;
    using Newtonsoft.Json;

    public class Scanner : IScanner
    {
        private const int MaxErrorsInARow = 3;
        private const int ComplexWidth = 2;

        private ILogger logger;
        private MeasurementStationConfigurationEndToEnd endToEndConfiguration;
        private List<RFSensorConfigurationEndToEnd> sensorConfig;
        private RawIqDataConfigurationElement rawIqConfig;
        private ClientAggregationConfiguration aggregationConfiguration;
        private CancellationTokenSource cts;
        private Task scanThread;
        private double[] startFrequencies;
        private double[] currentStartFrequencies;
        private double[] stopFrequencies;
        private double[] bandwidths;
        private bool[] skipDeviceScan;
        private DateTime currentTimeStamp;
        private DateTime startTime;
        private List<IDevice> devices;
        private List<double[]> samples;
        private string hardwareInformation = string.Empty;
        private SettingsConfigurationSection settingsConfiguration;
        private DateTime settingsConfigurationReadTime;
        private DateTime currentRawIqDataBlockTimeStamp;
        private ICalibrationDataSource calibrationDataSource;

        //[NOTE:] For the purpose of debugging
        //private bool displayPsdOnTime = true;

        public Scanner(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;

            // Config loaded in ctor in order to fail early
            this.settingsConfiguration = (SettingsConfigurationSection)ConfigurationManager.GetSection("SettingsConfiguration");
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void StartScanning()
        {
            this.logger.Log(TraceEventType.Information, LoggingMessageId.ScanningStarting, "Scanner Starting");

            Action action = null;

            action = this.ScanThread;

            this.scanThread = Task.Factory.StartNew(action);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Protect the thread shutdown")]
        public void StopScanning()
        {
            this.logger.Log(TraceEventType.Information, LoggingMessageId.ScanningStopping, "Scanner Stopping");

            if (this.cts != null)
            {
                this.cts.Cancel();
            }

            try
            {
                this.scanThread.Wait();
                this.Dispose();
            }
            catch (Exception ex)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.Scanner, ex.ToString());
            }

            this.logger.Log(TraceEventType.Information, LoggingMessageId.ScanningStopped, "Scanner Stopped");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.cts != null)
                {
                    this.cts.Dispose();
                }

                if (this.scanThread != null)
                {
                    this.scanThread.Dispose();
                }
            }
        }

        private static string DumpDevices(List<IDevice> devices)
        {
            StringBuilder sb = new StringBuilder();

            foreach (IDevice device in devices)
            {
                sb.AppendLine(device.DumpDevice());
            }

            return sb.ToString();
        }

        private void InitializeFields()
        {
            int deviceCount = this.sensorConfig.Count;

            this.startFrequencies = new double[deviceCount];
            this.currentStartFrequencies = new double[deviceCount];
            this.stopFrequencies = new double[deviceCount];
            this.bandwidths = new double[deviceCount];
            this.skipDeviceScan = new bool[deviceCount];
            this.samples = new List<double[]>();

            this.currentTimeStamp = DateTime.MinValue;

            for (int i = 0; i < deviceCount; i++)
            {

                this.startFrequencies[i] = this.devices[i].StartFrequencyHz;
                this.currentStartFrequencies[i] = this.startFrequencies[i];
                this.stopFrequencies[i] = this.devices[i].StopFrequencyHz;
                this.skipDeviceScan[i] = false;

                if (!this.aggregationConfiguration.OutputData)
                {
                    // NOTE: Allowing RawIQ scan only per device frequency range (RawIQ frequency range can't overlap over multiple devices frequencies).
                    if (this.rawIqConfig.StartFrequencyHz >= this.devices[i].StartFrequencyHz
                        && this.rawIqConfig.StopFrequencyHz <= this.devices[i].StopFrequencyHz)
                    {
                        this.startFrequencies[i] = rawIqConfig.StartFrequencyHz;
                        this.currentStartFrequencies[i] = this.startFrequencies[i];
                        this.stopFrequencies[i] = rawIqConfig.StopFrequencyHz;
                    }
                    else
                    {
                        this.skipDeviceScan[i] = true;
                    }
                }

                this.bandwidths[i] = this.devices[i].BandwidthHz;

                if (this.devices[i].SamplesAsDb)
                {
                    this.samples.Add(new double[this.devices[i].SamplesPerScan]);
                }
                else
                {
                    this.samples.Add(new double[ComplexWidth * this.devices[i].SamplesPerScan]);
                }
            }

            // In the config data, make format everything into a well formatted structure so that we can easily compare things
            JsonSerializerSettings jss = new JsonSerializerSettings();
            jss.TypeNameHandling = TypeNameHandling.All;

            this.hardwareInformation = DumpDevices(this.devices);
        }

        private void InitializeAll()
        {
            if (this.cts != null)
            {
                this.cts.Cancel();
            }

            this.cts = new CancellationTokenSource();
            this.devices = new List<IDevice>();
            bool settingsFileComplete = false;
            bool tryDelete;

            while (!settingsFileComplete && !this.cts.IsCancellationRequested)
            {
                tryDelete = false;

                // Wait until the settings file is created before starting to scan. We get this from the server, or for debugging purposes we can create it on 
                // the client side.
                this.logger.Log(TraceEventType.Information, LoggingMessageId.Scanner, "Waiting for the scanner settings file before we start scanning.");
                while (!File.Exists(this.settingsConfiguration.MeasurementStationConfigurationFileFullPath) && !this.cts.Token.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                }

                this.logger.Log(TraceEventType.Information, LoggingMessageId.Scanner, "Scanner Configuration Found");

                // Read all settings
                FileHelper.WaitForFileWriteCompletion(this.settingsConfiguration.MeasurementStationConfigurationFileFullPath);

                try
                {
                    using (Stream input = File.OpenRead(this.settingsConfiguration.MeasurementStationConfigurationFileFullPath))
                    {
                        this.endToEndConfiguration = MeasurementStationConfigurationEndToEnd.Read(input);
                        settingsFileComplete = true;
                    }
                }
                catch
                {
                    // If the settings file is corrupt, then keep retrying until the file is ok, but wait 30 seconds before retrying
                    this.logger.Log(TraceEventType.Information, LoggingMessageId.Scanner, "Settings file is corrupt, retrying delete and reread of the file again.");
                    tryDelete = true;
                }

                if (tryDelete)
                {
                    try
                    {
                        File.Delete(this.settingsConfiguration.MeasurementStationConfigurationFileFullPath);
                        Thread.Sleep(30000);
                    }
                    catch
                    {
                    }
                }
            }

            this.sensorConfig = this.endToEndConfiguration.RFSensorConfigurations;
            this.rawIqConfig = this.endToEndConfiguration.RawIqConfiguration;
            this.aggregationConfiguration = this.endToEndConfiguration.AggregationConfiguration;
            this.settingsConfigurationReadTime = DateTime.Now;

            // We can only handle exact bandwidth buckets, so we will automatically adjust to the closest value we can allow.
            foreach (RFSensorConfigurationEndToEnd device in this.sensorConfig)
            {
                if (device.CurrentStartFrequencyHz >= device.CurrentStopFrequencyHz)
                {
                    throw new ConfigurationErrorsException("The start frequency must be less than the stop frequency");
                }

                double frequencyDifference = device.CurrentStopFrequencyHz - device.CurrentStartFrequencyHz;
                device.CurrentStopFrequencyHz = device.CurrentStartFrequencyHz + (device.BandwidthHz * Math.Truncate(frequencyDifference / device.BandwidthHz));

                this.logger.Log(TraceEventType.Warning, LoggingMessageId.Scanner, string.Format(CultureInfo.InvariantCulture, "The configuration values for start and stop frequency were automatically adjusted to {0} - {1}", device.CurrentStartFrequencyHz, device.CurrentStopFrequencyHz));
            }

            // wait 2 seconds before tryingo to restart the file writer threads and trying to restart the RF sensors
            Thread.Sleep(2000);

            if (this.rawIqConfig.OutputData)
            {
                //RawIqFileWriterManager.SetLogger(this.logger);

                RawIqFileWriterManager.Initialize(
                    Environment.ExpandEnvironmentVariables(this.settingsConfiguration.OutputDirectory),
                    TimeSpan.FromSeconds(this.rawIqConfig.SecondsOfDataPerFile),
                    TimeSpan.FromMilliseconds(this.rawIqConfig.DutycycleOnTimeInMilliSec),
                    TimeSpan.FromMilliseconds(this.rawIqConfig.DutycycleTimeInMilliSec),
                    TimeSpan.FromSeconds(this.rawIqConfig.RetentionSeconds),
                    this.cts.Token);
            }

            if (this.aggregationConfiguration.OutputData
                || (this.rawIqConfig.OutputData
                    && this.rawIqConfig.OuputPSDDataInDutyCycleOffTime))
            {
                ScanFileWriterManager.Initialize(Environment.ExpandEnvironmentVariables(this.settingsConfiguration.OutputDirectory), this.aggregationConfiguration.MinutesOfDataPerScanFile, this.DataBlockWrittenHandler, this.cts.Token);
            }

            this.devices.Clear();
            foreach (RFSensorConfigurationEndToEnd dce in this.sensorConfig)
            {
                IDevice newDevice;
                if (dce.DeviceType == DeviceType.USRP.ToString())
                {
                    calibrationDataSource = new CsvCalibrationDataSource(this.settingsConfiguration.CityscapeCalibrationDataFileFullPath, this.logger);
                    newDevice = new UsrpDevice(calibrationDataSource, this.logger);
                }
                else if (dce.DeviceType == DeviceType.RFExplorer.ToString())
                {
                    newDevice = new RFExplorerDevice(this.logger);
                }
                else
                {
                    throw new InvalidOperationException("DeviceType specified in configuration file is not a supported DeviceType");
                }

                if (newDevice != null)
                {
                    newDevice.ConfigureDevice(dce);
                    this.devices.Add(newDevice);
                }
            }

            // Allow some time for device setup
            Thread.Sleep(1000);

            this.InitializeFields();

            RawIqFileWriterManager.HardwareInformation = this.hardwareInformation;
            RawIqFileWriterManager.EndToEndConfiguration = this.endToEndConfiguration;
            ScanFileWriterManager.HardwareInformation = this.hardwareInformation;
            ScanFileWriterManager.EndToEndConfiguration = this.endToEndConfiguration;
        }

        private void ScanThread()
        {
            this.logger.Log(TraceEventType.Information, LoggingMessageId.ScanningStarted, "Scanner Started");

            int outerErrorsInARow = 0;

            do
            {
                if (outerErrorsInARow > 0)
                {
                    this.logger.Log(TraceEventType.Information, LoggingMessageId.Scanner, "Reconnecting with devices due to errors in inner loop.");
                }

                this.InitializeAll();

                try
                {
                    this.logger.Log(TraceEventType.Information, LoggingMessageId.ScanningConfig, this.hardwareInformation);

                    this.InnerScanLoop(ref outerErrorsInARow);
                }
                finally
                {
                    foreach (IDevice device in this.devices)
                    {
                        device.Dispose();
                    }

                    this.devices.Clear();
                }

                if (++outerErrorsInARow == MaxErrorsInARow)
                {
                    this.logger.Log(TraceEventType.Critical, LoggingMessageId.ScanningError, string.Format(CultureInfo.InvariantCulture, "Terminating the scan thread due to {0} 'outer' errors in a row", MaxErrorsInARow));
                    this.cts.Cancel();
                }
            }
            while (!this.cts.Token.IsCancellationRequested);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Protect the thread")]
        private void InnerScanLoop(ref int outerErrorsInARow)
        {
            int innerErrorsInARow = 0;

            // this is where we want to add different scan types in (Standard, DCSpike...)                            
            // TODO: Should make this multithreaded
            while (!this.cts.Token.IsCancellationRequested)
            {
                try
                {
                    if (this.aggregationConfiguration.OutputData
                        || (this.rawIqConfig.OutputData && this.rawIqConfig.OuputPSDDataInDutyCycleOffTime))
                    {
                        this.BeginningOfFullScan();
                    }

                    for (int i = 0; i < this.devices.Count; i++)
                    {
                        switch ((ScanTypes)Enum.Parse(typeof(ScanTypes), this.sensorConfig[i].ScanPattern, true))
                        {
                            case ScanTypes.DCSpikeAdaptiveScan:
                                {
                                    this.DCSpikeAdaptiveScan(this.devices[i], this.samples[i], i);
                                    break;
                                }

                            case ScanTypes.StandardScan:
                            default:
                                {
                                    this.StandardScan(this.devices[i], this.samples[i], i);
                                    break;
                                }
                        }
                    }

                    this.EndOfFullScan();

                    innerErrorsInARow = 0;
                    outerErrorsInARow = 0;
                }
                catch (ScannerSettingsChangedException ex)
                {
                    this.logger.Log(TraceEventType.Information, LoggingMessageId.Scanner, string.Format(CultureInfo.InvariantCulture, "Settings have changed, resetting the scanner to use the new settings {0}", ex.Message));

                    RawIqFileWriterManager.ForceFlush();
                    ScanFileWriterManager.ForceFlush();

                    this.logger.Log(TraceEventType.Information, LoggingMessageId.Scanner, string.Format("Forced flushed data for old settings"));

                    break;
                }
                catch (Exception ex)
                {
                    this.logger.Log(TraceEventType.Error, LoggingMessageId.ScanningError, ex.ToString());

                    if (++innerErrorsInARow == MaxErrorsInARow)
                    {
                        this.logger.Log(TraceEventType.Warning, LoggingMessageId.ScanningError, string.Format(CultureInfo.InvariantCulture, "Received {0} 'inner' errors in a row", MaxErrorsInARow));
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// With some devices we have a spike representing the DC at the center frequency that we need to filter out
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="device"></param>
        /// <param name="streamer"></param>
        /// <param name="samples"></param>
        /// <param name="deviceIndex"></param>
        private void DCSpikeAdaptiveScan(IDevice device, double[] currentSamples, int deviceIndex)
        {
            do
            {
                for (int j = 0; j < this.sensorConfig[deviceIndex].NumberOfSampleBlocksPerScan; j++)
                {
                    if (this.skipDeviceScan[deviceIndex])
                    {
                        break;
                    }

                    double LowerTuneFreq = this.currentStartFrequencies[deviceIndex] - (this.bandwidths[deviceIndex] * 0.15);
                    double UpperTuneFreq = this.currentStartFrequencies[deviceIndex] + (this.bandwidths[deviceIndex] * 0.15);

                    device.TuneToFrequency(LowerTuneFreq);

                    for (int throwAwayBlocks = 0; throwAwayBlocks < this.sensorConfig[deviceIndex].NumberOfSampleBlocksToThrowAway; throwAwayBlocks++)
                    {
                        device.ReceiveSamples(currentSamples);
                    }

                    device.ReceiveSamples(currentSamples);

                    Complex[] fftDataFirst = null;

                    if (this.aggregationConfiguration.OutputData
                        || (this.rawIqConfig.OutputData && this.rawIqConfig.OuputPSDDataInDutyCycleOffTime))
                    {
                        fftDataFirst = device.PerformFFT(currentSamples);
                    }

                    if (this.rawIqConfig.OutputData && device.RawIqDataAvailable)
                    {
                        if ((LowerTuneFreq >= this.rawIqConfig.StartFrequencyHz &&
                            LowerTuneFreq <= this.rawIqConfig.StopFrequencyHz) ||
                            (LowerTuneFreq + this.bandwidths[deviceIndex] >= this.rawIqConfig.StartFrequencyHz &&
                            LowerTuneFreq + this.bandwidths[deviceIndex] <= this.rawIqConfig.StopFrequencyHz))
                        {
                            string gpsLocation = device.NmeaGpggaLocation;
                            currentRawIqDataBlockTimeStamp = DateTime.UtcNow;

                            RawIqFileWriterManager.AddDataBlockToQueue(
                                new SpectralIqDataBlock(
                                    currentRawIqDataBlockTimeStamp,
                                    LowerTuneFreq,
                                    LowerTuneFreq + this.bandwidths[deviceIndex],
                                    LowerTuneFreq + (this.bandwidths[deviceIndex] / 2),
                                    (double[])currentSamples.Clone(),
                                    gpsLocation));
                        }
                    }

                    device.TuneToFrequency(UpperTuneFreq);

                    for (int throwAwayBlocks = 0; throwAwayBlocks < this.sensorConfig[deviceIndex].NumberOfSampleBlocksToThrowAway; throwAwayBlocks++)
                    {
                        device.ReceiveSamples(currentSamples);
                    }

                    device.ReceiveSamples(currentSamples);

                    Complex[] fftDataSecond = null;
                    if (this.aggregationConfiguration.OutputData
                        || (this.rawIqConfig.OutputData && this.rawIqConfig.OuputPSDDataInDutyCycleOffTime))
                    {
                        fftDataSecond = device.PerformFFT(currentSamples);
                    }

                    if (this.rawIqConfig.OutputData && device.RawIqDataAvailable)
                    {
                        if ((UpperTuneFreq >= this.rawIqConfig.StartFrequencyHz &&
                            UpperTuneFreq <= this.rawIqConfig.StopFrequencyHz) ||
                            (UpperTuneFreq + this.bandwidths[deviceIndex] >= this.rawIqConfig.StartFrequencyHz &&
                            UpperTuneFreq + this.bandwidths[deviceIndex] <= this.rawIqConfig.StopFrequencyHz))
                        {
                            string gpsLocation = device.NmeaGpggaLocation;
                            currentRawIqDataBlockTimeStamp = DateTime.UtcNow;

                            RawIqFileWriterManager.AddDataBlockToQueue(
                                new SpectralIqDataBlock(
                                    currentRawIqDataBlockTimeStamp,
                                    UpperTuneFreq,
                                    UpperTuneFreq + this.bandwidths[deviceIndex],
                                    UpperTuneFreq  + (this.bandwidths[deviceIndex] / 2) ,
                                    (double[])currentSamples.Clone(),
                                    gpsLocation));
                        }
                    }

                    if ((this.aggregationConfiguration.OutputData
                        || (this.rawIqConfig.OutputData
                            && this.rawIqConfig.OuputPSDDataInDutyCycleOffTime
                            && currentRawIqDataBlockTimeStamp.ToUniversalTime().Ticks >= RawIqFileWriterManager.CurrentDutyCycleOffStartTime.Ticks
                            && currentRawIqDataBlockTimeStamp.ToUniversalTime().Ticks < RawIqFileWriterManager.NextDutyCycleOnTimeStamp.Ticks))
                        && fftDataFirst != null
                        && fftDataSecond != null)
                    {
                        //if (displayPsdOnTime)
                        //{
                        //    Console.WriteLine("DCSpikeScan | PSD Data ON Timestamp:{0}", currentRawIqDataBlockTimeStamp.ToString("yyyy-MM-dd hh:mm:ss.fff"));
                        //    displayPsdOnTime = false;
                        //}

                        device.Fvp.ProcessDataDCSpikeScan(fftDataFirst, fftDataSecond, device.InstantPowerStartIndex(this.currentStartFrequencies[deviceIndex]));
                    }
                    //else if (this.rawIqConfig.OutputData
                    //        && this.rawIqConfig.OuputPSDDataInDutyCycleOffTime)
                    //{
                    //    if (!displayPsdOnTime)
                    //    {
                    //        displayPsdOnTime = true;
                    //        Console.WriteLine("DCSpikeScan | PSD Data OFF Timestamp:{0}", currentRawIqDataBlockTimeStamp.ToString("yyyy-MM-dd hh:mm:ss.fff"));
                    //    }
                    //}
                }

                this.NextFrequencies(deviceIndex);
            }
            while (this.currentStartFrequencies[deviceIndex] != this.startFrequencies[deviceIndex]);
        }

        private void StandardScan(IDevice device, double[] currentSamples, int deviceIndex)
        {
            if (this.aggregationConfiguration.OutputData
                || (this.rawIqConfig.OutputData
                && !this.rawIqConfig.OuputPSDDataInDutyCycleOffTime))
            {
                do
                {
                    if (this.skipDeviceScan[deviceIndex])
                    {
                        break;
                    }

                    device.TuneToFrequency(this.currentStartFrequencies[deviceIndex]);

                    for (int throwAwayBlocks = 0; throwAwayBlocks < this.sensorConfig[deviceIndex].NumberOfSampleBlocksToThrowAway; throwAwayBlocks++)
                    {
                        device.ReceiveSamples(currentSamples);
                    }

                    for (int j = 0; j < this.sensorConfig[deviceIndex].NumberOfSampleBlocksPerScan; j++)
                    {
                        device.ReceiveSamples(currentSamples);

                        if (device.SamplesAsDb)
                        {
                            device.Fvp.ProcessDbData(currentSamples, device.InstantPowerStartIndex(this.currentStartFrequencies[deviceIndex]));
                        }
                        else if (this.aggregationConfiguration.OutputData
                                 || (this.rawIqConfig.OutputData
                                     && this.rawIqConfig.OuputPSDDataInDutyCycleOffTime
                                     && currentRawIqDataBlockTimeStamp.ToUniversalTime().Ticks >= RawIqFileWriterManager.CurrentDutyCycleOffStartTime.Ticks
                                     && currentRawIqDataBlockTimeStamp.ToUniversalTime().Ticks < RawIqFileWriterManager.NextDutyCycleOnTimeStamp.Ticks))
                        {
                            //if (displayPsdOnTime)
                            //{
                            //    Console.WriteLine("StandardScan | PSD Data ON Timestamp:{0}", currentRawIqDataBlockTimeStamp.ToString("yyyy-MM-dd hh:mm:ss.fff"));
                            //    displayPsdOnTime = false;
                            //}


                            Complex[] fftData = device.PerformFFT(currentSamples);

                            device.Fvp.ProcessData(fftData, device.InstantPowerStartIndex(this.currentStartFrequencies[deviceIndex]));
                        }
                        //else if (this.rawIqConfig.OutputData
                        //        && this.rawIqConfig.OuputPSDDataInDutyCycleOffTime)
                        //{
                        //    if (!displayPsdOnTime)
                        //    {
                        //        displayPsdOnTime = true;
                        //        Console.WriteLine("StandardScan | PSD Data OFF Timestamp:{0}", currentRawIqDataBlockTimeStamp.ToString("yyyy-MM-dd hh:mm:ss.fff"));
                        //    }
                        //}

                        if (this.rawIqConfig.OutputData && device.RawIqDataAvailable)
                        {
                            if ((this.currentStartFrequencies[deviceIndex] >= this.rawIqConfig.StartFrequencyHz &&
                                this.currentStartFrequencies[deviceIndex] <= this.rawIqConfig.StopFrequencyHz) ||
                                (this.currentStartFrequencies[deviceIndex] + this.bandwidths[deviceIndex] >= this.rawIqConfig.StartFrequencyHz &&
                                this.currentStartFrequencies[deviceIndex] + this.bandwidths[deviceIndex] <= this.rawIqConfig.StopFrequencyHz))
                            {
                                string gpsLocation = device.NmeaGpggaLocation;
                                currentRawIqDataBlockTimeStamp = DateTime.UtcNow;

                                RawIqFileWriterManager.AddDataBlockToQueue(
                                    new SpectralIqDataBlock(
                                        currentRawIqDataBlockTimeStamp,
                                        this.currentStartFrequencies[deviceIndex],
                                        this.currentStartFrequencies[deviceIndex] + this.bandwidths[deviceIndex],
                                        this.currentStartFrequencies[deviceIndex] + (this.bandwidths[deviceIndex] / 2),
                                        (double[])currentSamples.Clone(),
                                        gpsLocation));
                            }
                        }
                    }

                    this.NextFrequencies(deviceIndex);
                }
                while (this.currentStartFrequencies[deviceIndex] != this.startFrequencies[deviceIndex]);
            }
            else
            {
                this.Scan_RawIQByStandardScan_And_PsdByDCSpikeScan(device, currentSamples, deviceIndex);
            }
        }

        private void Scan_RawIQByStandardScan_And_PsdByDCSpikeScan(IDevice device, double[] currentSamples, int deviceIndex)
        {
            do
            {
                for (int j = 0; j < this.sensorConfig[deviceIndex].NumberOfSampleBlocksPerScan; j++)
                {
                    if (this.skipDeviceScan[deviceIndex])
                    {
                        break;
                    }

                    double LowerTuneFreq = this.currentStartFrequencies[deviceIndex] - (this.bandwidths[deviceIndex] * 0.15);
                    double UpperTuneFreq = this.currentStartFrequencies[deviceIndex] + (this.bandwidths[deviceIndex] * 0.15);

                    device.TuneToFrequency(LowerTuneFreq);

                    for (int throwAwayBlocks = 0; throwAwayBlocks < this.sensorConfig[deviceIndex].NumberOfSampleBlocksToThrowAway; throwAwayBlocks++)
                    {
                        device.ReceiveSamples(currentSamples);
                    }

                    device.ReceiveSamples(currentSamples);

                    Complex[] fftDataFirst = null;

                    fftDataFirst = device.PerformFFT(currentSamples);

                    if (device.RawIqDataAvailable)
                    {
                        if ((LowerTuneFreq >= this.rawIqConfig.StartFrequencyHz &&
                            LowerTuneFreq <= this.rawIqConfig.StopFrequencyHz) ||
                            (LowerTuneFreq + this.bandwidths[deviceIndex] >= this.rawIqConfig.StartFrequencyHz &&
                            LowerTuneFreq + this.bandwidths[deviceIndex] <= this.rawIqConfig.StopFrequencyHz))
                        {
                            string gpsLocation = device.NmeaGpggaLocation;
                            currentRawIqDataBlockTimeStamp = DateTime.UtcNow;

                            RawIqFileWriterManager.AddDataBlockToQueue(
                                new SpectralIqDataBlock(
                                    currentRawIqDataBlockTimeStamp,
                                    LowerTuneFreq,
                                    LowerTuneFreq + this.bandwidths[deviceIndex],
                                    LowerTuneFreq + (this.bandwidths[deviceIndex] / 2),
                                    (double[])currentSamples.Clone(),
                                    gpsLocation));
                        }
                    }

                    device.TuneToFrequency(UpperTuneFreq);

                    for (int throwAwayBlocks = 0; throwAwayBlocks < this.sensorConfig[deviceIndex].NumberOfSampleBlocksToThrowAway; throwAwayBlocks++)
                    {
                        device.ReceiveSamples(currentSamples);
                    }

                    device.ReceiveSamples(currentSamples);

                    Complex[] fftDataSecond = null;

                    fftDataSecond = device.PerformFFT(currentSamples);

                    if (currentRawIqDataBlockTimeStamp.ToUniversalTime().Ticks >= RawIqFileWriterManager.CurrentDutyCycleOffStartTime.Ticks
                        && currentRawIqDataBlockTimeStamp.ToUniversalTime().Ticks < RawIqFileWriterManager.NextDutyCycleOnTimeStamp.Ticks
                        && fftDataFirst != null
                        && fftDataSecond != null)
                    {
                        //if (displayPsdOnTime)
                        //{
                        //    Console.WriteLine("DCSpikeScan | PSD Data ON Timestamp:{0}", currentRawIqDataBlockTimeStamp.ToString("yyyy-MM-dd hh:mm:ss.fff"));
                        //    displayPsdOnTime = false;
                        //}

                        device.Fvp.ProcessDataDCSpikeScan(fftDataFirst, fftDataSecond, device.InstantPowerStartIndex(this.currentStartFrequencies[deviceIndex]));
                    }
                    //else if (this.rawIqConfig.OutputData
                    //        && this.rawIqConfig.OuputPSDDataInDutyCycleOffTime)
                    //{
                    //    if (!displayPsdOnTime)
                    //    {
                    //        displayPsdOnTime = true;
                    //        Console.WriteLine("DCSpikeScan | PSD Data OFF Timestamp:{0}", currentRawIqDataBlockTimeStamp.ToString("yyyy-MM-dd hh:mm:ss.fff"));
                    //    }
                    //}
                }

                this.NextFrequencies(deviceIndex);
            }
            while (this.currentStartFrequencies[deviceIndex] != this.startFrequencies[deviceIndex]);
        }

        private void BeginningOfFullScan()
        {
            this.startTime = DateTime.Now;

            if (this.currentTimeStamp == DateTime.MinValue)
            {
                this.currentTimeStamp = new DateTime((this.startTime.ToUniversalTime().Ticks / this.aggregationConfiguration.SecondsOfDataPerSample.Ticks) * this.aggregationConfiguration.SecondsOfDataPerSample.Ticks, DateTimeKind.Utc);
            }
        }

        private void EndOfFullScan()
        {
            DateTime roundedTimeStamp = new DateTime((this.startTime.ToUniversalTime().Ticks / this.aggregationConfiguration.SecondsOfDataPerSample.Ticks) * this.aggregationConfiguration.SecondsOfDataPerSample.Ticks, DateTimeKind.Utc);

            if ((this.aggregationConfiguration.OutputData
                || (this.rawIqConfig.OutputData && this.rawIqConfig.OuputPSDDataInDutyCycleOffTime))
                && (roundedTimeStamp > this.currentTimeStamp || this.aggregationConfiguration.SingleScan))
            {
                for (int i = 0; i < this.devices.Count; i++)
                {
                    string gpsLocation = this.devices[i].NmeaGpggaLocation;

                    foreach (var item in this.devices[i].Fvp.GetResults())
                    {
                        /* Console output to see what is being written to the scanfiles */
                        //Console.WriteLine(
                        //    "Adding SPD - Device Id {0} - Start Freq {1} - Stop Freq {2} - ReadingKind {3} - DataSize {4}",
                        //    i,
                        //    this.devices[i].StartFrequencyHz,
                        //    this.devices[i].StopFrequencyHz,
                        //    item.ReadingKind.ToString(),
                        //    item.Data.Length);


                        ScanFileWriterManager.AddDataBlockToQueue(new SpectralPsdDataBlock(this.currentTimeStamp, this.sensorConfig[i].CurrentStartFrequencyHz, this.sensorConfig[i].CurrentStopFrequencyHz, item.ReadingKind, item.Data, i, gpsLocation));
                    }

                    this.currentTimeStamp = roundedTimeStamp;
                }
            }

            // Use Console, so as not to fill event log
            //Console.WriteLine("Time for full scan: {0} ms", (DateTime.Now - this.startTime).TotalMilliseconds);

            // Use this to break out of the scan loop after 1 iteration
            // e.g. for timing how long a full scan takes
            if (this.aggregationConfiguration.SingleScan)
            {
                this.cts.Cancel();
            }

            // Check for a change in the settings that may have been pushed down
            if (File.Exists(this.settingsConfiguration.MeasurementStationConfigurationFileFullPath))
            {
                if (File.GetLastWriteTime(this.settingsConfiguration.MeasurementStationConfigurationFileFullPath) > this.settingsConfigurationReadTime)
                {
                    // Load the settings from the settings file and then throw the settings changed exception to reset the scanner                    
                    throw new ScannerSettingsChangedException();
                }
            }

            // Check for a change in calibration settings that may have been pushed down
            if (this.calibrationDataSource != null
                && this.calibrationDataSource.CalibrationUpdated())
            {
                // Load the settings from the settings file and then throw the settings changed exception to reset the scanner  
                throw new ScannerSettingsChangedException(string.Format("{0}, Calibration setting file has updated", this.settingsConfiguration.CityscapeCalibrationDataFileFullPath));
            }
        }

        /// <summary>
        /// We only advance bandwidth - a configurable overlap, so that we have overlap between chunks in order to avoid gaps / seams
        /// </summary>
        private void NextFrequencies(int deviceIndex)
        {
            this.currentStartFrequencies[deviceIndex] += this.bandwidths[deviceIndex];

            if (this.currentStartFrequencies[deviceIndex] >= this.stopFrequencies[deviceIndex])
            {
                this.currentStartFrequencies[deviceIndex] = this.startFrequencies[deviceIndex];
            }
        }

        private void DataBlockWrittenHandler(Microsoft.Spectrum.IO.ScanFile.DataBlock datablock)
        {
            SpectralPsdDataBlock sdb = datablock as SpectralPsdDataBlock;
            if (sdb != null)
            {
                this.devices[sdb.DeviceId].Fvp.ReturnItemToPool(sdb.DataPoints);
            }
        }
    }
}
