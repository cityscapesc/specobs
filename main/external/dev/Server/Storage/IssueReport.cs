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

namespace Microsoft.Spectrum.Storage
{
    using System;
    using System.Globalization;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      MS.Storage.AzureTables
    /// Class:          IssueReports (Entity Type:Public)
    /// Description:    IssueReports Entity Class
    /// -----------------------------------------------------------------  
    /// </summary>
    /// 
    public class IssueReport
    {
        public IssueReport()
        {
        }

        public IssueReport(Guid stationId, DateTime reportDate, string firstName, string lastName, string email, string phone, string subject, string issueDescription)
        {
            this.StationId = stationId;
            this.ReportDate = reportDate;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Email = email;
            this.Phone = phone;
            this.Subject = subject;
            this.IssueDescription = issueDescription;
        }

        public Guid StationId { get; set; }

        public DateTime ReportDate { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Subject { get; set; }

        public string IssueDescription { get; set; }
    }
}
