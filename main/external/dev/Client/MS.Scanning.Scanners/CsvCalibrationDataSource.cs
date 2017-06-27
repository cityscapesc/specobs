namespace Microsoft.Spectrum.Scanning.Scanners
{
    using System;
    using System.IO;
    using Microsoft.Spectrum.Common;

    public class CsvCalibrationDataSource : ICalibrationDataSource
    {
        private string calibrationFilePath;
        private ILogger logger;

        public DateTime LastModifiedOn
        {
            get;
            private set;
        }

        public CsvCalibrationDataSource(string calibrationFilePath, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(calibrationFilePath))
            {
                throw new ArgumentException("Calibration file path can't be null or empty", "calibrationFilePath");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.calibrationFilePath = calibrationFilePath;
            this.LastModifiedOn = DateTime.MinValue;
            this.logger = logger;
        }

      
        public bool CalibrationUpdated()
        {
            if (File.Exists(calibrationFilePath))
            {
                FileInfo fileInfo = new FileInfo(this.calibrationFilePath);

                return fileInfo.LastWriteTimeUtc > this.LastModifiedOn;
            }

            return false;
        }

        public CityscapeCalibration LoadCalibrations()
        {
            if (!File.Exists(this.calibrationFilePath))
            {
                this.logger.Log(System.Diagnostics.TraceEventType.Warning, LoggingMessageId.ScanningConfig, string.Format("USRP calibration file path doesn't exist {0}, so no amplitude adjustment will be made to PSD samples", this.calibrationFilePath));

                return null;
            }

            FileInfo fileInfo = new FileInfo(this.calibrationFilePath);
            
            CityscapeCalibration calibrations = new CityscapeCalibration(fileInfo.LastWriteTimeUtc);

            using (FileStream stream = File.Open(this.calibrationFilePath, FileMode.Open))
            using (StreamReader reader = new StreamReader(stream))
            {
                string[] versionstring = reader.ReadLine().Split(',');
                string[] nameString = reader.ReadLine().Split(',');
                string[] sdrString = reader.ReadLine().Split(',');
                string[] rfFrontEndString = reader.ReadLine().Split(',');
                string[] timeStamp = reader.ReadLine().Split(',');
                string[] sourceString = reader.ReadLine().Split(',');

                reader.ReadLine().Split(',');
                reader.ReadLine().Split(',');

                while (!reader.EndOfStream)
                {
                    string lineOutput = reader.ReadLine();

                    if (!string.IsNullOrWhiteSpace(lineOutput))
                    {
                        string[] commaSepartedValues = lineOutput.Split(',');

                        if (!string.IsNullOrWhiteSpace(commaSepartedValues[0])
                            && !string.IsNullOrWhiteSpace(commaSepartedValues[1]))
                        {
                            calibrations.Insert(double.Parse(commaSepartedValues[0]), double.Parse(commaSepartedValues[1]));
                        }
                    }
                }
            }

            this.LastModifiedOn = fileInfo.LastWriteTimeUtc;

            return calibrations;
        }


    }
}
