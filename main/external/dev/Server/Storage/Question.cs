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
    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Storage
    /// Class:          Question (Model Type:public)
    /// Description:    Question Model
    /// -----------------------------------------------------------------
    /// </summary>    
    public class Question
    {
        public Question(string section, string ask, string answer, int order)
        {
            this.Section = section;
            this.Ask = ask;
            this.Answer = answer;
            this.Order = order;
        }

        /// <summary>
        /// FAQs section
        /// </summary>
        public string Section { get; private set; }

        /// <summary>
        /// Qustion
        /// </summary>
        public string Ask { get; private set; }

        /// <summary>
        /// Answer
        /// </summary>
        public string Answer { get; private set; }

        /// <summary>
        /// Order
        /// </summary>
        public int Order { get; private set; }
    }
}
