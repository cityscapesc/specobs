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

namespace Microsoft.Spectrum.MeasurementStation.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.ServiceModel;
    using System.Xml;

    public class MeasurementStationServiceChannelFactory : IDisposable
    {
        private readonly ConcurrentDictionary<string, ChannelFactory<IMeasurementStationServiceChannel>> channelFactoryCache
            = new ConcurrentDictionary<string, ChannelFactory<IMeasurementStationServiceChannel>>();

        public IMeasurementStationServiceChannel CreateChannel(string serviceUri)
        {
            if (string.IsNullOrWhiteSpace(serviceUri))
            {
                throw new ArgumentNullException("serviceUri");
            }

            var channelFactory = this.channelFactoryCache.GetOrAdd(serviceUri, this.CreateChannelFactory);
            return channelFactory.CreateChannel();
        }

        // TODO: As it stands now, the Unity container is not configured to call this method.
        public void Dispose()
        {
            foreach (var channelFactory in this.channelFactoryCache.Values)
            {
                channelFactory.Close();
            }
        }

        private ChannelFactory<IMeasurementStationServiceChannel> CreateChannelFactory(string serviceUri)
        {
            WSHttpBinding binding = new WSHttpBinding(SecurityMode.Transport, false);
            binding.CloseTimeout = TimeSpan.FromMinutes(10);
            binding.OpenTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.MaxBufferPoolSize = 524288;
            binding.MaxReceivedMessageSize = 65536;
            binding.UseDefaultWebProxy = true;
            binding.ReaderQuotas = new XmlDictionaryReaderQuotas();
            binding.ReaderQuotas.MaxDepth = 32;
            binding.ReaderQuotas.MaxStringContentLength = 8192;
            binding.ReaderQuotas.MaxArrayLength = 16384;
            binding.ReaderQuotas.MaxBytesPerRead = 4096;
            binding.ReaderQuotas.MaxNameTableCharCount = 16384;            

            EndpointAddress endpointAddress = new EndpointAddress(serviceUri);

            return new ChannelFactory<IMeasurementStationServiceChannel>(binding, endpointAddress);
        }
    }
}
