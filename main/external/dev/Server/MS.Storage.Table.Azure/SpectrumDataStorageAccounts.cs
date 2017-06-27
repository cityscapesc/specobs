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
    using System.Text.RegularExpressions;
    using Microsoft.Spectrum.Common;
    using Microsoft.WindowsAzure.Storage.Table;    

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      MS.Storage.AzureTables
    /// Class:          SpectrumDataStorageAccounts (Entity Type:Private)
    /// Description:    SpectrumDataStorageAccounts Entity Class
    /// ----------------------------------------------------------------- 
    /// </summary>
    public class SpectrumDataStorageAccounts : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumDataStorageAccounts"/> class
        /// </summary>
        public SpectrumDataStorageAccounts()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumDataStorageAccounts"/> class
        /// </summary>
        /// <param name="regulatorId">Regulator Id</param>
        /// <param name="id">Known Frequency Band Id</param>
        public SpectrumDataStorageAccounts(string storageAccountName)
        {
            if (!Regex.IsMatch(storageAccountName, @"^[a-z0-9]{3,24}$"))
            {
                throw new ArgumentException("Storage account name should be lowercase alphanumeric characters having length 3 to 24", "storageAccountName");
            }

            this.RowKey = storageAccountName;
            this.PartitionKey = Constants.DummyPartitionKey;
        }

        /// <summary> 
        /// Gets or sets the name of the SpectrumDataStorageAccounts. 
        /// </summary> 
        public string Name 
        { 
            get
            {
                return this.RowKey;
            }
        }

        /// <summary> 
        /// Gets or sets the Uri of the SpectrumDataStorageAccounts. 
        /// </summary> 
        public string Uri { get; set; }

        /// <summary> 
        /// Gets or sets the AccountKey of the SpectrumDataStorageAccounts. 
        /// This needs to be private.
        /// </summary> 
        public string AccountKey { get; set; }

        /// <summary> 
        /// Gets or sets the StationCount of the SpectrumDataStorageAccounts. 
        /// </summary> 
        public int StationCount { get; set;  }

        /// <summary>
        /// Gets or sets the Maximum number of stations allowed on this storage account
        /// </summary>
        public int MaxStationCount { get; set; }
    }
}
