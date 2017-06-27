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

namespace Microsoft.Spectrum.Common.Azure
{
    using System;
    using System.Linq;

    public class CloudBlobContainerName
    {
        public CloudBlobContainerName(string validBlobContainerName)
        {
            this.Value = validBlobContainerName;
        }

        public string Value { get; private set; }

        public static CloudBlobContainerName Parse(string blobContainerName)
        {
            CloudBlobContainerName name;

            if (!TryParse(blobContainerName, out name))
            {
                throw new InvalidOperationException("The specific blob container name has some invalid characters.");
            }

            return name;
        }

        public static bool TryParse(string blobContainerName, out CloudBlobContainerName name)
        {
            if (IsValidBlobContainerName(blobContainerName))
            {
                name = new CloudBlobContainerName(blobContainerName);
                return true;
            }

            name = null;
            return false;
        }

        public override string ToString()
        {
            return this.Value;
        }

        private static bool IsValidBlobContainerName(string blobContainerName)
        {
            // NB: The name of the container must be of a certain format:
            // - 3 to 63 characters long
            // - all lowercase
            // - must start with a letter or number, can only contain letters, numbers, and dashes (but not consecutive dashes)
            // http://msdn.microsoft.com/en-us/library/windowsazure/dd135715.aspx
            string bcn = blobContainerName;

            return !(bcn.Length < 3
                    || bcn.Length > 63
                    || bcn.Any(c => char.IsUpper(c))
                    || (!char.IsLetter(bcn[0]) && !char.IsNumber(bcn[0]))
                    || bcn.Any(c => !char.IsLetter(c) && !char.IsNumber(c) && c != '-')
                    || bcn.Contains("--"));
        }
    }
}