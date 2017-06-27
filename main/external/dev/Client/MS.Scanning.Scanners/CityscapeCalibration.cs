namespace Microsoft.Spectrum.Scanning.Scanners
{    
    using System;
    using System.Collections.Generic;
    using Common;
    //using System.IO;

    public class CityscapeCalibration
    {
        public DateTime Timestamp
        {
            get;
            private set;
        }

        private List<double> rxFrequencyHzList;
        private List<double> rxGaindBList;

        public CityscapeCalibration(DateTime timestamp)
        {
            this.Timestamp = timestamp;

            this.rxFrequencyHzList = new List<double>();
            this.rxGaindBList = new List<double>();
        }

        public double ComputeAmplitudeAdjustment(double rxFrequencyHz)
        {
            int f1Index;
            int f2Index;

            if (rxFrequencyHz < this.rxFrequencyHzList[0])
            {
                f1Index = 0;
            }
            else if (rxFrequencyHz > this.rxFrequencyHzList[this.rxFrequencyHzList.Count - 1])
            {
                f1Index = this.rxGaindBList.Count - 2;
            }
            else
            {
                f1Index = this.SearchInterval(rxFrequencyHz);
            }

            f2Index = f1Index + 1;

            double gain1 = this.rxGaindBList[f1Index];
            double gain2 = this.rxGaindBList[f2Index];

            double f1 = this.rxFrequencyHzList[f1Index];
            double f2 = this.rxFrequencyHzList[f2Index];

            // interpolated gain in dB.
            double gainIndB = (gain1 * Math.Log(f2 / rxFrequencyHz) + gain2 * Math.Log(rxFrequencyHz / f1)) / Math.Log(f2 / f1);

            // Converting to linear scale.
            double linearAmplitude = MathLibrary.ToRawIQLinearGain(gainIndB);

            //string logFilePath = @"D:\RxCalibrations.csv";

            //using (StreamWriter writer = new StreamWriter(logFilePath, true))
            //{
            //    writer.WriteLine(string.Format("{0},{1},{2}", rxFrequencyHz, gainIndB, linearAmplitude));
            //}

            return linearAmplitude;
        }

        public void Insert(double rxFrequencyInHz, double rxGainIndB)
        {
            this.rxFrequencyHzList.Add(rxFrequencyInHz);
            this.rxGaindBList.Add(rxGainIndB);
        }

        private int SearchInterval(double rxFrequencyHz)
        {
            int startIndex = 0;
            int endIndex = this.rxFrequencyHzList.Count - 1;

            do
            {
                int mid = (endIndex + startIndex) / 2;

                if (this.rxFrequencyHzList[mid] == rxFrequencyHz)
                {
                    return mid;
                }
                else if (rxFrequencyHz < this.rxFrequencyHzList[mid])
                {
                    endIndex = mid - 1;
                }
                else
                {
                    startIndex = mid + 1;
                }

            }
            while (startIndex <= endIndex);

            if (rxFrequencyHz >= this.rxFrequencyHzList[startIndex])
            {
                return startIndex;
            }
            else
            {
                return startIndex - 1;
            }
        }
    }
}
