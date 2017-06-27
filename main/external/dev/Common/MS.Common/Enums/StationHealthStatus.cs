namespace Microsoft.Spectrum.Common.Enums
{
    public enum StationHealthStatus
    {
        USRPDown = 0,
        ScannerServiceDown = 1,
        ImporterServiceDown = 2,
        NoPower = 3,
        NoInternetConnection = 4,
        ErrorLogs = 5,
        OK = 6,
        UsrpScannerConfigurationChanged = 7
    }
}
