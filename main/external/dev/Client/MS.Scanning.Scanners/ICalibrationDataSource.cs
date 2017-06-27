namespace Microsoft.Spectrum.Scanning.Scanners
{
    using System;

    public interface ICalibrationDataSource
    {
        DateTime LastModifiedOn
        {
            get;          
        }

        CityscapeCalibration LoadCalibrations();

        bool CalibrationUpdated();      
    }
}
