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

namespace Microsoft.Spectrum.Storage.Table.Azure
{
    using System;
    using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
    using Microsoft.WindowsAzure.Storage.Table;
    using CloudStorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount;

    public class AzureTableDbContext
    {
        private readonly CloudStorageAccount storageAccount;
        private readonly RetryPolicy retryPolicy;

        public AzureTableDbContext(CloudStorageAccount cloudStorageAccount, RetryPolicy retryPolicy)
        {
            if (cloudStorageAccount == null)
            {
                throw new ArgumentNullException("cloudStorageAccount");
            }

            if (retryPolicy == null)
            {
                throw new ArgumentNullException("retryPolicy");
            }

            this.storageAccount = cloudStorageAccount;
            this.retryPolicy = retryPolicy;
        }

        public RetryAzureTableOperations<MeasurementStationsPublic> PublicMeasurementStationsOperations
        {
            get
            {
                return this.GetOperationContext<MeasurementStationsPublic>();
            }
        }
                
        public RetryAzureTableOperations<SpectralDataSchema> SpectralDataOperations
        {
            get
            {                
                return this.GetOperationContext<SpectralDataSchema>();
            }
        }

        public RetryAzureTableOperations<MeasurementStationsPublicHistorical> PublicMeasurementStationHistoricalDataOperations
        {
            get
            {
                return this.GetOperationContext<MeasurementStationsPublicHistorical>();
            }
        }

        public RetryAzureTableOperations<RawSpectralDataSchema> RawSpectralDataSchemaOperations
        {
            get
            {
                return this.GetOperationContext<RawSpectralDataSchema>();
            }
        }

        public RetryAzureTableOperations<MeasurementStationsPrivate> MeasurementStationPrivateOperations
        {
            get
            {
                return this.GetOperationContext<MeasurementStationsPrivate>();
            }
        }

        public RetryAzureTableOperations<UserRoles> UserRoleTableOperations
        {
            get
            {
                return this.GetOperationContext<UserRoles>();
            }
        }

        public RetryAzureTableOperations<Users> UserTableOperations
        {
            get
            {
                return this.GetOperationContext<Users>();
            }
        }

        public RetryAzureTableOperations<WebpagesOAuthMembership> OAuthMembershipTableOperations
        {
            get
            {
                return this.GetOperationContext<WebpagesOAuthMembership>();
            }
        }

        public RetryAzureTableOperations<SpectrumDataStorageAccounts> SpectrumDataStorageAccountsOperations
        {
            get
            {
                return this.GetOperationContext<SpectrumDataStorageAccounts>();
            }
        }

        public RetryAzureTableOperations<Settings> SettingsOperations
        {
            get
            {
                return this.GetOperationContext<Settings>();
            }
        }

        public RetryAzureTableOperations<Questions> QuestionsTableOperations
        {
            get
            {
                return this.GetOperationContext<Questions>();
            }
        }

        public RetryAzureTableOperations<Feedbacks> FeedbacksTableOperations
        {
            get
            {
                return this.GetOperationContext<Feedbacks>();
            }
        }

        public RetryAzureTableOperations<IssueReports> IssueReportsTableOperations
        {
            get
            {
                return this.GetOperationContext<IssueReports>();
            }
        }

        public RetryAzureTableOperations<TableSharedAccessSignatures> TableSharedAccessSignaturesTableOperations
        {
            get
            {
                return this.GetOperationContext<TableSharedAccessSignatures>();
            }
        }

        public RetryAzureTableOperations<SpectrumFileProcessingFailures> SpectrumFileProcessingFailuresTableOperations
        {
            get
            {
                return this.GetOperationContext<SpectrumFileProcessingFailures>();
            }
        }

        public RetryAzureTableOperations<RawIQScanPolicy> RawIQScanPolicyTableOperations
        {
            get
            {
                return this.GetOperationContext<RawIQScanPolicy>();
            }
        }

        private RetryAzureTableOperations<T> GetOperationContext<T>() where T : TableEntity
        {
            AzureTableOperations<T> operationContext = new AzureTableOperations<T>(this.storageAccount);
            RetryAzureTableOperations<T> retryOperationContext = new RetryAzureTableOperations<T>(this.retryPolicy, operationContext);

            return retryOperationContext;
        }
    }
}