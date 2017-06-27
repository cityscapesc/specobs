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

namespace Microsoft.Spectrum.MeasurementStation.Contract
{
    using System;
    using System.ServiceModel;

    [ServiceContract(SessionMode = SessionMode.NotAllowed)]
    public interface IMeasurementStationService
    {
        [OperationContract]
        void GetUpdatedSettings(string measurementStationAccessId, out string storageAccountName, out string storageAccessKey, out byte[] measurementStationConfiguration);

        [OperationContract]
        void ScanFileUploaded(string measurementStationAccessId, string blobUri, bool success);

        [OperationContract]
        void ReportHealthStatus(string measurementStationAccessId, int healthStatus, int messagePriority, string title, string description, DateTime occurredAt);

        [OperationContract]
        int GetHealthStatusCheckInterval(string meaurementStationAccessId);

        [OperationContract]
        int GetStationAvailability(string measurementStationAccessId);
    }
}
