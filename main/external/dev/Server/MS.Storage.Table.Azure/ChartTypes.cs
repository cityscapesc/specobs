﻿// Copyright (c) Microsoft Corporation
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
    /// Class:          ChartTypes (Entity Type:Private)
    /// Description:    ChartTypes Entity Class
    /// -----------------------------------------------------------------  
    /// </summary>
    public class ChartTypes : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartTypes"/> class
        /// </summary>
        public ChartTypes()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartTypes"/> class
        /// </summary>
        /// <param name="name">Name</param>
        public ChartTypes(string name)
        {
            this.PartitionKey = Constants.DummyPartitionKey;
            this.RowKey = name;
        }

        /// <summary> 
        /// Gets the Name of the ChartTypes. 
        /// </summary>
        public string Name
        {
            get
            {
                return this.RowKey;
            }
        }
    }
}
