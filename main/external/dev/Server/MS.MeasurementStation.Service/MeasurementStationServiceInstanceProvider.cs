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
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    /// <summary>
    /// Provides a behavior that resolves instances of the service through the container.
    /// </summary>
    public class MeasurementStationServiceInstanceProvider : IInstanceProvider, IContractBehavior
    {
        private readonly IMeasurementStationServiceContainer container;

        public MeasurementStationServiceInstanceProvider(IMeasurementStationServiceContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            this.container = container;
        }

        #region IInstanceProvider Methods

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.GetInstance(instanceContext);
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return this.container.ResolveMeasurementStationService();
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            this.container.Release(instance);
        }

        #endregion

        #region IContractBehavior Methods

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            // Nothing to do here
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            // Nothing to do here
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            if (dispatchRuntime != null)
            {
                // Use this object instance as the instance provider
                dispatchRuntime.InstanceProvider = this;
            }
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
            // Nothing to do here
            // Implement to confirm that the contract and endpoint can support the contract behavior.
        }

        #endregion
    }
}
