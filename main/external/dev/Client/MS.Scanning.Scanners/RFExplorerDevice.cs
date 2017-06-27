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
    using Microsoft.Spectrum.IO.MeasurementStationSettings;
    using RFExplorerCommunicator;    

    public class RFExplorerDevice : IDevice
    {        
        private const int ComplexWidth = 2;

        private ILogger logger;
        private RFSensorConfigurationEndToEnd dce;
        private RFECommunicator rfe;
        private EventWaitHandle latch;
        private EventWaitHandle samplesRecieved;
        private RFESweepData sweepData;
        private string reportInfo;
        private string rfeReceivedString = string.Empty;
        private Thread recieveThread;

        public RFExplorerDevice(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;

            this.latch = new EventWaitHandle(false, EventResetMode.AutoReset);
            this.samplesRecieved = new EventWaitHandle(false, EventResetMode.AutoReset);
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
                if (this.dce.SamplesPerScan > RFExplorerDevice.MaxSamplesPerScan)
                {
                    return RFExplorerDevice.MaxSamplesPerScan;
                }

                if (this.dce.SamplesPerScan < RFExplorerDevice.MinSamplesPerScan)
                {
                    return RFExplorerDevice.MinSamplesPerScan;
                }

                return this.dce.SamplesPerScan;
            }
        }

        public bool RawIqDataAvailable
        {
            get 
            {
                return false;
            }
        }

        public bool SamplesAsDb
        {
            get
            {
                return true;
            }
        }

        public string NmeaGpggaLocation
        {
            get
            {
                return string.Empty;
            }
        }

        private static int MinSamplesPerScan
        {
            get { return 112; }
        }

        private static int MaxSamplesPerScan
        {
            get { return 112; }
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

            sb.AppendFormat("rfe.Mode: {0}\r\n", this.rfe.Mode.ToString());
            sb.AppendFormat("rfe.MainBoardModel: {0}", this.rfe.MainBoardModel.ToString());
            sb.AppendFormat("rfe.ExpansionBoardModel: {0}", this.rfe.ExpansionBoardModel.ToString());
            sb.AppendFormat("rfe.ExpansionBoardActive: {0}", this.rfe.ExpansionBoardActive.ToString());
            sb.AppendFormat("rfe.ActiveModel: {0}", this.rfe.ActiveModel.ToString());
            sb.AppendFormat("rfe.MinFreqMHZ: {0}", this.rfe.MinFreqMHZ);
            sb.AppendFormat("rfe.PeakValueMHZ: {0}", this.rfe.MaxFreqMHZ);
            sb.AppendFormat("rfe.MinSpanMHZ: {0}", this.rfe.MinSpanMHZ);
            sb.AppendFormat("rfe.MaxSpanMHZ: {0}", this.rfe.MaxSpanMHZ);
            sb.AppendFormat("rfe.ConfigurationText: {0}\r\n", this.rfe.ConfigurationText);
            
            sb.AppendLine();

            return sb.ToString();
        }                                

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Object will be disposed outside this scope")]
        public void ConfigureDevice(RFSensorConfigurationEndToEnd deviceConfiguration)
        {
            this.dce = deviceConfiguration;            
            double frequencyBuckets = (this.dce.CurrentStopFrequencyHz - this.dce.CurrentStartFrequencyHz) / this.BandwidthHz;

            this.Fvp = new FeatureVectorProcessor((int)(frequencyBuckets * this.dce.SamplesPerScan), this.dce.SamplesPerScan);

            this.rfe = new RFECommunicator();
            this.rfe.ReceivedConfigurationDataEvent += new EventHandler(this.OnRFE_ReceivedConfigData);
            this.rfe.ReportInfoAddedEvent += new EventHandler(this.OnRFE_ReportInfoAdded);
            this.rfe.UpdateDataEvent += new EventHandler(this.OnRFE_UpdateData);

            int i;
            if (!this.rfe.GetConnectedPorts())
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "The RF Explorer device is not configured properly. {0}", this.reportInfo));
            }
            
            // check to make sure that comm port is a valid choice
            for (i = 0; i < this.rfe.ValidCP2101Ports.Length; i++)
            {
                if (this.rfe.ValidCP2101Ports[i] == this.dce.CommunicationsChannel)
                {
                    break;
                }
            }            

            if (i == this.rfe.ValidCP2101Ports.Length)
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "The RF Explorer is available only on the following communication ports {0}, please check your settings", string.Join(", ", this.rfe.ValidCP2101Ports)));
            }

            this.rfe.ConnectPort(this.dce.CommunicationsChannel, 500000);
            
            if (this.rfe.PortConnected)
            {
                this.recieveThread = new Thread(new ThreadStart(this.ProcessReceivedString));
                this.recieveThread.Start();

                if (!this.rfe.AutoConfigure)
                {
                    this.rfe.AskConfigData();
                }

                Thread.Sleep(2000);
                this.rfe.ProcessReceivedString(true, out this.rfeReceivedString);

                // Wait until we get the response back from the RF Explorer device
                EventWaitHandle.WaitAny(new WaitHandle[] { this.latch });                
            }        
            else
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "The RF Explorer device is not configured properly. {0}", this.reportInfo));
            }
        }

        public double TuneToFrequency(double startFrequencyHz)
        {
            this.rfe.StepFrequencyMHZ = MathLibrary.HzToMHz(this.BandwidthHz) / this.rfe.FreqSpectrumSteps;

            // #[32]C2-F:Sssssss,Eeeeeee,tttt,bbbb
            uint startKhz = (uint)(MathLibrary.HzToMHz(startFrequencyHz) * 1000);
            uint endKhz = startKhz + (uint)(MathLibrary.HzToMHz(this.BandwidthHz) * 1000);
            short topDBM = (short)20;
            short bottomDBM = (short)(-150);

            string data = "C2-F:" +
                startKhz.ToString("D7", CultureInfo.InvariantCulture) + "," + endKhz.ToString("D7", CultureInfo.InvariantCulture) + "," +
                topDBM.ToString("D3", CultureInfo.InvariantCulture) + "," + bottomDBM.ToString("D3", CultureInfo.InvariantCulture);
            
            int eventExit = WaitHandle.WaitTimeout;

            while (eventExit == WaitHandle.WaitTimeout)
            {
                this.rfe.SendCommand(data);

                // Wait until we get the response back from the RF Explorer device
                eventExit = EventWaitHandle.WaitAny(new WaitHandle[] { this.latch }, 1000);    
            }

            return this.rfe.StartFrequencyMHZ;            
        }

        public void ReceiveSamples(double[] samples)
        {
            // Wait until we get the samples back from the RF Explorer device
            EventWaitHandle.WaitAny(new WaitHandle[] { this.samplesRecieved });

            for (ushort i = 0; i < this.SamplesPerScan; i++)
            {
                samples[i] = this.sweepData.GetAmplitudeDBM(i);
            }

            // after we take a snapshot of the data, we don't want to use the same data again
            this.rfe.SweepData.CleanAll(); 
        }

        public Complex[] PerformFFT(double[] samples)
        {
            // This device doesn't return raw samples, so it doesn't need to do an FFT
            return null;
        }

        public Complex[] PerformFFTForCenterFrequency(double[] samples, double centerFrequencyWidthInHz)
        {

            // This device doesn't return raw samples, so it doesn't need to do an FFT
            return null;
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
                if (this.rfe != null)
                {
                    this.rfe.ClosePort();
                    this.rfe.Close();
                }

                if (this.samplesRecieved != null)
                {
                    this.samplesRecieved.Dispose();
                }

                if (this.latch != null)
                {
                    this.latch.Dispose();
                }

                if (this.Fvp != null)
                {
                    this.Fvp.Dispose();
                }
            }
        }

        private void OnRFE_ReceivedConfigData(object sender, EventArgs e)
        {
            // we do not want mixed data sweep values
            this.rfe.SweepData.CleanAll(); 

            this.latch.Set();
        }

        private void OnRFE_UpdateData(object sender, EventArgs e)
        {
            // With the RF Explorer, the Windows app requires a minimum of two sweeps, so we will do the same
            if (!(this.rfe.SweepData.UpperBound < 2))
            {
                this.sweepData = this.rfe.SweepData.MaxHoldData;
                this.samplesRecieved.Set();
            }
        }

        private void OnRFE_ReportInfoAdded(object sender, EventArgs e)
        {
            this.reportInfo = ((EventReportInfo)e).Data;
        }

        private void ProcessReceivedString()
        {
            while (this.rfe.PortConnected)
            {
                this.rfe.ProcessReceivedString(true, out this.rfeReceivedString);
            }
        }
    }
}