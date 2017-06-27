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
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    /// <summary>
    /// -----------------------------------------------------------------
    /// Namespace:      Microsoft.Spectrum.Common
    /// Class:          AccessDeniedException
    /// Description:    Custom exception for access denied exception
    /// ----------------------------------------------------------------- 
    [Serializable]
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException()
        {
        }

        public AccessDeniedException(string message, Exception exception)
            : base(message, exception)
        {
        }

        public AccessDeniedException(string message)
            : base(message)
        {
        }

        public AccessDeniedException(ErrorDetails error)
            : base(error.Message)
        {
            this.AccessError = error;
        }

        protected AccessDeniedException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            this.AccessError = (ErrorDetails)info.GetValue("AccessError", typeof(ErrorDetails));
        }

        public ErrorDetails AccessError { get; set; }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentException("info can not be null", "info");
            }

            info.AddValue("AccessError", this.AccessError);
            base.GetObjectData(info, context);
        }
    }
}
