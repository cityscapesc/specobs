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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Table;    
    
    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      MS.Storage.AzureTables
    /// Class:          ApprovalStatus (Entity Type:Private)
    /// Description:    ApprovalStatus Entity Class
    /// -----------------------------------------------------------------
    /// </summary>
    public class ApprovalStatus : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApprovalStatus"/> class
        /// </summary>      
        public ApprovalStatus()
        {
        }

       /// <summary>
        /// Initializes a new instance of the <see cref="ApprovalStatus"/> class
       /// </summary>
        /// <param name="approvalCategory">Approval Category</param>
        /// <param name="id">Id</param>
        public ApprovalStatus(string approvalCategory, int id)
        {
            this.PartitionKey = approvalCategory;
            this.RowKey = id.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary> 
        /// Gets the Id of the ApprovalStatus. 
        /// </summary> 
        public int Id
        {
            get
            {
                return int.Parse(this.RowKey, CultureInfo.InvariantCulture);
            }
        }

        /// <summary> 
        /// Gets the ApprovalCategory of the ApprovalStatus. 
        /// </summary> 
        public string ApprovalCategory
        {
            get
            {
                return this.PartitionKey;
            }
        }

        /// <summary> 
        /// Gets or sets the Name of the ApprovalStatus. 
        /// </summary> 
        public string Name { get; set; }

        /// <summary> 
        /// Gets or sets the Description of the ApprovalStatus. 
        /// </summary> 
        public string Description { get; set; }        
    }
}
