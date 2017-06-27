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
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      MS.Storage.AzureTables
    /// Class:          Settings (Entity Type:Private)
    /// Description:    Settings Entity Class
    /// ----------------------------------------------------------------- 
    /// </summary>
    public class Settings : TableEntity
    {
        public Settings() 
        { 
        }

        public Settings(string category, string name, string value)
        {
            this.PartitionKey = category;
            this.RowKey = name;
            this.Value = value;
        }

        public string Category 
        { 
            get
            {
                return this.PartitionKey;
            }
        }

        public string Name
        {
            get
            {
                return this.RowKey;
            }
        }

        public string Value { get; set; }
    }
}
