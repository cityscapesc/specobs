namespace Microsoft.Spectrum.Storage.Models
{
    using System;

    public class RawIQPolicy
    {
        public RawIQPolicy(string category, int bandPriority, double startFrequency, double stopFrequency, int dutycycleOnTimeUpperBoundInMilliSec, int dutycycleTimeLowerBoundInMilliSec, int fileDurationInSec, int retentionTimeInSec, string policyDetails)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Category can't be null or empty", "category");
            }

            if (string.IsNullOrWhiteSpace(policyDetails))
            {
                throw new ArgumentException("Policy details can't be null or empty", "policyDetails");
            }

            this.Category = category;
            this.BandPriority = bandPriority;
            this.StartFrequency = startFrequency;
            this.StopFrequency = stopFrequency;
            this.DutycycleOnTimeUpperBoundInMilliSec = dutycycleOnTimeUpperBoundInMilliSec;
            this.DutycycleTimeLowerBoundInMilliSec = dutycycleTimeLowerBoundInMilliSec;
            this.FileDurationInSec = fileDurationInSec;
            this.RetentionTimeInSec = retentionTimeInSec;
            this.PolicyDetails = policyDetails;
        }

        public string Category { get; private set; }

        public int BandPriority { get; private set; }

        public double StartFrequency { get; private set; }

        public double StopFrequency { get; private set; }

        public int DutycycleOnTimeUpperBoundInMilliSec { get; private set; }

        public int DutycycleTimeLowerBoundInMilliSec { get; private set; }

        public int DutycycleTimeUpperBoundInMilliSec
        {
            get
            {
                return this.FileDurationInSec * 1000;
            }
        }

        public int FileDurationInSec { get; private set; }

        public int RetentionTimeInSec { get; private set; }

        public string PolicyDetails { get; private set; }
    }
}
