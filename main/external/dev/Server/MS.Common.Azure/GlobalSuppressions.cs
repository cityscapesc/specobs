// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant", Justification = "Not going to make these cls compliant")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wll", Scope = "member", Target = "Microsoft.Spectrum.Common.Azure.ConnectionStringsUtility.#WllAppId", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Wllsecret", Scope = "member", Target = "Microsoft.Spectrum.Common.Azure.ConnectionStringsUtility.#Wllsecret", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Microsoft.Spectrum.Common.Azure.ConnectionStringsUtility.#MSIInstallerUri", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "MSI", Scope = "member", Target = "Microsoft.Spectrum.Common.Azure.ConnectionStringsUtility.#MSIInstallerUri", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Microsoft.Spectrum.Common.Azure.ConnectionStringsUtility.#StationDeploymentGuideUri", Justification = "Azure tables require strings")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "Open Source")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "Microsoft.Spectrum.Common.Azure.AzureLogger.#Log(System.Diagnostics.TraceEventType,Microsoft.Spectrum.Common.LoggingMessageId,System.String)", Justification = "Logger needs to catch all exceptions")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "Microsoft.Spectrum.Common.Azure.AzureLogger.#Log(System.Diagnostics.TraceEventType,Microsoft.Spectrum.Common.LoggingMessageId,System.Exception)", Justification = "Logger needs to catch all exceptions")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "Microsoft.Spectrum.Common.Azure.ConnectionStringsUtility.#EnableAsynchronousProcessing", Justification = "Don't want setting to cause crash")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Scope = "member", Target = "Microsoft.Spectrum.Common.Azure.ConnectionStringsUtility.#WorkerThreadsCount", Justification = "Don't want setting to cause crash")]
