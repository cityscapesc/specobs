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
    using System.ServiceModel.Activation;

    public class MeasurementStationServiceHostFactory : ServiceHostFactory
    {
        private readonly IMeasurementStationServiceContainer container;

        public MeasurementStationServiceHostFactory()
        {
            this.container = new UnityMeasurementStationServiceContainer();
        }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            if (serviceType == typeof(MeasurementStationService))
            {
                return new MeasurementStationServiceHost(this.container, serviceType, baseAddresses);
            }

            return base.CreateServiceHost(serviceType, baseAddresses);
        }
    }
}
