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
    using System.Numerics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using FftwInterop;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Devices.Usrp;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;

    public class UsrpDevice : IDevice
    {
        private const int ComplexWidth = 2;
        private const string GpsSensorName = "gps_gpgga";

        private ILogger logger;
        private StreamCmd streamCmd;
        private StreamArgs streamArgs;
        private MultiUsrp usrp;
        private RxStreamer streamer;
        private RFSensorConfigurationEndToEnd dce;
        private Fftw fftw;
        private ulong gpsMboard;
        private double rxLinearGain;

        private Dictionary<double, double> cacheByRxOFrequency;

        private ICalibrationDataSource calibrationDataSource;
        private CityscapeCalibration cityscapeCalibrations;

        private double[] WindowFct = new double[0];
        private MathLibrary.WindowFunctions WindowFctType = MathLibrary.WindowFunctions.Hann;   
        private MathLibrary.WindowFunctions WindowFctType_current = MathLibrary.WindowFunctions.Hann;   //TODO: Make a knob for this. (Some people may prefer different window fcts).

        public UsrpDevice(ICalibrationDataSource calibrationDataSource, ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (calibrationDataSource == null)
            {
                throw new ArgumentNullException("calibrationDataSource");
            }

            this.calibrationDataSource = calibrationDataSource;
            this.cacheByRxOFrequency = new Dictionary<double, double>();
            this.logger = logger;
        }

        public FeatureVectorProcessor Fvp { get; set; }

        public double BandwidthHz
        {
            get
            {
                return this.dce.BandwidthHz;
            }
        }

        public double StartFrequencyHz
        {
            get
            {
                return this.dce.CurrentStartFrequencyHz;
            }
        }

        public double StopFrequencyHz
        {
            get
            {
                return this.dce.CurrentStopFrequencyHz;
            }
        }

        public int SamplesPerScan
        {
            get
            {
                if (this.dce.SamplesPerScan > UsrpDevice.MaxSamplesPerScan)
                {
                    return UsrpDevice.MaxSamplesPerScan;
                }

                if (this.dce.SamplesPerScan < UsrpDevice.MinSamplesPerScan)
                {
                    return UsrpDevice.MinSamplesPerScan;
                }

                return this.dce.SamplesPerScan;
            }
        }

        public bool RawIqDataAvailable
        {
            get
            {
                return true;
            }
        }

        public bool SamplesAsDb
        {
            get
            {
                return false;
            }
        }

        public string NmeaGpggaLocation
        {
            get
            {
                if (this.dce.GpsEnabled)
                {
                    SensorValue value = this.usrp.get_mboard_sensor(UsrpDevice.GpsSensorName, this.gpsMboard);

                    return value.Value;
                }

                return string.Empty;
            }
        }

        private static int MinSamplesPerScan
        {
            get { return 2; }
        }

        private static int MaxSamplesPerScan
        {
            get { return 50000000; }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string DumpDevice()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("DumpDevices...\r\n");
            sb.AppendLine(this.usrp.get_pp_string());

            ulong mboardCount = this.usrp.get_num_mboards();
            sb.AppendFormat("usrp.get_num_mboards(): {0}\r\n", mboardCount);

            for (ulong mboard = 0; mboard < mboardCount; mboard++)
            {
                sb.AppendFormat("usrp.get_mboard_name(mboard[{0}]): {1}\r\n", mboard, this.usrp.get_mboard_name(mboard));

                sb.AppendFormat("usrp.get_time_sources(mboard[{0}])...\r\n", mboard);
                foreach (string source in this.usrp.get_time_sources(mboard))
                {
                    sb.AppendFormat("\t{0}\r\n", source);
                }

                sb.AppendFormat("usrp.get_time_source(mboard[{0}]): {1}\r\n", mboard, this.usrp.get_time_source(mboard));
                sb.AppendFormat("usrp.get_time_synchronized(): {0}\r\n", this.usrp.get_time_synchronized());

                sb.AppendFormat("usrp.get_clock_sources(mboard[{0}])...\r\n", mboard);
                foreach (string source in this.usrp.get_clock_sources(mboard))
                {
                    sb.AppendFormat("\t{0}\r\n", source);
                }

                sb.AppendFormat("usrp.get_clock_source(mboard[{0}]): {1}\r\n", mboard, this.usrp.get_clock_source(mboard));

                sb.AppendFormat("usrp.get_mboard_sensor_names(mboard[{0}])...\r\n", mboard);
                foreach (string name in this.usrp.get_mboard_sensor_names(mboard))
                {
                    sb.AppendFormat("\tName: {0}\r\n", name);
                    sb.AppendFormat("\tusrp.get_mboard_sensor(name[{0}], mboard[{1}]): {2}\r\n", name, mboard, this.usrp.get_mboard_sensor(name, mboard));
                }

                // Rx
                sb.AppendFormat("usrp.get_rx_subdev_spec(mboard[{0}])...\r\n", mboard);
                foreach (SubDevSpecPair specPair in this.usrp.get_rx_subdev_spec(mboard))
                {
                    sb.AppendFormat("\tsubdev_spec_pair_t.db_name: {0}", specPair.DaughterboardName);
                    sb.AppendFormat("\tsubdev_spec_pair_t.sd_name: {0}\r\n", specPair.SubDeviceName);
                }

                sb.AppendLine();
            }

            ulong channelCount = this.usrp.get_rx_num_channels();
            sb.AppendFormat("usrp.get_rx_num_channels(): {0}\r\n", channelCount);

            for (ulong channel = 0; channel < channelCount; channel++)
            {
                ////sb.AppendFormat("usrp.get_usrp_rx_info(channel[{0}])...\r\n", channel);
                ////Dictionary<string, string> data = usrp.get_usrp_rx_info(channel);
                ////foreach (var item in data)
                ////{
                ////    sb.AppendFormat("\t{0}: {1}\r\n", item.Key, item.Value);
                ////}

                sb.AppendFormat("usrp.get_rx_subdev_name(channel[{0}]): {1}\r\n", channel, this.usrp.get_rx_subdev_name(channel));

                ////sb.AppendFormat("usrp.get_rx_rates(channel[{0}])...\r\n", channel);
                ////foreach (Range range in usrp.get_rx_rates(channel))
                ////{
                ////    sb.AppendFormat("\t{0}\r\n", range.ToString());
                ////}

                sb.AppendFormat("usrp.get_rx_rate(channel[{0}]): {1}\r\n", channel, this.usrp.get_rx_rate(channel));

                sb.AppendFormat("usrp.get_fe_rx_freq_range(channel[{0}])...\r\n", channel);
                foreach (Range range in this.usrp.get_fe_rx_freq_range(channel))
                {
                    sb.AppendFormat("\t{0}\r\n", range.ToString());
                }

                sb.AppendFormat("usrp.get_rx_freq_range(channel[{0}])...\r\n", channel);
                foreach (Range range in this.usrp.get_rx_freq_range(channel))
                {
                    sb.AppendFormat("\t{0}\r\n", range.ToString());
                }

                sb.AppendFormat("usrp.get_rx_freq(channel[{0}]): {1}\r\n", channel, this.usrp.get_rx_freq(channel));

                sb.AppendFormat("usrp.get_rx_gain_names(channel[{0}])...\r\n", channel);
                List<string> gainRangeNames = this.usrp.get_rx_gain_names(channel);
                foreach (string name in gainRangeNames)
                {
                    sb.AppendFormat("\tName: {0}", name);

                    foreach (Range range in this.usrp.get_rx_gain_range(name, channel))
                    {
                        sb.AppendFormat("\t{0}\r\n", range);
                    }

                    sb.AppendFormat("\tActual: {0}\r\n", this.usrp.get_rx_gain(name, channel));
                }

                sb.AppendFormat("usrp.get_rx_gain_range(channel[{0}])...\r\n", channel);
                foreach (Range range in this.usrp.get_rx_gain_range(channel))
                {
                    sb.AppendFormat("\tRange: {0}\r\n", range);
                }

                sb.AppendFormat("usrp.get_rx_gain(channel[{0}]): {1}\r\n", channel, this.usrp.get_rx_gain(channel));

                sb.AppendFormat("usrp.get_rx_antennas(channel[{0}])...\r\n", channel);
                foreach (string name in this.usrp.get_rx_antennas(channel))
                {
                    sb.AppendFormat("\t{0}\r\n", name);
                }

                sb.AppendFormat("usrp.get_rx_antenna(channel[{0}]): {1}\r\n", channel, this.usrp.get_rx_antenna(channel));

                sb.AppendFormat("usrp.get_rx_bandwidth_range(channel[{0}])...\r\n", channel);
                foreach (Range range in this.usrp.get_rx_bandwidth_range(channel))
                {
                    sb.AppendFormat("\t{0}\r\n", range.ToString());
                }

                sb.AppendFormat("usrp.get_rx_bandwidth(channel[{0}]): {1}\r\n", channel, this.usrp.get_rx_bandwidth(channel));

                sb.AppendFormat("usrp.get_rx_sensor_names(channel[{0}])...\r\n", channel);
                foreach (string name in this.usrp.get_rx_sensor_names(channel))
                {
                    sb.AppendFormat("\tName: {0}", name);
                    sb.AppendFormat("\tusrp.get_rx_sensor(name[{0}], channel[{1}]): {2}\r\n", name, channel, this.usrp.get_rx_sensor(name, channel));
                }
            }

            sb.AppendLine();

            return sb.ToString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Object will be disposed outside this scope")]
        public void ConfigureDevice(RFSensorConfigurationEndToEnd deviceConfiguration)
        {
            this.dce = deviceConfiguration;
            double frequencyBuckets = (this.dce.CurrentStopFrequencyHz - this.dce.CurrentStartFrequencyHz) / this.BandwidthHz;

            this.fftw = new Fftw();
            this.fftw.BuildPlan1d(this.dce.SamplesPerScan);

            this.Fvp = new FeatureVectorProcessor((int)(frequencyBuckets * this.dce.SamplesPerScan), this.dce.SamplesPerScan);

            this.usrp = new MultiUsrp(new DeviceAddr() { { this.dce.CommunicationsChannel, this.dce.DeviceAddress } });

            this.usrp.set_rx_bandwidth(this.BandwidthHz, 0);
            this.usrp.set_rx_rate(this.dce.BandwidthHz, 0);
            this.usrp.set_rx_gain(this.dce.Gain, 0);
            this.usrp.set_clock_source("internal", 0);
            this.usrp.set_time_now(0);

            if (this.dce.GpsEnabled)
            {
                ulong mboardCount = this.usrp.get_num_mboards();

                for (ulong mboard = 0; mboard < mboardCount; mboard++)
                {
                    foreach (string name in this.usrp.get_mboard_sensor_names(mboard))
                    {
                        if (name == UsrpDevice.GpsSensorName)
                        {
                            this.gpsMboard = mboard;
                            break;
                        }
                    }
                }
            }

            this.streamArgs = new StreamArgs("fc64", "sc16");
            this.streamArgs.Args = new DeviceAddr();

            /* We are getting errors from this API occasionally, so commenting this out for now
            List<Range> usrpRange = this.usrp.get_rx_freq_range(0);
            if ((usrpRange[0].Stop < (this.dce.CurrentStopFrequencyHz - (this.dce.BandwidthHz / 2))) || (usrpRange[0].Start > (this.dce.CurrentStartFrequencyHz + (this.dce.BandwidthHz / 2))))
            {
                throw new InvalidOperationException("Attempting to tune to a frequency range outside of what is supported by the device!");
            }
            */

            this.streamCmd = new StreamCmd(StreamMode.NumSampsAndDone);
            this.streamCmd.NumSamps = (ulong)this.dce.SamplesPerScan;
            this.streamCmd.StreamNow = true;
            this.streamCmd.TimeSpec = new TimeSpec();

            this.streamer = this.usrp.get_rx_stream(this.streamArgs);
            this.rxLinearGain = MathLibrary.ToRawIQLinearGain(this.dce.Gain);
            this.cityscapeCalibrations = this.calibrationDataSource.LoadCalibrations();
        }

        /// <summary>
        /// The proper way to tune a frequency is to...
        /// Set it.  Sleep 1ms.  Get it.  Check it.
        /// Wait for it to be locked on the device.
        /// If the lock does not happen within 20ms, something is wrong.
        /// 
        /// It turns out that frequency changes are the performance bottleneck accounting for 99% of the time,
        /// due to the Thread.Sleep(1).  after a little digging and benchmarking...
        /// 
        /// From http://stackoverflow.com/questions/1413630/switchtothread-thread-yield-vs-thread-sleep0-vs-thead-sleep1
        /// Thread.Sleep(1): yields to any thread on any processor
        /// Thread.Sleep(0): yields to any thread of same or higher priority on any processor
        /// Thread.Yield(): yields to any thread on same processor
        /// 
        /// It turns out that Sleep(1) causes a 3-15ms delay on my processors, usually greater than or equal to 7ms, and frequently 12-15ms.
        /// Sleep(0), Sleep.Yield(), and no Sleep all completed in 3ms or less.
        /// To be conservative, chose Sleep(0).
        /// 
        /// Chose to iterate 50 times checking for the Local Oscillator (LO) lock, since Sleep(0) isn't a well-defined time, and we
        /// would, on occasion, not lock within 20 attempts.
        /// </summary>
        public double TuneToFrequency(double startFrequencyHz)
        {
            double centerFreq = startFrequencyHz + (this.BandwidthHz / 2);
            const ulong Channel = 0;
            bool tuning = true;
            int tuneAttempts = 0;

            // Try tuning 10 times and if we can't then error out
            while (tuning && tuneAttempts < 10)
            {
                TuneResult result = this.usrp.set_rx_freq(centerFreq, Channel);

                if (this.dce.LockingCommunicationsChannel)
                {
                    // Wait for the frequency to lock
                    bool locked = false;
                    for (int i = 0; i < 50; i++)
                    {
                        SensorValue val = this.usrp.get_rx_sensor("lo_locked", Channel);
                        if (bool.Parse(val.Value))
                        {
                            locked = true;
                            break;
                        }

                        // PLL Flag Poll Delay
                        Thread.Sleep(this.dce.TuneSleep);
                    }

                    if (!locked)
                    {
                        throw new InvalidOperationException("Unable to successfully get a sensor lock!");
                    }
                }

                double actFreqRatio = result.ActualRfFreqHz / result.TargetRfFreqHz;
                double actDspRatio = result.ActualDspFreqHz / result.TargetDspFreqHz;

                if ((actFreqRatio > 0.9999 && actFreqRatio < 1.0001) || (actDspRatio > 0.9999 && actDspRatio < 1.0001))
                {
                    tuning = false;
                }
            }

            // an additional delay after PLL lock
            Thread.Sleep(this.dce.AdditionalTuneDelayInMilliSecs);

            if (tuning == true)
            {
                this.logger.Log(TraceEventType.Error, LoggingMessageId.ScanningBadFrequency, string.Format(CultureInfo.InvariantCulture, "Tuning Error to {0} Hz tried {1} attempts", centerFreq, tuneAttempts));
            }

            return centerFreq;
        }

        public void ReceiveSamples(double[] samples)
        {
            //temp buffer to store I-Q data.
            int TransientLength = 300;  //TODO: Make this a knob.
            double[] tempbuf = new double[samples.Length + TransientLength];

            this.usrp.issue_stream_cmd(this.streamCmd, 0);

            RxMetadata md;
            int receivedSamplesCount = 0;

            //Get I-Q data.
            while (receivedSamplesCount < this.dce.SamplesPerScan)
            {
                int samplesCount = (int)this.streamer.Receive(
                    tempbuf, (ulong)this.dce.SamplesPerScan, UsrpDevice.ComplexWidth, out md, 3, false);

                receivedSamplesCount += samplesCount;

                if (md.ErrorCode != RxErrorCode.None)
                {
                    string detailedError = string.Format(CultureInfo.InvariantCulture, "streamer.Receive returned error code: {0}, Number of samples passed as args {1}, Received samples from RxStreamer {2}, RxMetadata {3}, Dce Samples per scan {4}", md.ErrorCode, samples.Length, receivedSamplesCount, (md != null ? md.ToString() : "Null"), (ulong)this.dce.SamplesPerScan);
                    throw new ScanningErrorException(detailedError);
                }
            }

            //temp buffer -> output buffer
            Array.Copy(tempbuf, TransientLength, samples, 0, samples.Length);

            //Adjust amplitudes using calibration info
            if (this.cityscapeCalibrations != null)
            {
                this.AdjustSnapshotAmplitudes(samples, this.usrp.get_rx_freq((ulong)0), this.rxLinearGain);
            }

            Debug.Assert(receivedSamplesCount == this.dce.SamplesPerScan, "Did not receive the expected number of samples");
        }

        public Complex[] PerformFFT(double[] samples)
        {
            //Console.WriteLine("Center Fre Hz:{0}", rxCenterFrequencyHz);

            //if (this.cityscapeCalibrations != null)
            //{
            //    this.AdjustSnapshotAmplitudes(samples, this.usrp.get_rx_freq((ulong)0), this.rxLinearGain);
            //}

            //Update the Window function If Necessary.
            if (WindowFct.Length != samples.Length || WindowFctType != WindowFctType_current)
            {
                WindowFctType = WindowFctType_current;
                WindowFct = MathLibrary.GetWindowFunction(WindowFctType_current, samples.Length);
            }

            double[] newSamples = MathLibrary.ApplyWindowFunction(samples, WindowFct);

            Complex[] fftData = new Complex[this.dce.SamplesPerScan];

            for (int i = 0; i < this.dce.SamplesPerScan; i++)
            {
                int index = ComplexWidth * i;
                fftData[i] = new Complex(
                    Convert.ToDouble(newSamples[index], CultureInfo.InvariantCulture),
                    Convert.ToDouble(newSamples[index + 1], CultureInfo.InvariantCulture));

                // double currentFreq = currentFrequencies[deviceIndex] + (i / this.config.SamplesPerScan * this.bandwidths[deviceIndex]);                
            }

            this.fftw.Execute1d(fftData);
            FFTAmplitudeCompensation(fftData, WindowFctType);

            return fftData;
        }


        public Complex[] PerformFFTForCenterFrequency(double[] samples, double centerFrequencyWidthInHz)
        {
            if (centerFrequencyWidthInHz > this.BandwidthHz || centerFrequencyWidthInHz <= 0)
            {
                throw new ArgumentOutOfRangeException("centerFrequencyWidthInHz", "Center frequency width out of range");
            }

            double midFrequency = this.dce.CurrentStartFrequencyHz + (this.BandwidthHz / 2);
            double centralFrequencyEqualPartInHz = centerFrequencyWidthInHz / 2;

            double startFrequency = midFrequency - centralFrequencyEqualPartInHz;
            double endFrequency = midFrequency + centralFrequencyEqualPartInHz;

            Func<double, int> frequencyValueIndex = (currentStartFrequency) =>
             {
                 return (int)(((currentStartFrequency - this.dce.CurrentStartFrequencyHz) / this.BandwidthHz) * this.dce.SamplesPerScan);
             };

            int startIndex = frequencyValueIndex(startFrequency);
            int endIndex = frequencyValueIndex(endFrequency);

            Complex[] fftData = new Complex[endIndex - startIndex + 1];

            for (int i = 0; i < fftData.Length; i++)
            {
                int index = ComplexWidth * (i + startIndex);
                fftData[i] = new Complex(
                    Convert.ToDouble(samples[index], CultureInfo.InvariantCulture),
                    Convert.ToDouble(samples[index + 1], CultureInfo.InvariantCulture));

                //double currentFreq = currentFrequencies[deviceIndex] + (i / this.config.SamplesPerScan * this.bandwidths[deviceIndex]);                
            }

            this.fftw.Execute1d(fftData);

            return fftData;
        }

        public void FFTAmplitudeCompensation(Complex[] data, MathLibrary.WindowFunctions fct)
        {
            double factor = MathLibrary.GetWindowCompensationFactor(fct);
            for (int i =0; i < data.Length; i++)
            {
                data[i] = data[i] * factor;
            }
        }

        public int InstantPowerStartIndex(double currentStartFrequency)
        {
            // TODO: When we fix the fact that the scan need to be in bandwidth channels, we will need to fix this as well
            return (int)(Math.Truncate((currentStartFrequency - this.dce.CurrentStartFrequencyHz) / this.BandwidthHz) * this.dce.SamplesPerScan);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Fvp != null)
                {
                    this.Fvp.Dispose();
                }

                if (this.streamer != null)
                {
                    this.streamer.Dispose();
                }

                if (this.usrp != null)
                {
                    this.usrp.Dispose();
                }
            }
        }


        private void AdjustSnapshotAmplitudes(double[] samples, double rxRxOFreqHz, double rxGain)
        {
            double amplitudeAdjustment = this.FindOrMakeAmpliutdeAdjustment(rxRxOFreqHz);

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= amplitudeAdjustment / rxGain;
            }
        }

        private double FindOrMakeAmpliutdeAdjustment(double rxFrequencyHz)
        {
            if (!this.cacheByRxOFrequency.ContainsKey(rxFrequencyHz))
            {
                double amplitude = this.cityscapeCalibrations.ComputeAmplitudeAdjustment(rxFrequencyHz);
                this.cacheByRxOFrequency.Add(rxFrequencyHz, amplitude);

                return amplitude;
            }
            else
            {
                return this.cacheByRxOFrequency[rxFrequencyHz];
            }
        }
    }
}