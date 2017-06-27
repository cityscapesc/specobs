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
    /// Class:          Questions (Entity Type:Private)
    /// Description:    Questions Entity Class
    /// -----------------------------------------------------------------
    public class Questions : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the Questions class
        /// </summary>
        public Questions()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        /// <param name="ask"></param>
        /// <param name="answer"></param>
        /// <param name="order"></param>
        public Questions(string section, string ask, string answer, int order)
        {
            this.PartitionKey = section;
            this.RowKey = Guid.NewGuid().ToString();
            this.Ask = ask;
            this.Answer = answer;
            this.Order = order;
        }

        /// <summary>
        /// FAQs section
        /// </summary>
        public string Section
        {
            get
            {
                return this.PartitionKey;
            }
        }

        /// <summary>
        /// Qustion
        /// </summary>
        public string Ask { get; set; }

        /// <summary>
        /// Answer
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Order of the Questions in this section (we sort from lowest to highest)
        /// </summary>
        public int Order { get; set; }
    }
}
