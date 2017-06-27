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

namespace MS.IO.MeasurementStationSettings.ManualConfiguration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;
    using ProtoBuf;   

    public partial class Form1 : Form
    {
        private MeasurementStationConfigurationEndToEnd configuration;
        private string previousDevice = string.Empty;
        private const string stationId = "7f7a9ad8-dda9-54be-8899d-939ace9eda94";

        public Form1()
        {
            this.InitializeComponent();
            this.devices.SelectionMode = SelectionMode.One;

            this.configuration = new MeasurementStationConfigurationEndToEnd();
        }

        private void LoadFromFileClick(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.InitialDirectory = "c:\\SpectrumData\\Settings";
            DialogResult result = fileDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string filename = fileDialog.SafeFileName;

                using (Stream input = fileDialog.OpenFile())
                {
                    this.configuration = MeasurementStationConfigurationEndToEnd.Read(input); //Serializer.Deserialize<MeasurementStationConfigurationEndToEnd>(input);
                }

                this.LoadFromCurrentConfiguration();
            }
        }

        private void LoadFromCurrentConfiguration()
        {
            this.measurementStationId.Text = string.IsNullOrWhiteSpace(this.configuration.MeasurementStationId) == true ? stationId : this.configuration.MeasurementStationId;

            // Aggregation
            this.outputPSDRadiotBtn.Checked = this.configuration.AggregationConfiguration.OutputData;
            this.singleScan.Checked = this.configuration.AggregationConfiguration.SingleScan;
            this.minutesOfDataPerScanFile.Text = this.configuration.AggregationConfiguration.MinutesOfDataPerScanFile.TotalMinutes.ToString();
            this.secondsOfDataPerSample.Text = this.configuration.AggregationConfiguration.SecondsOfDataPerSample.TotalSeconds.ToString();

            // Raw IQ
            this.outputRawIqData.Checked = this.configuration.RawIqConfiguration.OutputData;
            this.outputPSDInOffCycleCheckBox.Checked = this.configuration.RawIqConfiguration.OuputPSDDataInDutyCycleOffTime;
            this.onTimeTxtBtn.Text = this.configuration.RawIqConfiguration.DutycycleOnTimeInMilliSec.ToString();
            this.cycleTimeTxtBtn.Text = this.configuration.RawIqConfiguration.DutycycleTimeInMilliSec.ToString();

            this.startRawIqFrequencyHz.Text = MathLibrary.HzToMHz(this.configuration.RawIqConfiguration.StartFrequencyHz).ToString();
            this.stopRawIqFrequencyHz.Text = MathLibrary.HzToMHz(this.configuration.RawIqConfiguration.StopFrequencyHz).ToString();
            this.secondsOfRawIqDataPerFile.Text = this.configuration.RawIqConfiguration.SecondsOfDataPerFile.ToString();
            this.retentionRawIqSeconds.Text = this.configuration.RawIqConfiguration.RetentionSeconds.ToString();

            devices.Items.Clear();

            // RF Sensor
            foreach (var device in this.configuration.RFSensorConfigurations)
            {
                int index = this.devices.Items.Add(device.DescriptiveName);

                if (this.devices.SelectedItem != null)
                {
                    this.UpdateCurrentDeviceSelected(device);
                    break;
                }
            }
        }

        private void UpdateCurrentDeviceSelected(RFSensorConfigurationEndToEnd device)
        {
            this.CacheCurrentDevice(this.previousDevice, false);

            this.rfSensorCurrentStartFrequency.Text = MathLibrary.HzToMHz(device.CurrentStartFrequencyHz).ToString();
            this.rfSensorCurrentStopFrequency.Text = MathLibrary.HzToMHz(device.CurrentStopFrequencyHz).ToString();
            this.rfSensorAddress.Text = device.DeviceAddress;
            this.rfSensorAntennaPort.Text = device.AntennaPort;
            this.rfSensorBandwidthHz.Text = device.BandwidthHz.ToString();
            this.rfSensorCommChannel.Text = device.CommunicationsChannel;
            this.rfSensorGain.Text = device.Gain.ToString();
            this.rfSensorGPSEnabled.Checked = device.GpsEnabled;
            this.rfSensorMaxStopFrequency.Text = MathLibrary.HzToMHz(device.MaxPossibleEndFrequencyHz).ToString();
            this.rfSensorMinStartFrequency.Text = MathLibrary.HzToMHz(device.MinPossibleStartFrequencyHz).ToString();
            this.rfSensorName.Text = device.DescriptiveName;
            this.rfSensorSampleBlocksPerScan.Text = device.NumberOfSampleBlocksPerScan.ToString();
            this.rfSensorSampleBlocksToThrowAway.Text = device.NumberOfSampleBlocksToThrowAway.ToString();
            this.rfSensorSamplesPerScan.Text = device.SamplesPerScan.ToString();
            this.rfSensorScanPattern.Text = device.ScanPattern;
            this.rfSensorTuneSleep.Text = device.TuneSleep.ToString();
            this.rfSensorType.Text = device.DeviceType;
            this.rfSensorLockingCommChannel.Checked = device.LockingCommunicationsChannel;
            this.addTuneDelayTxtBtn.Text = device.AdditionalTuneDelayInMilliSecs.ToString();

            this.previousDevice = device.DescriptiveName;
        }

        private void CacheCurrentDevice(string currentDevice, bool newDevice)
        {
            RFSensorConfigurationEndToEnd device = null;

            foreach (var searchdevice in this.configuration.RFSensorConfigurations)
            {
                if (currentDevice == searchdevice.DescriptiveName)
                {
                    device = searchdevice;
                    break;
                }
            }

            if (device == null && newDevice == true)
            {
                device = new RFSensorConfigurationEndToEnd();
                this.configuration.RFSensorConfigurations.Add(device);
                this.devices.Items.Add(currentDevice);
            }

            if (device != null)
            {
                device.CurrentStartFrequencyHz = MathLibrary.MHzToHz(Convert.ToDouble(this.rfSensorCurrentStartFrequency.Text));
                device.CurrentStopFrequencyHz = MathLibrary.MHzToHz(Convert.ToDouble(this.rfSensorCurrentStopFrequency.Text));
                device.DeviceAddress = this.rfSensorAddress.Text;
                device.AntennaPort = this.rfSensorAntennaPort.Text;
                device.BandwidthHz = Convert.ToDouble(this.rfSensorBandwidthHz.Text);
                device.CommunicationsChannel = this.rfSensorCommChannel.Text;
                device.Gain = Convert.ToDouble(this.rfSensorGain.Text);
                device.GpsEnabled = this.rfSensorGPSEnabled.Checked;
                device.MaxPossibleEndFrequencyHz = MathLibrary.MHzToHz(Convert.ToDouble(this.rfSensorMaxStopFrequency.Text));
                device.MinPossibleStartFrequencyHz = MathLibrary.MHzToHz(Convert.ToDouble(this.rfSensorMinStartFrequency.Text));
                device.DescriptiveName = this.rfSensorName.Text;
                device.NumberOfSampleBlocksPerScan = Convert.ToInt32(this.rfSensorSampleBlocksPerScan.Text);
                device.NumberOfSampleBlocksToThrowAway = Convert.ToInt32(this.rfSensorSampleBlocksToThrowAway.Text);
                device.SamplesPerScan = Convert.ToInt32(this.rfSensorSamplesPerScan.Text);
                device.ScanPattern = this.rfSensorScanPattern.Text;
                device.TuneSleep = Convert.ToInt32(this.rfSensorTuneSleep.Text);
                device.DeviceType = this.rfSensorType.Text;
                device.LockingCommunicationsChannel = this.rfSensorLockingCommChannel.Checked;
                device.AdditionalTuneDelayInMilliSecs = Convert.ToInt32(this.addTuneDelayTxtBtn.Text);
            }
        }

        private void CacheAllDisplayedSettings()
        {
            // Aggregation
            this.configuration.AggregationConfiguration.OutputData = this.outputPSDRadiotBtn.Checked;
            this.configuration.AggregationConfiguration.SingleScan = this.singleScan.Checked;
            this.configuration.AggregationConfiguration.MinutesOfDataPerScanFile = TimeSpan.FromMinutes(Convert.ToInt32(this.minutesOfDataPerScanFile.Text));
            this.configuration.AggregationConfiguration.SecondsOfDataPerSample = TimeSpan.FromSeconds(Convert.ToInt32(this.secondsOfDataPerSample.Text));

            // Raw IQ
            this.configuration.RawIqConfiguration.OutputData = this.outputRawIqData.Checked;
            this.configuration.RawIqConfiguration.DutycycleOnTimeInMilliSec = Convert.ToInt32(this.onTimeTxtBtn.Text);
            this.configuration.RawIqConfiguration.DutycycleTimeInMilliSec = Convert.ToInt32(this.cycleTimeTxtBtn.Text);
            this.configuration.RawIqConfiguration.OuputPSDDataInDutyCycleOffTime = this.outputPSDInOffCycleCheckBox.Checked;
            this.configuration.RawIqConfiguration.StartFrequencyHz = MathLibrary.MHzToHz(Convert.ToDouble(this.startRawIqFrequencyHz.Text));
            this.configuration.RawIqConfiguration.StopFrequencyHz = MathLibrary.MHzToHz(Convert.ToDouble(this.stopRawIqFrequencyHz.Text));
            this.configuration.RawIqConfiguration.SecondsOfDataPerFile = Convert.ToInt32(this.secondsOfRawIqDataPerFile.Text);
            this.configuration.RawIqConfiguration.RetentionSeconds = Convert.ToInt32(this.retentionRawIqSeconds.Text);

            this.configuration.MeasurementStationId = this.measurementStationId.Text;

            if (this.devices.SelectedItem != null)
            {
                this.CacheCurrentDevice(this.devices.SelectedItem.ToString(), false);
            }
        }

        private void DevicesSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.devices.SelectedItem != null)
            {
                foreach (var device in this.configuration.RFSensorConfigurations)
                {
                    if (this.devices.SelectedItem.ToString() == device.DescriptiveName)
                    {
                        this.UpdateCurrentDeviceSelected(device);
                        break;
                    }
                }
            }
        }

        private void SaveToFileClick(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.InitialDirectory = "c:\\SpectrumData\\Settings";
            saveDialog.FileName = "stationConfiguration.dsos";
            DialogResult result = saveDialog.ShowDialog();

            this.CacheAllDisplayedSettings();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                using (Stream output = saveDialog.OpenFile())
                {
                    this.configuration.LastModifiedTime = DateTime.UtcNow;
                    Serializer.Serialize<MeasurementStationConfigurationEndToEnd>(output, this.configuration);
                }
            }
        }

        private void AddNewDeviceClick(object sender, EventArgs e)
        {
            this.CacheCurrentDevice(this.rfSensorName.Text, true);
        }
    }
}
