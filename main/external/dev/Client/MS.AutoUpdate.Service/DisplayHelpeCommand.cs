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

namespace Microsoft.Spectrum.AutoUpdate.Service
{
    using System;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Unity")]
    internal class DisplayHelpCommand : IExecute
    {
        public void Execute()
        {
            Console.WriteLine("Microsoft Spectrum Auto Updater Service.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("Microsoft.Spectrum.AutoUpdater.Service.exe");
            Console.WriteLine("Starts the spectrum auto updater service as a Windows service in the background");
            Console.WriteLine();
            Console.WriteLine("Microsoft.Spectrum.AutoUpdater.Service.exe RunAsExe");
            Console.WriteLine("Starts the spectrum auto updater service as a standalone executable. Press Enter to stop the executable.");
            Console.WriteLine();
        }
    }
}
