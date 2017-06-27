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

namespace Microsoft.Spectrum.MeasurementStation.Service
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    /// <summary>
    /// Provides a ServiceHost implementation that automatically adds an additional behavior to provide service
    /// instances by resolving them through the container.
    /// </summary>
    public class MeasurementStationServiceHost : ServiceHost
    {
        private readonly IMeasurementStationServiceContainer container;

        public MeasurementStationServiceHost(IMeasurementStationServiceContainer container, Type serviceType, Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this.container = container;

            // Add the instance provider behavior to each contract
            foreach (ContractDescription contract in this.ImplementedContracts.Values)
            {
                contract.Behaviors.Add(new MeasurementStationServiceInstanceProvider(this.container));
            }
        }
    }
}
