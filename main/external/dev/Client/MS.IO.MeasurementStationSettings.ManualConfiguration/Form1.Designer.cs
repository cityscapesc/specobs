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
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.devices = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.loadFromFile = new System.Windows.Forms.Button();
            this.saveToFile = new System.Windows.Forms.Button();
            this.measurementStationId = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.outputRawIqData = new System.Windows.Forms.RadioButton();
            this.outputPSDRadiotBtn = new System.Windows.Forms.RadioButton();
            this.secondsOfDataPerSample = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.minutesOfDataPerScanFile = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.singleScan = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.outputPSDInOffCycleCheckBox = new System.Windows.Forms.CheckBox();
            this.cycleTimeTxtBtn = new System.Windows.Forms.TextBox();
            this.label27 = new System.Windows.Forms.Label();
            this.onTimeTxtBtn = new System.Windows.Forms.TextBox();
            this.label26 = new System.Windows.Forms.Label();
            this.retentionRawIqSeconds = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.secondsOfRawIqDataPerFile = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.stopRawIqFrequencyHz = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.startRawIqFrequencyHz = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.addTuneDelayTxtBtn = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.rfSensorLockingCommChannel = new System.Windows.Forms.CheckBox();
            this.rfSensorGPSEnabled = new System.Windows.Forms.CheckBox();
            this.addNewDevice = new System.Windows.Forms.Button();
            this.label24 = new System.Windows.Forms.Label();
            this.rfSensorSampleBlocksPerScan = new System.Windows.Forms.TextBox();
            this.label25 = new System.Windows.Forms.Label();
            this.rfSensorSampleBlocksToThrowAway = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.rfSensorSamplesPerScan = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.rfSensorTuneSleep = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.rfSensorCommChannel = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.rfSensorBandwidthHz = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.rfSensorAntennaPort = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.rfSensorScanPattern = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.rfSensorAddress = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.rfSensorGain = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.rfSensorName = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.rfSensorType = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.rfSensorCurrentStartFrequency = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.rfSensorCurrentStopFrequency = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.rfSensorMinStartFrequency = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.rfSensorMaxStopFrequency = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // devices
            // 
            this.devices.FormattingEnabled = true;
            this.devices.Location = new System.Drawing.Point(629, 29);
            this.devices.Name = "devices";
            this.devices.Size = new System.Drawing.Size(236, 264);
            this.devices.TabIndex = 0;
            this.devices.SelectedIndexChanged += new System.EventHandler(this.DevicesSelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(626, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Devices";
            // 
            // loadFromFile
            // 
            this.loadFromFile.Location = new System.Drawing.Point(668, 544);
            this.loadFromFile.Name = "loadFromFile";
            this.loadFromFile.Size = new System.Drawing.Size(115, 23);
            this.loadFromFile.TabIndex = 2;
            this.loadFromFile.Text = "Load From File...";
            this.loadFromFile.UseVisualStyleBackColor = true;
            this.loadFromFile.Click += new System.EventHandler(this.LoadFromFileClick);
            // 
            // saveToFile
            // 
            this.saveToFile.Location = new System.Drawing.Point(790, 544);
            this.saveToFile.Name = "saveToFile";
            this.saveToFile.Size = new System.Drawing.Size(75, 23);
            this.saveToFile.TabIndex = 3;
            this.saveToFile.Text = "Save As...";
            this.saveToFile.UseVisualStyleBackColor = true;
            this.saveToFile.Click += new System.EventHandler(this.SaveToFileClick);
            // 
            // measurementStationId
            // 
            this.measurementStationId.Enabled = false;
            this.measurementStationId.Location = new System.Drawing.Point(209, 5);
            this.measurementStationId.Name = "measurementStationId";
            this.measurementStationId.Size = new System.Drawing.Size(289, 20);
            this.measurementStationId.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Measurement Station Id";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.outputRawIqData);
            this.groupBox1.Controls.Add(this.outputPSDRadiotBtn);
            this.groupBox1.Controls.Add(this.secondsOfDataPerSample);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.minutesOfDataPerScanFile);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.singleScan);
            this.groupBox1.Location = new System.Drawing.Point(17, 53);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(597, 100);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Aggregation Configuration";
            // 
            // outputRawIqData
            // 
            this.outputRawIqData.AutoSize = true;
            this.outputRawIqData.Location = new System.Drawing.Point(192, 19);
            this.outputRawIqData.Name = "outputRawIqData";
            this.outputRawIqData.Size = new System.Drawing.Size(122, 17);
            this.outputRawIqData.TabIndex = 21;
            this.outputRawIqData.TabStop = true;
            this.outputRawIqData.Text = "Output Raw IQ Data";
            this.outputRawIqData.UseVisualStyleBackColor = true;
            // 
            // outputPSDRadiotBtn
            // 
            this.outputPSDRadiotBtn.AutoSize = true;
            this.outputPSDRadiotBtn.Location = new System.Drawing.Point(13, 20);
            this.outputPSDRadiotBtn.Name = "outputPSDRadiotBtn";
            this.outputPSDRadiotBtn.Size = new System.Drawing.Size(108, 17);
            this.outputPSDRadiotBtn.TabIndex = 8;
            this.outputPSDRadiotBtn.TabStop = true;
            this.outputPSDRadiotBtn.Text = "Output PSD Data";
            this.outputPSDRadiotBtn.UseVisualStyleBackColor = true;
            // 
            // secondsOfDataPerSample
            // 
            this.secondsOfDataPerSample.Location = new System.Drawing.Point(192, 68);
            this.secondsOfDataPerSample.Name = "secondsOfDataPerSample";
            this.secondsOfDataPerSample.Size = new System.Drawing.Size(100, 20);
            this.secondsOfDataPerSample.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(144, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Seconds of Data Per Sample";
            // 
            // minutesOfDataPerScanFile
            // 
            this.minutesOfDataPerScanFile.Location = new System.Drawing.Point(192, 44);
            this.minutesOfDataPerScanFile.Name = "minutesOfDataPerScanFile";
            this.minutesOfDataPerScanFile.Size = new System.Drawing.Size(100, 20);
            this.minutesOfDataPerScanFile.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(148, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Minutes of Data Per Scan File";
            // 
            // singleScan
            // 
            this.singleScan.AutoSize = true;
            this.singleScan.Location = new System.Drawing.Point(337, 19);
            this.singleScan.Name = "singleScan";
            this.singleScan.Size = new System.Drawing.Size(83, 17);
            this.singleScan.TabIndex = 0;
            this.singleScan.Text = "Single Scan";
            this.singleScan.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.outputPSDInOffCycleCheckBox);
            this.groupBox2.Controls.Add(this.cycleTimeTxtBtn);
            this.groupBox2.Controls.Add(this.label27);
            this.groupBox2.Controls.Add(this.onTimeTxtBtn);
            this.groupBox2.Controls.Add(this.label26);
            this.groupBox2.Controls.Add(this.retentionRawIqSeconds);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.secondsOfRawIqDataPerFile);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.stopRawIqFrequencyHz);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.startRawIqFrequencyHz);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Location = new System.Drawing.Point(17, 160);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(597, 151);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Raw IQ Data Configuration";
            // 
            // outputPSDInOffCycleCheckBox
            // 
            this.outputPSDInOffCycleCheckBox.AutoSize = true;
            this.outputPSDInOffCycleCheckBox.Location = new System.Drawing.Point(10, 22);
            this.outputPSDInOffCycleCheckBox.Name = "outputPSDInOffCycleCheckBox";
            this.outputPSDInOffCycleCheckBox.Size = new System.Drawing.Size(167, 17);
            this.outputPSDInOffCycleCheckBox.TabIndex = 20;
            this.outputPSDInOffCycleCheckBox.Text = "Output PSD Data In Off Cycle";
            this.outputPSDInOffCycleCheckBox.UseVisualStyleBackColor = true;
            // 
            // cycleTimeTxtBtn
            // 
            this.cycleTimeTxtBtn.Location = new System.Drawing.Point(483, 73);
            this.cycleTimeTxtBtn.Name = "cycleTimeTxtBtn";
            this.cycleTimeTxtBtn.Size = new System.Drawing.Size(100, 20);
            this.cycleTimeTxtBtn.TabIndex = 19;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(301, 73);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(131, 13);
            this.label27.TabIndex = 18;
            this.label27.Text = "Cycle Time In Milliseconds";
            // 
            // onTimeTxtBtn
            // 
            this.onTimeTxtBtn.Location = new System.Drawing.Point(483, 45);
            this.onTimeTxtBtn.Name = "onTimeTxtBtn";
            this.onTimeTxtBtn.Size = new System.Drawing.Size(100, 20);
            this.onTimeTxtBtn.TabIndex = 17;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(301, 45);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(119, 13);
            this.label26.TabIndex = 16;
            this.label26.Text = "On Time In Milliseconds";
            // 
            // retentionRawIqSeconds
            // 
            this.retentionRawIqSeconds.Location = new System.Drawing.Point(192, 120);
            this.retentionRawIqSeconds.Name = "retentionRawIqSeconds";
            this.retentionRawIqSeconds.Size = new System.Drawing.Size(100, 20);
            this.retentionRawIqSeconds.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 120);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(98, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Retention Seconds";
            // 
            // secondsOfRawIqDataPerFile
            // 
            this.secondsOfRawIqDataPerFile.Location = new System.Drawing.Point(192, 91);
            this.secondsOfRawIqDataPerFile.Name = "secondsOfRawIqDataPerFile";
            this.secondsOfRawIqDataPerFile.Size = new System.Drawing.Size(100, 20);
            this.secondsOfRawIqDataPerFile.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 91);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(125, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Seconds of Data Per File";
            // 
            // stopRawIqFrequencyHz
            // 
            this.stopRawIqFrequencyHz.Location = new System.Drawing.Point(192, 66);
            this.stopRawIqFrequencyHz.Name = "stopRawIqFrequencyHz";
            this.stopRawIqFrequencyHz.Size = new System.Drawing.Size(100, 20);
            this.stopRawIqFrequencyHz.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 66);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(107, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Stop Frequency MHz";
            // 
            // startRawIqFrequencyHz
            // 
            this.startRawIqFrequencyHz.Location = new System.Drawing.Point(192, 42);
            this.startRawIqFrequencyHz.Name = "startRawIqFrequencyHz";
            this.startRawIqFrequencyHz.Size = new System.Drawing.Size(100, 20);
            this.startRawIqFrequencyHz.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 42);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(107, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Start Frequency MHz";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.addTuneDelayTxtBtn);
            this.groupBox3.Controls.Add(this.label21);
            this.groupBox3.Controls.Add(this.rfSensorLockingCommChannel);
            this.groupBox3.Controls.Add(this.rfSensorGPSEnabled);
            this.groupBox3.Controls.Add(this.addNewDevice);
            this.groupBox3.Controls.Add(this.label24);
            this.groupBox3.Controls.Add(this.rfSensorSampleBlocksPerScan);
            this.groupBox3.Controls.Add(this.label25);
            this.groupBox3.Controls.Add(this.rfSensorSampleBlocksToThrowAway);
            this.groupBox3.Controls.Add(this.label22);
            this.groupBox3.Controls.Add(this.rfSensorSamplesPerScan);
            this.groupBox3.Controls.Add(this.label23);
            this.groupBox3.Controls.Add(this.rfSensorTuneSleep);
            this.groupBox3.Controls.Add(this.label17);
            this.groupBox3.Controls.Add(this.rfSensorCommChannel);
            this.groupBox3.Controls.Add(this.label18);
            this.groupBox3.Controls.Add(this.rfSensorBandwidthHz);
            this.groupBox3.Controls.Add(this.label19);
            this.groupBox3.Controls.Add(this.rfSensorAntennaPort);
            this.groupBox3.Controls.Add(this.label20);
            this.groupBox3.Controls.Add(this.rfSensorScanPattern);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Controls.Add(this.rfSensorAddress);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.rfSensorGain);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Controls.Add(this.rfSensorName);
            this.groupBox3.Controls.Add(this.label16);
            this.groupBox3.Controls.Add(this.rfSensorType);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.rfSensorCurrentStartFrequency);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.rfSensorCurrentStopFrequency);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.rfSensorMinStartFrequency);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.rfSensorMaxStopFrequency);
            this.groupBox3.Location = new System.Drawing.Point(17, 318);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(890, 209);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "RF Sensor Configuration";
            // 
            // addTuneDelayTxtBtn
            // 
            this.addTuneDelayTxtBtn.Location = new System.Drawing.Point(192, 169);
            this.addTuneDelayTxtBtn.Name = "addTuneDelayTxtBtn";
            this.addTuneDelayTxtBtn.Size = new System.Drawing.Size(99, 20);
            this.addTuneDelayTxtBtn.TabIndex = 54;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(7, 169);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(145, 13);
            this.label21.TabIndex = 53;
            this.label21.Text = "Additional Tune Delay (msec)";
            // 
            // rfSensorLockingCommChannel
            // 
            this.rfSensorLockingCommChannel.AutoSize = true;
            this.rfSensorLockingCommChannel.Location = new System.Drawing.Point(589, 142);
            this.rfSensorLockingCommChannel.Name = "rfSensorLockingCommChannel";
            this.rfSensorLockingCommChannel.Size = new System.Drawing.Size(129, 17);
            this.rfSensorLockingCommChannel.TabIndex = 52;
            this.rfSensorLockingCommChannel.Text = "Comm Channel Locks";
            this.rfSensorLockingCommChannel.UseVisualStyleBackColor = true;
            // 
            // rfSensorGPSEnabled
            // 
            this.rfSensorGPSEnabled.AutoSize = true;
            this.rfSensorGPSEnabled.Location = new System.Drawing.Point(590, 114);
            this.rfSensorGPSEnabled.Name = "rfSensorGPSEnabled";
            this.rfSensorGPSEnabled.Size = new System.Drawing.Size(90, 17);
            this.rfSensorGPSEnabled.TabIndex = 51;
            this.rfSensorGPSEnabled.Text = "GPS Enabled";
            this.rfSensorGPSEnabled.UseVisualStyleBackColor = true;
            // 
            // addNewDevice
            // 
            this.addNewDevice.Enabled = false;
            this.addNewDevice.Location = new System.Drawing.Point(726, 180);
            this.addNewDevice.Name = "addNewDevice";
            this.addNewDevice.Size = new System.Drawing.Size(148, 23);
            this.addNewDevice.TabIndex = 50;
            this.addNewDevice.Text = "Add /Update Device";
            this.addNewDevice.UseVisualStyleBackColor = true;
            this.addNewDevice.Click += new System.EventHandler(this.AddNewDeviceClick);
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(298, 140);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(171, 13);
            this.label24.TabIndex = 48;
            this.label24.Text = "No. of Snapshots per Channel Visit";
            // 
            // rfSensorSampleBlocksPerScan
            // 
            this.rfSensorSampleBlocksPerScan.Location = new System.Drawing.Point(483, 140);
            this.rfSensorSampleBlocksPerScan.Name = "rfSensorSampleBlocksPerScan";
            this.rfSensorSampleBlocksPerScan.Size = new System.Drawing.Size(100, 20);
            this.rfSensorSampleBlocksPerScan.TabIndex = 49;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(7, 140);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(173, 13);
            this.label25.TabIndex = 46;
            this.label25.Text = "Number of Samples to Throw Away";
            // 
            // rfSensorSampleBlocksToThrowAway
            // 
            this.rfSensorSampleBlocksToThrowAway.Location = new System.Drawing.Point(192, 140);
            this.rfSensorSampleBlocksToThrowAway.Name = "rfSensorSampleBlocksToThrowAway";
            this.rfSensorSampleBlocksToThrowAway.Size = new System.Drawing.Size(100, 20);
            this.rfSensorSampleBlocksToThrowAway.TabIndex = 47;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(298, 114);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(114, 13);
            this.label22.TabIndex = 42;
            this.label22.Text = "Samples Per Snapshot";
            // 
            // rfSensorSamplesPerScan
            // 
            this.rfSensorSamplesPerScan.Location = new System.Drawing.Point(483, 114);
            this.rfSensorSamplesPerScan.Name = "rfSensorSamplesPerScan";
            this.rfSensorSamplesPerScan.Size = new System.Drawing.Size(100, 20);
            this.rfSensorSamplesPerScan.TabIndex = 43;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(7, 114);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(103, 13);
            this.label23.TabIndex = 40;
            this.label23.Text = "PLL Flag Pool Delay";
            // 
            // rfSensorTuneSleep
            // 
            this.rfSensorTuneSleep.Location = new System.Drawing.Point(192, 114);
            this.rfSensorTuneSleep.Name = "rfSensorTuneSleep";
            this.rfSensorTuneSleep.Size = new System.Drawing.Size(100, 20);
            this.rfSensorTuneSleep.TabIndex = 41;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(589, 67);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(126, 13);
            this.label17.TabIndex = 36;
            this.label17.Text = "Communications Channel";
            // 
            // rfSensorCommChannel
            // 
            this.rfSensorCommChannel.Location = new System.Drawing.Point(774, 67);
            this.rfSensorCommChannel.Name = "rfSensorCommChannel";
            this.rfSensorCommChannel.Size = new System.Drawing.Size(100, 20);
            this.rfSensorCommChannel.TabIndex = 37;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(589, 91);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(160, 13);
            this.label18.TabIndex = 38;
            this.label18.Text = "Eff. Sampling Rate (ESPS In Hz)";
            // 
            // rfSensorBandwidthHz
            // 
            this.rfSensorBandwidthHz.Location = new System.Drawing.Point(774, 91);
            this.rfSensorBandwidthHz.Name = "rfSensorBandwidthHz";
            this.rfSensorBandwidthHz.Size = new System.Drawing.Size(100, 20);
            this.rfSensorBandwidthHz.TabIndex = 39;
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(589, 19);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(69, 13);
            this.label19.TabIndex = 32;
            this.label19.Text = "Antenna Port";
            // 
            // rfSensorAntennaPort
            // 
            this.rfSensorAntennaPort.Location = new System.Drawing.Point(774, 19);
            this.rfSensorAntennaPort.Name = "rfSensorAntennaPort";
            this.rfSensorAntennaPort.Size = new System.Drawing.Size(100, 20);
            this.rfSensorAntennaPort.TabIndex = 33;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(589, 43);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(69, 13);
            this.label20.TabIndex = 34;
            this.label20.Text = "Scan Pattern";
            // 
            // rfSensorScanPattern
            // 
            this.rfSensorScanPattern.Location = new System.Drawing.Point(774, 43);
            this.rfSensorScanPattern.Name = "rfSensorScanPattern";
            this.rfSensorScanPattern.Size = new System.Drawing.Size(100, 20);
            this.rfSensorScanPattern.TabIndex = 35;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(298, 64);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(82, 13);
            this.label13.TabIndex = 28;
            this.label13.Text = "Device Address";
            // 
            // rfSensorAddress
            // 
            this.rfSensorAddress.Location = new System.Drawing.Point(483, 64);
            this.rfSensorAddress.Name = "rfSensorAddress";
            this.rfSensorAddress.Size = new System.Drawing.Size(100, 20);
            this.rfSensorAddress.TabIndex = 29;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(298, 88);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(29, 13);
            this.label14.TabIndex = 30;
            this.label14.Text = "Gain";
            // 
            // rfSensorGain
            // 
            this.rfSensorGain.Location = new System.Drawing.Point(483, 88);
            this.rfSensorGain.Name = "rfSensorGain";
            this.rfSensorGain.Size = new System.Drawing.Size(100, 20);
            this.rfSensorGain.TabIndex = 31;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(298, 16);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(91, 13);
            this.label15.TabIndex = 24;
            this.label15.Text = "Descriptive Name";
            // 
            // rfSensorName
            // 
            this.rfSensorName.Location = new System.Drawing.Point(483, 16);
            this.rfSensorName.Name = "rfSensorName";
            this.rfSensorName.Size = new System.Drawing.Size(100, 20);
            this.rfSensorName.TabIndex = 25;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(298, 40);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(68, 13);
            this.label16.TabIndex = 26;
            this.label16.Text = "Device Type";
            // 
            // rfSensorType
            // 
            this.rfSensorType.Location = new System.Drawing.Point(483, 40);
            this.rfSensorType.Name = "rfSensorType";
            this.rfSensorType.Size = new System.Drawing.Size(100, 20);
            this.rfSensorType.TabIndex = 27;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 64);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(144, 13);
            this.label9.TabIndex = 20;
            this.label9.Text = "Current Start Frequency MHz";
            // 
            // rfSensorCurrentStartFrequency
            // 
            this.rfSensorCurrentStartFrequency.Location = new System.Drawing.Point(192, 64);
            this.rfSensorCurrentStartFrequency.Name = "rfSensorCurrentStartFrequency";
            this.rfSensorCurrentStartFrequency.Size = new System.Drawing.Size(100, 20);
            this.rfSensorCurrentStartFrequency.TabIndex = 21;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 88);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(144, 13);
            this.label10.TabIndex = 22;
            this.label10.Text = "Current Stop Frequency MHz";
            // 
            // rfSensorCurrentStopFrequency
            // 
            this.rfSensorCurrentStopFrequency.Location = new System.Drawing.Point(192, 88);
            this.rfSensorCurrentStopFrequency.Name = "rfSensorCurrentStopFrequency";
            this.rfSensorCurrentStopFrequency.Size = new System.Drawing.Size(100, 20);
            this.rfSensorCurrentStopFrequency.TabIndex = 23;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 16);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(127, 13);
            this.label12.TabIndex = 16;
            this.label12.Text = "Min Start Frequency MHz";
            // 
            // rfSensorMinStartFrequency
            // 
            this.rfSensorMinStartFrequency.Location = new System.Drawing.Point(192, 16);
            this.rfSensorMinStartFrequency.Name = "rfSensorMinStartFrequency";
            this.rfSensorMinStartFrequency.Size = new System.Drawing.Size(100, 20);
            this.rfSensorMinStartFrequency.TabIndex = 17;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 40);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(130, 13);
            this.label11.TabIndex = 18;
            this.label11.Text = "Max Stop Frequency MHz";
            // 
            // rfSensorMaxStopFrequency
            // 
            this.rfSensorMaxStopFrequency.Location = new System.Drawing.Point(192, 40);
            this.rfSensorMaxStopFrequency.Name = "rfSensorMaxStopFrequency";
            this.rfSensorMaxStopFrequency.Size = new System.Drawing.Size(100, 20);
            this.rfSensorMaxStopFrequency.TabIndex = 19;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(938, 582);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.measurementStationId);
            this.Controls.Add(this.saveToFile);
            this.Controls.Add(this.loadFromFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.devices);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox devices;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button loadFromFile;
        private System.Windows.Forms.Button saveToFile;
        private System.Windows.Forms.TextBox measurementStationId;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox singleScan;
        private System.Windows.Forms.TextBox minutesOfDataPerScanFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox secondsOfDataPerSample;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox stopRawIqFrequencyHz;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox startRawIqFrequencyHz;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox secondsOfRawIqDataPerFile;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox retentionRawIqSeconds;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox rfSensorCurrentStartFrequency;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox rfSensorCurrentStopFrequency;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox rfSensorMinStartFrequency;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox rfSensorMaxStopFrequency;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox rfSensorAddress;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox rfSensorGain;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox rfSensorName;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox rfSensorType;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox rfSensorCommChannel;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox rfSensorBandwidthHz;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.TextBox rfSensorAntennaPort;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox rfSensorScanPattern;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox rfSensorSamplesPerScan;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.TextBox rfSensorTuneSleep;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.TextBox rfSensorSampleBlocksPerScan;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.TextBox rfSensorSampleBlocksToThrowAway;
        private System.Windows.Forms.Button addNewDevice;
        private System.Windows.Forms.CheckBox rfSensorGPSEnabled;
        private System.Windows.Forms.CheckBox rfSensorLockingCommChannel;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox addTuneDelayTxtBtn;
        private System.Windows.Forms.TextBox cycleTimeTxtBtn;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TextBox onTimeTxtBtn;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.RadioButton outputPSDRadiotBtn;
        private System.Windows.Forms.RadioButton outputRawIqData;
        private System.Windows.Forms.CheckBox outputPSDInOffCycleCheckBox;
    }
}

