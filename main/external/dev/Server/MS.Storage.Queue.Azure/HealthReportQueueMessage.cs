namespace Microsoft.Spectrum.Storage.Queue.Azure
{
    using System;
    using Microsoft.Spectrum.Common.Enums;

    [Serializable]
    public class HealthReportQueueMessage
    {
        public HealthReportQueueMessage(StationHealthStatus healthStatus, string measurementStationKey, string title, string description, DateTime occurredAt)
        {
            if (string.IsNullOrWhiteSpace(measurementStationKey))
            {
                throw new ArgumentException("MeasurementStaitonKey can't be null or empty", "measurementStationKey");
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Message title can't be null or empty", "title");
            }

            this.HealthStatus = (int)healthStatus;
            this.MeasurementStationKey = measurementStationKey;
            this.Title = title;
            this.Description = description;
            this.OccurredAt = occurredAt;
        }

        public int HealthStatus { get; private set; }

        public string MeasurementStationKey { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime OccurredAt { get; set; }

        public override string ToString()
        {
            return string.Format("Station Key:{0},Title :{1}, Description:{2}, OccurredAt: {3}", this.MeasurementStationKey, this.Title, this.Description, this.OccurredAt);
        }
    }
}
