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

namespace Microsoft.Spectrum.Storage.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage
    /// Class:          IPortalTableOperations
    /// Description:    interface for <see cref="IPortalTableOperations"/> class
   public interface IPortalTableOperations
    {
        /// <summary>
        /// Gets list of 
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
       IEnumerable<Question> GetQuestionsBySection(string sectionName);

       /// <summary>
       /// Save feedback
       /// </summary>
       /// <param name="feedback">feedback</param>
       void SaveFeedback(Feedback feedback);

       /// <summary>
       /// Save problem report
       /// </summary>
       /// <param name="issue">issue report</param>
       void SaveIssueReport(IssueReport issue);

       /// <summary>
       /// Gets all issues last reported from given time
       /// </summary>
       /// <param name="stationId">Station Id</param>
       /// <param name="lastReportedTime">last reported time</param>
       /// <returns></returns>
       IEnumerable<IssueReport> GetIssuesReported(Guid stationId, DateTime lastReportedTime);
    }
}
