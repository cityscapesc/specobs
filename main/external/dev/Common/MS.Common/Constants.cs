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

namespace Microsoft.Spectrum.Common
{
    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Common
    /// Class:          Constants 
    /// Description:    Constants Class
    /// -----------------------------------------------------------------   
    /// </summary>    
    public static class Constants
    {
        /// <summary>
        /// Dummy Partion Key
        /// </summary>
        public const string DummyPartitionKey = "1";

        /// <summary>
        /// Partition Key
        /// </summary>
        public const string PartitionKey = "PartitionKey";

        /// <summary>
        /// Frequency
        /// </summary>
        public const string Frequency = "Frequency";

        /// <summary>
        /// Row Key
        /// </summary>
        public const string RowKey = "RowKey";

        /// <summary>
        /// Nineteen Digit
        /// </summary>
        public const string NineteenDigit = "d19";

        /// <summary>
        /// Eleven Digit
        /// </summary>
        public const string ElevenDigit = "d11";

        /// <summary>
        /// Ascii Sixteen
        /// </summary>
        public const string AsciiSixteen = "x16";

        /// <summary>
        /// Ascii Nine
        /// </summary>
        public const string AsciiNine = "x9";

        /// <summary>
        /// Site administrator partition key.
        /// <remarks>Idea behind this is to avoid data redundancy that could occur by having multiple entries in the UserRoles table for the Site Administrators
        /// by having entry for each measurement station. Easy way to overcome this issue is to have a common key which represent Site administrator entry just 
        /// saying "AllStations", so now we can have only one entry per Site Administrator in the UserRole Table.
        /// </remarks>
        /// </summary>
        public const string SiteAdminsPartitionKey = "AllStations";

        /// <summary>
        /// Page size for pagination
        /// </summary>
        public const int PageSize = 8;


        public const string DefaultRawIQFrequencyPartition = "DefaultFrequency";


        public const string DefaultRawIQCategoryRow = "DefaultBand";
    }
}
