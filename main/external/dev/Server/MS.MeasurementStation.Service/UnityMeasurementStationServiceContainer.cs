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
    using Common.Enums;
    using Microsoft.Practices.Unity;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.Spectrum.MeasurementStation.Contract;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Storage.Queue.Azure;

    public class UnityMeasurementStationServiceContainer : IMeasurementStationServiceContainer
    {
        private readonly IUnityContainer container;

        public UnityMeasurementStationServiceContainer()
        {
            this.container = new UnityContainer();
            this.container.AddNewExtension<CompositionExtension>();
        }

        public IMeasurementStationService ResolveMeasurementStationService()
        {
            return this.container.Resolve<IMeasurementStationService>();
        }

        public void Release(object instance)
        {
            // TODO: Unity doesn't actually call Dispose on IDisposable services
            // Need to implement a custom lifetime manager to be completely correct
            this.container.Teardown(instance);
        }

        private class CompositionExtension : UnityContainerExtension
        {
            protected override void Initialize()
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionStringsUtility.StorageAccountConnectionString);

                CloudBlobContainerName queueName = CloudBlobContainerName.Parse(ConnectionStringsUtility.WorkerQueueName);

                string[] topics = new string[3] { MessagePriority.High.ToString(), MessagePriority.Medium.ToString(), MessagePriority.Low.ToString() };
                AzureServiceBusQueue azureServiceBusQueue = new AzureServiceBusQueue(ConnectionStringsUtility.HealthReportServiceBusConnectionString, topics, ConnectionStringsUtility.HealthReportMessagingSubscription);
                this.Container.RegisterType<IMeasurementStationService, MeasurementStationService>(new InjectionConstructor(storageAccount, queueName.Value, azureServiceBusQueue));
            }
        }
    }
}
