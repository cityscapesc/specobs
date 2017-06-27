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
    using System.ServiceProcess;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Unity")]
    internal class RunAsServiceCommand : IExecute
    {
        private readonly ServiceBase serviceBase;

        public RunAsServiceCommand(ServiceBase serviceBase)
        {
            if (serviceBase == null)
            {
                throw new ArgumentNullException("serviceBase");
            }

            this.serviceBase = serviceBase;
        }

        public void Execute()
        {
            ServiceBase.Run(this.serviceBase);
        }
    }
}
