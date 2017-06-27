namespace Microsoft.Spectrum.Storage.Queue.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public enum MessageType
    {
        FileProcessing = 0,
        RetentionPolicy = 1,
        TablesSharedAccessSignatureUpdate = 2,
        SpectrumObservatoriesHealthMonitoring = 3,
        SpectrumObservatoriesAvailability = 4,
        UserPendingAccessRequestsNotification = 5
    }

    [Serializable]
    public class WorkerQueueMessage
    {
        public WorkerQueueMessage(MessageType type, string measurementStationKey, string blobUri, bool success)
        {
            this.MessageType = (int)type;
            this.BlobUri = blobUri;
            this.MeasurementStationKey = measurementStationKey;
            this.UploadSuccess = success;
        }

        public int MessageType { get; set; }

        public string MeasurementStationKey { get; set; }

        public string BlobUri { get; set; }

        public bool UploadSuccess { get; set; }
    }
}
