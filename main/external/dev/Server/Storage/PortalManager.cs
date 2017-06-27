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
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Storage.DataContracts;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage
    /// Class:          CommonManager
    /// Description:    Operations required to work with faqs, feedback etc
    /// ----------------------------------------------------------------- 
    /// </summary>
    public class PortalManager
    {
        private readonly IPortalTableOperations commonTableOperations;

        public PortalManager(IPortalTableOperations commonTableOperations)
        {
            Check.IsNotNull(commonTableOperations, "Common Table Operations");

            this.commonTableOperations = commonTableOperations;
        }

        /// <summary>
        /// Gets list of questions by section name
        /// </summary>
        /// <param name="section">section name</param>
        /// <returns>list of questions</returns>
        public IEnumerable<Question> GetFrequentQuestionsAsync(string section)
        {
            return this.commonTableOperations.GetQuestionsBySection(section);
        }

        /// <summary>
        /// Save feedback
        /// </summary>
        /// <param name="feedback">feedback object</param>
        public void SaveFeedback(Feedback feedback)
        {
            this.commonTableOperations.SaveFeedback(feedback);
        }
    }
}
