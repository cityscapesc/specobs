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

namespace Microsoft.Spectrum.Analysis.DataProcessor
{
    using System;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.SqlAzure;
    using Microsoft.Practices.Unity;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Common.Azure;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.Blob.V1;
    using Microsoft.Spectrum.Storage.Queue.Azure;
    using Microsoft.WindowsAzure;

    public class UnityDataProcessorAgentContainer : IDataProcessorAgentContainer, IDisposable
    {
        private readonly IUnityContainer container;

        public UnityDataProcessorAgentContainer()
        {
            this.container = new UnityContainer();
            this.container.AddNewExtension<CompositionExtension>();
        }

        public IDataProcessorAgent ResolveDataProcessorAgent()
        {
            return this.container.Resolve<IDataProcessorAgent>();
        }

        public void Release(object instance)
        {
            // TODO: Unity doesn't actually call Dispose on IDisposable services
            // Need to implement a custom lifetime manager to be completely correct
            this.container.Teardown(instance);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.container.Dispose();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "The type is used in the constructor")]
        private class CompositionExtension : UnityContainerExtension
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "That's just the way it is for now")]
            protected override void Initialize()
            {
                // Get all of the static configuration values
                string inputQueueName = ConnectionStringsUtility.InputQueueName;
                string outputQueueName = ConnectionStringsUtility.OutputQueueName;
                string storageAccountConnectionString = ConnectionStringsUtility.StorageAccountConnectionString;
                bool resetQueues = false;
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
                CloudBlobContainerName cloudContainerName = CloudBlobContainerName.Parse(ConnectionStringsUtility.PartialPowerSpectralDataContainer);

                var queueRetryStrategy = new FixedInterval(3, TimeSpan.FromSeconds(1));
                var queueRetryPolicy = new RetryPolicy<StorageTransientErrorDetectionStrategy>(queueRetryStrategy);

                var blobRetryStrategy = new FixedInterval(3, TimeSpan.FromSeconds(1));
                var blobRetryPolicy = new RetryPolicy<StorageTransientErrorDetectionStrategy>(blobRetryStrategy);

                // Register the WAD logger
                this.Container.RegisterType<ILogger, AzureLogger>();

                // Register the implementations for storage
                this.Container.RegisterType<IMessageQueue, AzureMessageQueue>(
                    "Input_Queue",
                    new InjectionConstructor(inputQueueName, storageAccount, resetQueues));

                this.Container.RegisterType<IMessageQueue, RetryMessageQueue>(
                    "Retry_Input_Queue",
                    new InjectionConstructor(
                        new ResolvedParameter<IMessageQueue>("Input_Queue"),
                        queueRetryPolicy));

                this.Container.RegisterType<IMessageQueue, AzureMessageQueue>(
                    "Output_Queue",
                    new InjectionConstructor(outputQueueName, storageAccount, resetQueues));

                this.Container.RegisterType<IMessageQueue, RetryMessageQueue>(
                    "Retry_Output_Queue",
                    new InjectionConstructor(
                        new ResolvedParameter<IMessageQueue>("Output_Queue"),
                        queueRetryPolicy));

                // Register the payload formatters
                this.Container.RegisterType<IPayloadXmlFormatter, AggregationRequestPayloadXmlFormatter>("AggregationRequest_Formatter");
                this.Container.RegisterType<IPayloadXmlFormatter, AggregationResponsePayloadXmlFormatter>("AggregationResponse_Formatter");
                this.Container.RegisterType<IPayloadXmlFormatter, NormalizationRequestPayloadXmlFormatter>("NormalizationRequest_Formatter");
                this.Container.RegisterType<IPayloadXmlFormatter, NormalizationResponsePayloadXmlFormatter>("NormalizationResponse_Formatter");

                this.Container.RegisterType<IQueueMessageFormatter, XmlQueueMessageFormatter>();
                this.Container.RegisterType<IBlockStorage, BlockStorage>(
                    new InjectionConstructor(
                        new ResolvedParameter<ISpectrumBlobStorage>("Retry_SpectrumBlobStorage"),
                        new ResolvedParameter<IBlockFormatter>()));

                this.Container.RegisterType<IBlockMatrixFormatter, RowMajorBlockMatrixFormatter>();
                this.Container.RegisterType<IBlockMatrixStorage, BlockMatrixStorage>(
                    new InjectionConstructor(
                        new ResolvedParameter<ISpectrumBlobStorage>("Retry_SpectrumBlobStorage"),
                        new ResolvedParameter<IBlockMatrixFormatter>()));

                this.Container.RegisterType<ISpectrumBlobStorage, AzureSpectrumBlobStorage>(
                    "Real_SpectrumBlobStorage",
                    new InjectionConstructor(
                        storageAccount,
                        cloudContainerName));

                this.Container.RegisterType<ISpectrumBlobStorage, RetrySpectrumBlobStorage>(
                    "Retry_SpectrumBlobStorage",
                    new InjectionConstructor(
                        new ResolvedParameter<ISpectrumBlobStorage>("Real_SpectrumBlobStorage"),
                        blobRetryPolicy));

                // Register all data processors
                this.Container.RegisterType<IDataProcessor, NormalizationDataProcessor>(
                    typeof(NormalizationDataProcessor).Name,
                    new InjectionConstructor(
                        new ResolvedParameter<IBlockStorage>(),
                        new ResolvedParameter<IBlockMatrixStorage>(),
                        new ResolvedParameter<ILogger>()));

                this.Container.RegisterType<IDataProcessor, AggregationDataProcessor>(
                    typeof(AggregationDataProcessor).Name,
                    new InjectionConstructor(
                        new ResolvedParameter<IBlockMatrixStorage>(),
                        new ResolvedParameter<ILogger>()));

                // Register the root data processor agent
                this.Container.RegisterType<IDataProcessorAgent, DataProcessorAgent>(
                    new InjectionConstructor(
                        new ResolvedParameter<IMessageQueue>("Retry_Input_Queue"),
                        new ResolvedParameter<IMessageQueue>("Retry_Output_Queue"),
                        new ResolvedParameter<IQueueMessageFormatter>(),
                        new ResolvedParameter<IDataProcessor[]>(), // NB: This will return all of the named registered implementations for IDataProcessor as an array
                        new ResolvedParameter<ILogger>()));
            }
        }
    }
}
