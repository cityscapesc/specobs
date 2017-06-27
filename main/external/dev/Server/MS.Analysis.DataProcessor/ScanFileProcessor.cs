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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Spectrum.Analysis.DataProcessor.Rules;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.IO.ScanFile;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Enums;
    using Microsoft.Spectrum.Storage.Models;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using Storage;

    public class ScanFileProcessor
    {
        private readonly SpectrumDataProcessorStorage spectrumDataProcessorStorage;
        private readonly ILogger logger;
        private IEnumerable<AggregationRule> aggregationRules;

        public ScanFileProcessor(SpectrumDataProcessorStorage spectrumDataProcessorStorage, IEnumerable<AggregationRule> aggregationRules, ILogger logger)
        {
            if (spectrumDataProcessorStorage == null)
            {
                throw new ArgumentNullException("spectrumDataProcessorStorage");
            }

            if (aggregationRules == null)
            {
                throw new ArgumentNullException("aggregationRules");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.spectrumDataProcessorStorage = spectrumDataProcessorStorage;
            this.aggregationRules = aggregationRules;
            this.logger = logger;
        }

        public void Process(ScanFile scanFile, string measurementStationKey)
        {
            if (scanFile == null)
            {
                throw new ArgumentException("Scan file can not be null", "scanFile");
            }

            ConfigDataBlock configDataBlock = scanFile.Config;
            List<SpectralPsdDataBlock> spectralPsdData = scanFile.SpectralPsdData;

            Stopwatch stopWatch = new Stopwatch();

            this.CheckForMeasurementStationConfigUpdates(configDataBlock, measurementStationKey);

            foreach (AggregationRule rule in this.aggregationRules)
            {
                stopWatch.Restart();

                rule.Run(spectralPsdData, Guid.Parse(measurementStationKey));

                stopWatch.Stop();

                string information = string.Format(CultureInfo.InvariantCulture, "Measurement Station:{0} | Rule :{1} | Time taken to process: {2} seconds", measurementStationKey, rule.ToString(), stopWatch.Elapsed.TotalSeconds);
                this.logger.Log(TraceEventType.Information, LoggingMessageId.DataProcessorAgentId, information);
            }
        }

        public async Task ProcessAsync(ScanFile scanFile, string measurementStationKey)
        {
            ConfigDataBlock configDataBlock = scanFile.Config;
            List<SpectralPsdDataBlock> spectralPsdData = scanFile.SpectralPsdData;

            this.CheckForMeasurementStationConfigUpdates(configDataBlock, measurementStationKey);

            IEnumerable<Task> aggreagationRuleSet = this.aggregationRules.Select(rule => rule.RunAsync(spectralPsdData, Guid.Parse(measurementStationKey)));

            await Task.WhenAll(aggreagationRuleSet);
        }

        public void UpdateScanFileInformation(Guid measurementStationId, ConfigDataBlock configDataBlock, string blobUri, FileType scanFileType)
        {
            if (configDataBlock == null)
            {
                throw new ArgumentException("Config data block can not be null", "configDataBlock");
            }

            DateTime timeStart = configDataBlock.Timestamp;

            // TODO: We will need to add this as a parameter that we pass in. For now, we can just leave this as hard coded though since we only have one type 
            // and there is no reason to pass extra information over the wire if it isn’t necessary. If we add another compression type in the future, then we 
            // will need to add this parameter.
            ScanFileInformation scanFileInformation;

            if (scanFileType == FileType.RawIqFile)
            {
                scanFileInformation = new ScanFileInformation(measurementStationId, timeStart, (int)FileCompressionType.Deflate, scanFileType, blobUri, configDataBlock.EndToEndConfiguration.RawIqConfiguration.StartFrequencyHz, configDataBlock.EndToEndConfiguration.RawIqConfiguration.StopFrequencyHz);
            }
            else
            {
                double lowestStartFrequency = double.MaxValue;
                double highestEndFrequency = 0;
                foreach (var rfSensor in configDataBlock.EndToEndConfiguration.RFSensorConfigurations)
                {
                    if (lowestStartFrequency > rfSensor.CurrentStartFrequencyHz)
                    {
                        lowestStartFrequency = rfSensor.CurrentStartFrequencyHz;
                    }

                    if (highestEndFrequency < rfSensor.CurrentStopFrequencyHz)
                    {
                        highestEndFrequency = rfSensor.CurrentStopFrequencyHz;
                    }
                }

                scanFileInformation = new ScanFileInformation(measurementStationId, timeStart, (int)FileCompressionType.Deflate, scanFileType, blobUri, lowestStartFrequency, highestEndFrequency);
            }

            this.spectrumDataProcessorStorage.InsertOrUpdateScanFileInformation(scanFileInformation);
        }

        private void CheckForMeasurementStationConfigUpdates(ConfigDataBlock configDataBlock, string measurementStationKey)
        {
            Guid measurementStationId = Guid.Parse(measurementStationKey);
            MeasurementStationInfo measurementStation = GlobalCache.Instance.MeasurementStationManager.GetMeasurementStationPublic(measurementStationId);

            if (measurementStation == null)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, "Invalid Measurement StationKey:{0}", measurementStationKey);
                throw new ArgumentException(errorMessage, "measurementStationKey");
            }

            if (!measurementStation.PSDDataAvailabilityStartDate.HasValue)
            {
                IEnumerable<RawSpectralDataInfo> spectralDataSchema = this.spectrumDataProcessorStorage.GetRawSpectralDataSchema(measurementStationId, FileType.ScanFile);
                measurementStation.PSDDataAvailabilityStartDate = configDataBlock.Timestamp;

                if (spectralDataSchema != null
                    && spectralDataSchema.Any())
                {
                    measurementStation.PSDDataAvailabilityStartDate = spectralDataSchema.FirstOrDefault().TimeStart;
                }
            }

            GlobalCache.Instance.MeasurementStationManager.UpdatePSDDataAvailabilityTimestamp(measurementStationId, measurementStation.PSDDataAvailabilityStartDate.Value, configDataBlock.Timestamp);

            if (configDataBlock != null)
            {
                if (measurementStation.DeviceDescription.ClientEndToEndConfiguration.LastModifiedTime < configDataBlock.EndToEndConfiguration.LastModifiedTime)
                {
                    // Update the entire client end to end configuration
                    string information = string.Format(CultureInfo.InvariantCulture, "Found a new update in configuration {0} for the Measurement Station Key", measurementStationKey);
                    this.logger.Log(TraceEventType.Information, LoggingMessageId.AggregatingDataProcessorEventId, information);

                    measurementStation.DeviceDescription.ClientEndToEndConfiguration = configDataBlock.EndToEndConfiguration;
                    measurementStation.DeviceDescription.HardwareInformation = configDataBlock.HardwareInformation;
                    GlobalCache.Instance.MeasurementStationManager.UpdateStationData(measurementStation);
                }
                else if (configDataBlock.HardwareInformation != measurementStation.DeviceDescription.HardwareInformation)
                {
                    // Just update the hardware information string
                    GlobalCache.Instance.MeasurementStationManager.UpdateStationHardwareInformation(measurementStation.Identifier.Id, configDataBlock.HardwareInformation);
                }
            }
            else
            {
                this.logger.Log(TraceEventType.Information, LoggingMessageId.AggregatingDataProcessorEventId, string.Format(CultureInfo.InvariantCulture, "Configuration data block for measurement station {0} is missing", measurementStationKey));
            }
        }
    }
}
