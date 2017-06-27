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
    using System.Numerics;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.MeasurementStationSettings;

    public interface IDevice : IDisposable
    {
        FeatureVectorProcessor Fvp { get; }

        double BandwidthHz { get; }

        double StartFrequencyHz { get; }

        double StopFrequencyHz { get; }

        int SamplesPerScan { get; }

        bool RawIqDataAvailable { get; }

        bool SamplesAsDb { get; }

        string NmeaGpggaLocation { get; }

        void ConfigureDevice(RFSensorConfigurationEndToEnd deviceConfiguration);

        string DumpDevice();

        double TuneToFrequency(double startFrequencyHz);

        void ReceiveSamples(double[] samples);

        Complex[] PerformFFT(double[] samples);

        Complex[] PerformFFTForCenterFrequency(double[] samples, double centerFrequencyWidthInHz);

        int InstantPowerStartIndex(double currentStartFrequency);
    }
}
