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

namespace Microsoft.Spectrum.IO.ScanFile
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Spectrum.Common;

    public static class DataBlockExtensions
    {
        /// <summary>
        /// Groups DataBlocks between TimeStampDataBlocks.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Not a functional issue")]
        public static IEnumerable<IGrouping<DateTime, DataBlock>> GroupByTimestamp(this IEnumerable<DataBlock> dataBlocks, TimeSpan timeGranularity)
        {
            DateTime currentTimestamp = DateTime.MinValue;
            DateTime nextTimestamp = DateTime.MinValue;
            List<DataBlock> scannedBlocks = new List<DataBlock>();

            foreach (DataBlock db in dataBlocks)
            {
                if (db != null)
                {
                    if (currentTimestamp == DateTime.MinValue)
                    {
                        // Round TimeStamp to the nearest natural boundary by removing remainder
                        currentTimestamp = new DateTime((db.Timestamp.Ticks / timeGranularity.Ticks) * timeGranularity.Ticks);
                        nextTimestamp = currentTimestamp + timeGranularity;
                        scannedBlocks = new List<DataBlock>(new[] { db });
                    }
                    else
                    {
                        if (db.Timestamp >= nextTimestamp)
                        {
                            // A grouping divider has been found, so return the group
                            yield return new TimestampGrouping<DataBlock>(currentTimestamp, scannedBlocks);

                            // And reset the accumulators for the next group
                            currentTimestamp = nextTimestamp;
                            nextTimestamp = currentTimestamp + timeGranularity;
                            scannedBlocks = new List<DataBlock>(new[] { db });
                        }
                    }
                }
                else
                {
                    // Not a TimeStampDataBlock, so keep it in the list to be returned in the next grouping
                    scannedBlocks.Add(db);
                }
            }

            yield return new TimestampGrouping<DataBlock>(currentTimestamp, scannedBlocks);
        }

        /// <summary>
        /// Groups DataBlocks between TimeStampDataBlocks.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Not a functional issue")]
        public static IEnumerable<IGrouping<DateTime, DataBlock>> GroupByTimeGranularity(this IEnumerable<DataBlock> dataBlocks, TimeRangeKind timeRangeKind)
        {
            DateTime currentTimestamp = DateTime.MinValue;
            DateTime nextTimestamp = DateTime.MinValue;
            List<DataBlock> scannedBlocks = new List<DataBlock>();

            foreach (DataBlock db in dataBlocks)
            {
                if (db != null)
                {
                    if (currentTimestamp == DateTime.MinValue)
                    {                           
                        TimeSpan timeGranularity = GetTimeGranularity(timeRangeKind, db.Timestamp, out currentTimestamp);
                        nextTimestamp = currentTimestamp + timeGranularity;
                        scannedBlocks = new List<DataBlock>(new[] { db });
                    }
                    else
                    {
                        if (db.Timestamp >= nextTimestamp)
                        {
                            // A grouping divider has been found, so return the group
                            yield return new TimestampGrouping<DataBlock>(currentTimestamp, scannedBlocks);                            

                            // And reset the accumulators for the next group
                            // Following calculation will ensure data grouping is done correctly if in case some values are missing in a time sequence.
                            // For e.g. In case of a scan file having multiple Hours of spectral data, if an hour data is missing in between,an hour data 
                            // following the missing hour will be grouped to the correct timestamp instead of grouping them for the missing hour timestamp.
                            TimeSpan timeGranularity = GetTimeGranularity(timeRangeKind, db.Timestamp, out currentTimestamp);
                            nextTimestamp = currentTimestamp + timeGranularity;
                            scannedBlocks = new List<DataBlock>(new[] { db });
                        }
                        else
                        {
                            // To capture all the data blocks within the timeGranularity.
                            scannedBlocks.Add(db);
                        }
                    }
                }
                else
                {
                    // Not a TimeStampDataBlock, so keep it in the list to be returned in the next grouping
                    scannedBlocks.Add(db);
                }
            }

            yield return new TimestampGrouping<DataBlock>(currentTimestamp, scannedBlocks);
        }

        private static TimeSpan GetTimeGranularity(TimeRangeKind timeRangeKind, DateTime dataBlockTimeStamp, out DateTime currentTimeStamp)
        {
            TimeSpan timeGranularity;

            switch (timeRangeKind)
            {
                case TimeRangeKind.Hourly:
                    timeGranularity = TimeSpan.FromHours(1);
                    currentTimeStamp = new DateTime((dataBlockTimeStamp.Ticks / timeGranularity.Ticks) * timeGranularity.Ticks, DateTimeKind.Utc);
                    break;

                case TimeRangeKind.Daily:
                    timeGranularity = TimeSpan.FromDays(1);
                    currentTimeStamp = new DateTime((dataBlockTimeStamp.Ticks / timeGranularity.Ticks) * timeGranularity.Ticks, DateTimeKind.Utc);
                    break;

                case TimeRangeKind.Weekly:
                    timeGranularity = TimeSpan.FromDays(7);

                    currentTimeStamp = dataBlockTimeStamp.Date;

                    while (currentTimeStamp.DayOfWeek != CultureInfo.InvariantCulture.DateTimeFormat.FirstDayOfWeek)
                    {
                        currentTimeStamp = currentTimeStamp.AddDays(-1);
                    }
                    break;

                case TimeRangeKind.Monthly:

                    // Only value which keeps changes over the time.
                    currentTimeStamp = new DateTime(dataBlockTimeStamp.Year, dataBlockTimeStamp.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    int numberOfDaysInAMonth = DateTime.DaysInMonth(dataBlockTimeStamp.Year, dataBlockTimeStamp.Month);
                    timeGranularity = TimeSpan.FromDays(numberOfDaysInAMonth);
                    break;

                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The TimeRangeKind :{0} is not supported", timeRangeKind.ToString()));
            }

            return timeGranularity;
        }
    }
}
