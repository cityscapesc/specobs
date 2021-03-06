// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Scope = "member", Target = "Microsoft.Spectrum.Storage.Queue.Azure.WorkerQueueMessage.#BlobUri", Justification = "The Azure table only support string types")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "1#", Scope = "member", Target = "Microsoft.Spectrum.Storage.Queue.Azure.WorkerQueueMessage.#.ctor(System.String,System.String,System.Boolean)", Justification = "The Azure table only support string types")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Scope = "member", Target = "Microsoft.Spectrum.Storage.Queue.Azure.WorkerQueueMessage.#.ctor(Microsoft.Spectrum.Storage.Queue.Azure.MessageTypes,System.String,System.String,System.Boolean)", Justification = "azure tables require strings")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Scope = "member", Target = "Microsoft.Spectrum.Storage.Queue.Azure.WorkerQueueMessage.#.ctor(Microsoft.Spectrum.Storage.Queue.Azure.MessageType,System.String,System.String,System.Boolean)", Justification = "azure tables require strings")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "Microsoft.Spectrum.Storage.Queue.Azure.RetryMessageQueue", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "Microsoft.Spectrum.Storage.Queue.Azure.IMessageQueue", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Scope = "type", Target = "Microsoft.Spectrum.Storage.Queue.Azure.AzureMessageQueue", Justification = "Style Issue")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "Open source")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant", Justification = "Not going to make these cls compliant")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "queueClient", Scope = "member", Target = "Microsoft.Spectrum.Storage.Queue.Azure.AzureMessageQueue.#SetReferenceToQueue(System.String,System.Boolean)", Justification = "Style Issue")]
