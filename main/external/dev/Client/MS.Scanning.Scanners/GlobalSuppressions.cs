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

// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Spectrum.Scanning", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Usrp", Scope = "type", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Usrp", Scope = "type", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScannerConfigurationSection", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Cpu", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScannerConfigurationSection.#CpuFormat", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fft", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScannerConfigurationSection.#MultithreadFft", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Rx", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScannerConfigurationSection.#BandwidthHz", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ms", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#EndOfFullScan()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#EndOfFullScan()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "UsrpSequentialScanner", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#StartScanning()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogInformation(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#StartScanning()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "UsrpSequentialScanner", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#StopScanning()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogInformation(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#StopScanning()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "UsrpSequentialScanner", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#ScanThread()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogInformation(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#ScanThread()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogError(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#TuneToFrequency(Microsoft.Spectrum.Devices.Usrp.MultiUsrp,System.UInt64,System.Double)", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogCritical(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#ScanThread()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogWarning(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#ScanThread()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fft", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScannerConfigurationSection.#FftAlg", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogCritical(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#ScanThread`1()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogWarning(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#ScanThread`1()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogInformation(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#ScanThread`1()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "UsrpSequentialScanner", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#ScanThread`1()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogWarning(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#InnerScanLoop`1(System.Collections.Generic.List`1<Microsoft.Spectrum.Devices.Usrp.MultiUsrp>,System.Int32&)", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ThrowAway", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScannerConfigurationSection.#NumberOfSampleBlocksToThrowAway", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogError(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpSequentialScanner.#TuneToFrequency(Microsoft.Spectrum.Devices.Usrp.MultiUsrp,System.UInt64,System.Double,System.Double)", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpDevice.#PerformFFT(System.Double[])", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILogger.Log(System.Diagnostics.TraceEventType,Microsoft.Spectrum.Common.LoggingMessageId,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpDevice.#TuneToFrequency(System.Double)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpDevice.#ConfigureDevice(Microsoft.Spectrum.Scanning.Scanners.DeviceConfigurationElement)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "cts", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpDevice.#.ctor(Microsoft.Spectrum.Common.ILogger,System.Threading.CancellationTokenSource)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Usrp", Scope = "type", Target = "Microsoft.Spectrum.Scanning.Scanners.UsrpDevice", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.Scanner.#EndOfFullScan()", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ms", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.Scanner.#EndOfFullScan()", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILogger.Log(System.Diagnostics.TraceEventType,Microsoft.Spectrum.Common.LoggingMessageId,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.Scanner.#InnerScanLoop(System.Collections.Generic.List`1<Microsoft.Spectrum.Scanning.Scanners.IDevice>,System.Int32&)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILogger.Log(System.Diagnostics.TraceEventType,Microsoft.Spectrum.Common.LoggingMessageId,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.Scanner.#ScanThread()", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "UsrpSequentialScanner", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.Scanner.#ScanThread()", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILogger.Log(System.Diagnostics.TraceEventType,Microsoft.Spectrum.Common.LoggingMessageId,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.Scanner.#StopScanning()", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "UsrpSequentialScanner", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.Scanner.#StopScanning()", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILogger.Log(System.Diagnostics.TraceEventType,Microsoft.Spectrum.Common.LoggingMessageId,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.Scanner.#StartScanning()", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "UsrpSequentialScanner", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.Scanner.#StartScanning()", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "cts", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.RFExplorerDevice.#.ctor(Microsoft.Spectrum.Common.ILogger,System.Threading.CancellationTokenSource)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.RawIqDataConfigurationSection.#StopFrequencyHz", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.RawIqDataConfigurationSection.#StartFrequencyHz", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Rawiq", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.RawIqDataConfigurationSection.#RawiqDirectory", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Iq", Scope = "type", Target = "Microsoft.Spectrum.Scanning.Scanners.RawIqDataConfigurationSection", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iq", Scope = "type", Target = "Microsoft.Spectrum.Scanning.Scanners.RawIqDataConfigurationSection", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "FFT", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.IDevice.#PerformFFT(System.Double[])", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Iq", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.IDevice.#RawIqDataAvailable", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.IDevice.#StopFrequencyHz", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.IDevice.#StartFrequencyHz", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.IDevice.#BandwidthHz", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fvp", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.IDevice.#Fvp", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "fft", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.FeatureVectorProcessor.#ProcessDataDCSpikeScan(System.Numerics.Complex[],System.Numerics.Complex[],System.Int32)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "fft", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.FeatureVectorProcessor.#ProcessData(System.Numerics.Complex[],System.Int32)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ThrowAway", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.DeviceConfigurationElement.#NumberOfSampleBlocksToThrowAway", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.DeviceConfigurationElement.#BandwidthHz", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.DeviceConfigurationElement.#StopFrequencyHz", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.DeviceConfigurationElement.#StartFrequencyHz", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.IDevice.#TuneToFrequency(System.Double)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Iq", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.IDevice.#RawIqDataAvailable", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILogger.Log(System.Diagnostics.TraceEventType,Microsoft.Spectrum.Common.LoggingMessageId,System.String)", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.Scanner.#InnerScanLoop(System.Int32&)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DeviceType", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.Scanner.#ScanThread()", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.FeatureVectorProcessor.#GetResults()", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.FeatureVectorProcessor.#ProcessDbData(System.Double[],System.Int32)", Justification = "Performance")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.FeatureVectorProcessor.#ProcessDataDCSpikeScan(System.Numerics.Complex[],System.Numerics.Complex[],System.Int32)", Justification = "Performance")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.FeatureVectorProcessor.#ProcessData(System.Numerics.Complex[],System.Int32)", Justification = "Performance")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fft", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.FeatureVectorProcessor.#.ctor(System.Int32,System.Int32,System.Int32)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Usrp", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.DeviceType.#Usrp", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.RFExplorerDevice.#ReceiveSamples(System.Double[])", Justification = "Performance")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.RFExplorerDevice.#ConfigureDevice(Microsoft.Spectrum.Scanning.Scanners.DeviceConfigurationElement)", Justification = "Performance")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.ReadingKindData.#Data", Justification = "Performance")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.FeatureVectorProcessor.#ReturnItemToPool(Microsoft.Spectrum.Common.FixedShort[])", Justification = "Performance")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpgga", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.IDevice.#NmeaGpggaLocation", Justification = "valid for gps messages")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nmea", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.IDevice.#NmeaGpggaLocation", Justification = "valid for gps messages")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gps", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.DeviceConfigurationElement.#GpsEnabled", Justification = "valid for gps messages")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.Scanner+ConfigJsonFormatter.#SensorConfiguration", Justification = "Used in the JSON output")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.Spectrum.Scanning.Scanners.Scanner+ConfigJsonFormatter.#HardwareSpecifics", Justification = "Used in the JSON output")]
