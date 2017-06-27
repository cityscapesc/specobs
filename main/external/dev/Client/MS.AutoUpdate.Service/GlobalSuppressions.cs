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

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1824:MarkAssembliesWithNeutralResourcesLanguage", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "StyleCop Justification")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "AutoUpdater", Scope = "member", Target = "Microsoft.Spectrum.AutoUpdate.Service.DisplayHelpCommand.#Execute()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "RunAsExe", Scope = "member", Target = "Microsoft.Spectrum.AutoUpdate.Service.DisplayHelpCommand.#Execute()", Justification = "StyleCop Justification")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogStart(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.AutoUpdate.Service.AutoUpdateService.#OnStart(System.String[])", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogStop(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.AutoUpdate.Service.AutoUpdateService.#OnStop()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)", Scope = "member", Target = "Microsoft.Spectrum.AutoUpdate.Service.DisplayHelpCommand.#Execute()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogStart(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.AutoUpdate.Service.RunAsExeCommand.#Execute()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILoggerExtensions.LogStop(Microsoft.Spectrum.Common.ILogger,System.Int32,System.String)", Scope = "member", Target = "Microsoft.Spectrum.AutoUpdate.Service.RunAsExeCommand.#Execute()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)", Scope = "member", Target = "Microsoft.Spectrum.AutoUpdate.Service.RunAsExeCommand.#Execute()", Justification = "StyleCop Justification")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILogger.Log(System.Diagnostics.TraceEventType,Microsoft.Spectrum.Common.LoggingMessageId,System.String)", Scope = "member", Target = "Microsoft.Spectrum.AutoUpdate.Service.AutoUpdateService.#OnStart(System.String[])", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILogger.Log(System.Diagnostics.TraceEventType,Microsoft.Spectrum.Common.LoggingMessageId,System.String)", Scope = "member", Target = "Microsoft.Spectrum.AutoUpdate.Service.AutoUpdateService.#OnStop()", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Microsoft.Spectrum.Common.ILogger.Log(System.Diagnostics.TraceEventType,Microsoft.Spectrum.Common.LoggingMessageId,System.String)", Scope = "member", Target = "Microsoft.Spectrum.AutoUpdate.Service.RunAsExeCommand.#Execute()", Justification = "Style Issue")]
