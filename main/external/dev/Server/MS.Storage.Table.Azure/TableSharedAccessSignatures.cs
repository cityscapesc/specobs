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
    using Microsoft.Spectrum.Common;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      MS.Storage.AzureTables
    /// Class:          TableSharedAccessSignatures (Entity Type:Private)
    /// Description:    TableSharedAccessSignatures Entity Class
    /// ----------------------------------------------------------------- 
    /// </summary>
    public class TableSharedAccessSignatures : TableEntity
    {
        public TableSharedAccessSignatures() 
        { 
        }

        public TableSharedAccessSignatures(string storageAccountName, string tableName, string sharedAccessSignature)
        {
            this.PartitionKey = storageAccountName;
            this.RowKey = tableName;
            this.SharedAccessSignature = sharedAccessSignature;
        }

        public string StorageAccountName
        {
            get
            {
                return this.PartitionKey;
            }
        }

        public string TableName
        {
            get
            {
                return this.RowKey;
            }
        }

        public string SharedAccessSignature { get; set; }
    }
}
