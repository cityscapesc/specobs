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
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Usrp", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Usrp", Scope = "namespace", Target = "Microsoft.Spectrum.IO.ScanFile", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Usrp", Scope = "type", Target = "Microsoft.Spectrum.IO.ScanFile.UsrpBinaryStorageModelFormatter", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Usrp", Scope = "type", Target = "Microsoft.Spectrum.IO.ScanFile.UsrpFileReader", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Usrp", Scope = "type", Target = "Microsoft.Spectrum.IO.ScanFile.UsrpFileWriterManager", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralDataBlock.#.ctor(System.Int16,System.Int16,Microsoft.Spectrum.Common.FixedShort[])", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralDataBlock.#StartFrequencyMHz", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralDataBlock.#StopFrequencyMHz", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralDataBlock.#.ctor(System.Int16,System.Int16,Microsoft.Spectrum.Storage.Model.ReadingKind,Microsoft.Spectrum.Common.FixedShort[])", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#StopFrequencyHz", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#StartFrequencyHz", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#.ctor(System.DateTime,System.Double,System.Double,Microsoft.Spectrum.Storage.Model.ReadingKind,Microsoft.Spectrum.Common.FixedShort[],System.Int32)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#.ctor(System.DateTime,System.Double,System.Double,Microsoft.Spectrum.Storage.Model.ReadingKind,Microsoft.Spectrum.Common.FixedShort[],System.Int32)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Psd", Scope = "type", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TimeStamp", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.ScanFileWriter.#TimeStamp", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TimeRangeKind", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.DataBlockExtensions.#GetTimeGranularity(Microsoft.Spectrum.Common.TimeRangeKind,System.DateTime,System.DateTime&)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Psd", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.ScanFile.#SpectralPsdData", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.ScanFile.#SpectralPsdData", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#OutputDataPoints", Justification = "Performance")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.ScanFileWriter.#WriteBlock(Microsoft.Spectrum.IO.ScanFile.DataBlock)", Justification = "Performance")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.ScanFile.#SpectralPsdData", Justification = "Performance")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Scope = "type", Target = "Microsoft.Spectrum.IO.ScanFile.ScanFile", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.ConfigDataBlock.#.ctor(System.String)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#DataPoints", Justification = "Performance")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Nmea", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#NmeaGpggaLocation", Justification = "Proper for GPS")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#.ctor(System.DateTime,System.Double,System.Double,Microsoft.Spectrum.Storage.Model.ReadingKind,Microsoft.Spectrum.Common.FixedShort[],System.Int32,System.String)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#.ctor(System.DateTime,System.Double,System.Double,Microsoft.Spectrum.Storage.Model.ReadingKind,Microsoft.Spectrum.Common.FixedShort[],System.Int32,System.String)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpgga", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#.ctor(System.DateTime,System.Double,System.Double,Microsoft.Spectrum.Storage.Model.ReadingKind,Microsoft.Spectrum.Common.FixedShort[],System.Int32,System.String)", Justification = "Proper for GPS")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpgga", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#NmeaGpggaLocation", Justification = "Proper for GPS")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "nmea", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#.ctor(System.DateTime,System.Double,System.Double,Microsoft.Spectrum.Storage.Model.ReadingKind,Microsoft.Spectrum.Common.FixedShort[],System.Int32,System.String)", Justification = "Proper for GPS")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gpgga", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#.ctor(System.DateTime,System.Double,System.Double,Microsoft.Spectrum.Common.ReadingKind,Microsoft.Spectrum.Common.FixedShort[],System.Int32,System.String)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "nmea", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#.ctor(System.DateTime,System.Double,System.Double,Microsoft.Spectrum.Common.ReadingKind,Microsoft.Spectrum.Common.FixedShort[],System.Int32,System.String)", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Hz", Scope = "member", Target = "Microsoft.Spectrum.IO.ScanFile.SpectralPsdDataBlock.#.ctor(System.DateTime,System.Double,System.Double,Microsoft.Spectrum.Common.ReadingKind,Microsoft.Spectrum.Common.FixedShort[],System.Int32,System.String)", Justification = "Style Issue")]
