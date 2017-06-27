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

namespace Microsoft.Spectrum.Storage.Table.Azure.Helpers
{
    using System;
    using System.Globalization;
    using Microsoft.Spectrum.Common;

    public static class AzureTableHelper
    {
        public const string MeasurementStationsPublicHistoricalTable = "MeasurementStationsPublicHistorical";

        public const string MeasurementStationsPublicTable = "MeasurementStationsPublic";

        public const string RawSpectralDataSchemaTable = "RawSpectralDataSchema";

        public const string MeasurementStationsPrivateTable = "MeasurementStationsPrivate";

        public const string ScheduledItemsTable = "ScheduledItems";

        public const string SpectrumDataStorageAccountsTable = "SpectrumDataStorageAccounts";

        public const string SettingsTable = "Settings";

        public const string SpectrumFileProcessingFailuresTable = "SpectrumFileProcessingFailures";

        public const string IssueReports = "IssueReports";

        public const string WadLogsTable = "WADLogsTable";

        public const string RawIQScanPolicyTable = "RawIQScanPolicy";

        /// <summary>
        /// users table
        /// </summary>
        public const string UsersTable = "Users";

        /// <summary>
        /// user role table
        /// </summary>
        public const string UserRoleTable = "Userrole";

        /// <summary>
        /// membership info table
        /// </summary>
        public const string WebpagesOAuthMembershipTable = "WebpagesOAuthMembership";

        /// <summary>
        /// Faqs table
        /// </summary>
        public const string QuestionsTable = "Questions";

        /// <summary>
        /// Feedbacks table
        /// </summary>
        public const string FeedbacksTable = "Feedbacks";

        public const string IssueReportTable = "IssueReports";

        public const string TableSharedAccessSignaturesTable = "TableSharedAccessSignatures";
        
        private const string SpectralDataTablePrefix = "SpectralDataPartition";        

        public static string GetSpectralDataTableName(Guid measurementStationKey, TimeRangeKind timeRangeKind)
        {
            string stationKey = measurementStationKey.ToString().Replace("-", string.Empty);
            string timeRangeKindText = timeRangeKind.ToString();

            string spectralDataTableName = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", timeRangeKindText, SpectralDataTablePrefix, stationKey);

            return spectralDataTableName;
        }

        public static bool IsPrivateTable(string tableName)
        {
            if ((tableName == AzureTableHelper.ScheduledItemsTable)
                || (tableName == AzureTableHelper.SettingsTable)
                || (tableName == AzureTableHelper.SpectrumDataStorageAccountsTable)
                || (tableName == AzureTableHelper.UserRoleTable)
                || (tableName == AzureTableHelper.UsersTable)
                || (tableName == AzureTableHelper.MeasurementStationsPrivateTable)
                || (tableName == AzureTableHelper.FeedbacksTable)
                || (tableName == AzureTableHelper.QuestionsTable)
                || (tableName == AzureTableHelper.WebpagesOAuthMembershipTable)
                || (tableName == AzureTableHelper.IssueReports)
                || (tableName == AzureTableHelper.WadLogsTable))
            {
                return true;
            }

            return false;
        }
    }
}
