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
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Table.Azure.Helpers;
    using Microsoft.WindowsAzure.Storage.Table;    

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage.Table.Azure
    /// Class:          CommonTableOperations
    /// Description:    class containing operations to deal with common tables like faqs, feedback etc.
    /// ----------------------------------------------------------------- 
    public class PortalTableOperations : IPortalTableOperations
    {
        private readonly RetryAzureTableOperations<Questions> questionTableOperations;

        private readonly RetryAzureTableOperations<Feedbacks> feedbackTableOperations;

        private readonly RetryAzureTableOperations<IssueReports> isuueReportTableOperations;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalTableOperations"/> class
        /// </summary>
        /// <param name="dataContext">data context containing table references</param>
        public PortalTableOperations(AzureTableDbContext dataContext)
        {
            if (dataContext == null)
            {
                throw new ArgumentNullException("dataContext");
            }

            this.questionTableOperations = dataContext.QuestionsTableOperations;
            this.questionTableOperations.GetTableReference(AzureTableHelper.QuestionsTable);

            this.feedbackTableOperations = dataContext.FeedbacksTableOperations;
            this.feedbackTableOperations.GetTableReference(AzureTableHelper.FeedbacksTable);

            this.isuueReportTableOperations = dataContext.IssueReportsTableOperations;
            this.isuueReportTableOperations.GetTableReference(AzureTableHelper.IssueReportTable);
        }

        /// <summary>
        /// Get all questions in a section
        /// </summary>
        /// <param name="sectionName">section name</param>
        /// <returns>list of questions</returns>
        public IEnumerable<Question> GetQuestionsBySection(string sectionName)
        {
            Check.IsNotEmptyOrWhiteSpace(sectionName, "Section Name");

            IEnumerable<Questions> questionEntities = this.questionTableOperations.GetByKeys<Questions>(sectionName);

            List<Question> questions = new List<Question>();

            foreach (Questions entity in questionEntities)
            {
                questions.Add(new Question(entity.Section, entity.Ask, entity.Answer, entity.Order));
            }

            return questions.OrderBy(question => question.Order);
        }

        /// <summary>
        /// Save feedback
        /// </summary>
        /// <param name="feedback">feedback</param>
        public void SaveFeedback(Feedback feedback)
        {
            if (feedback == null)
            {
                throw new ArgumentNullException("feedback");
            }

            Feedbacks entity = new Feedbacks(DateTime.UtcNow)
            {
                FirstName = feedback.FirstName,
                LastName = feedback.LastName,
                Email = feedback.Email,
                Phone = feedback.Phone,
                Subject = feedback.Subject,
                Comment = feedback.Comment
            };

            this.feedbackTableOperations.InsertEntity(entity);
        }

        /// <summary>
        /// Saves issue reported
        /// </summary>
        /// <param name="issue">issue report</param>
        public void SaveIssueReport(IssueReport issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException("issue");
            }

            IssueReports report = new IssueReports(issue.StationId, issue.ReportDate, issue.FirstName, issue.LastName, issue.Email, issue.Phone, issue.Subject, issue.IssueDescription);

            this.isuueReportTableOperations.InsertEntity(report);
        }

        public IEnumerable<IssueReport> GetIssuesReported(Guid stationId, DateTime lastReportedTime)
        {
            string startTime = lastReportedTime.ToUniversalTime().Ticks.ToString(Constants.NineteenDigit, CultureInfo.InvariantCulture);
            string partionKeyQuery = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, stationId.ToString());
            string rowKeyQuery = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, startTime);
            string query = TableQuery.CombineFilters(partionKeyQuery, TableOperators.And, rowKeyQuery);

            return this.isuueReportTableOperations.ExecuteQueryWithContinuation<IssueReports>(query)
                                                      .Select(x => new IssueReport
                   {
                       FirstName = x.FirstName,
                       LastName = x.LastName,
                       Email = x.Email,
                       Phone = x.Phone,
                       StationId = new Guid(x.PartitionKey),
                       Subject = x.Subject,
                       IssueDescription = x.IssueDescription
                   });
        }
    }
}
