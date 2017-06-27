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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Client
{
    public class BlockingPool<T>
    {
        AutoResetEvent areItemAvailable = new AutoResetEvent(false);
        Queue<T> items;
        int currentItemCount;
        int maxItemCount;
        Func<T> itemCreationFunc;

        public BlockingPool(int maxItemCount, Func<T> itemCreationFunc)
        {
            this.maxItemCount = maxItemCount;
            this.itemCreationFunc = itemCreationFunc;
        }

        public T Get()
        {
            while (true)
            {
                lock (items)
                {
                    if (items.Count > 0)
                    {
                        return items.Dequeue();
                    }

                    if (currentItemCount < maxItemCount)
                    {
                        currentItemCount++;
                        Console.WriteLine("BlockingPool<{0}>, item count: {1}", typeof(T), currentItemCount);

                        return itemCreationFunc();
                    }
                }

                areItemAvailable.WaitOne();
            }
        }

        public void Return(T item)
        {
            lock (items)
            {
                items.Enqueue(item);
                areItemAvailable.Set();
            }
        }
    }
}