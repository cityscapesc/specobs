namespace Microsoft.Spectrum.Storage.Table.Azure
{
    using System;
    using Microsoft.WindowsAzure.Storage.Table;
    using Common;
    using System.Globalization;

    public class RawIQScanPolicy : TableEntity
    {
        public RawIQScanPolicy()
        {
        }

        public RawIQScanPolicy(long startFrequency, long stopFrequency, string category)
        {
            if (String.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Category can't be null or empty", "category");
            }

            this.PartitionKey = startFrequency.ToString(Constants.ElevenDigit, CultureInfo.InvariantCulture).PadLeft(11, '0');
            string stopFrequencyText = stopFrequency.ToString(Constants.ElevenDigit, CultureInfo.InvariantCulture).PadLeft(11, '0');
            this.RowKey = string.Format("{0}_{1}", stopFrequencyText, category);

            this.StartFrequency = startFrequency;
            this.StopFrequency = stopFrequency;
            this.Category = category;
        }

        public string Category { get; set; }

        public int BandPriority { get; set; }

        public long StartFrequency { get; set; }

        public long StopFrequency { get; set; }

        public int DutycycleOnTimeInMilliSec { get; set; }

        public int DutycycleTimeInMilliSec { get; set; }

        public int FileDurationInSec { get; set; }

        public int RetentionTimeInSec { get; set; }

        public string PolicyDetails { get; set; }
    }
}
