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

namespace Microsoft.Spectrum.Analysis.DataProcessor
{
    using System;
    using System.Diagnostics;
    using Microsoft.Spectrum.Common;

    public abstract class DataProcessorBase<TInput, TOutput> : IDataProcessor
        where TInput : RequestPayloadBase
        where TOutput : ResponsePayloadBase
    {
        protected DataProcessorBase(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.Logger = logger;
        }

        public virtual string RequestPayloadType
        {
            get { return typeof(TInput).Name; } // NB: typeof(System.String).Name returns "String"
        }

        protected ILogger Logger { get; private set; }

        public abstract TOutput Execute(TInput request);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "This is a base class doing generic work in the cloud.  Error is returned as a message.")]
        public ResponsePayloadBase Execute(RequestPayloadBase request)
        {
            TInput input = (TInput)request;
            TOutput output = null;

            try
            {
                output = this.Execute(input);
            }
            catch (Exception exception)
            {
                this.Logger.Log(TraceEventType.Error, EventIds.DataProcessorBaseId, exception);
                output = this.CreateExceptionResponse(exception, input);
            }

            return output;
        }

        protected abstract TOutput CreateExceptionResponse(Exception exception, TInput inputRequest);
    }
}
