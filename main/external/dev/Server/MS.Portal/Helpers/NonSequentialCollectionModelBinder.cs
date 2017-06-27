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

namespace Microsoft.Spectrum.Portal.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// Courtesy: http://ivanz.com/2011/06/16/editing-variable-length-reorderable-collections-in-asp-net-mvc-part-1/
    /// </summary>
    public static class NonSequentialCollectionModelBinder
    {
        public static IDisposable BeginCollectionItem<TModel>(this HtmlHelper<TModel> html, string collectionName)
        {
            string collectionIndexFieldName = string.Format("{0}.Index", collectionName);

            string itemIndex = GetCollectionItemIndex(collectionIndexFieldName);
            string collectionItemName = string.Format("{0}[{1}]", collectionName, itemIndex);

            TagBuilder indexField = new TagBuilder("input");
            indexField.MergeAttributes(new Dictionary<string, string>() 
                {
                    { "name", collectionIndexFieldName },
                    { "value", itemIndex },
                    { "type", "hidden" },
                    { "autocomplete", "off" }
                });

            html.ViewContext.Writer.WriteLine(indexField.ToString(TagRenderMode.SelfClosing));
            return new CollectionItemNamePrefixScope(html.ViewData.TemplateInfo, collectionItemName);
        }

        private static string GetCollectionItemIndex(string collectionIndexFieldName)
        {
            Queue<string> previousIndices = (Queue<string>)HttpContext.Current.Items[collectionIndexFieldName];
            if (previousIndices == null)
            {
                HttpContext.Current.Items[collectionIndexFieldName] = previousIndices = new Queue<string>();

                string previousIndicesValues = HttpContext.Current.Request[collectionIndexFieldName];
                if (!string.IsNullOrWhiteSpace(previousIndicesValues))
                {
                    foreach (string index in previousIndicesValues.Split(','))
                    {
                        previousIndices.Enqueue(index);
                    }
                }
            }

            return previousIndices.Count > 0 ? previousIndices.Dequeue() : Guid.NewGuid().ToString();
        }

        private class CollectionItemNamePrefixScope : IDisposable
        {
            private readonly TemplateInfo templateInfo;
            private readonly string previousPrefix;

            public CollectionItemNamePrefixScope(TemplateInfo templateInfo, string collectionItemName)
            {
                this.templateInfo = templateInfo;

                this.previousPrefix = templateInfo.HtmlFieldPrefix;
                templateInfo.HtmlFieldPrefix = collectionItemName;
            }

            public void Dispose()
            {
                this.templateInfo.HtmlFieldPrefix = this.previousPrefix;
            }
        }
    }
}