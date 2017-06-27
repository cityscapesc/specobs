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

namespace Microsoft.Spectrum.Portal.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using MathNet.Numerics.Distributions;
    using Microsoft.Spectrum.Common;
    using Microsoft.Spectrum.Portal.Helpers;
    using Microsoft.Spectrum.Portal.Models;
    using Microsoft.Spectrum.Portal.Security;
    using Microsoft.Spectrum.Storage;
    using Microsoft.Spectrum.Storage.DataContracts;
    using Microsoft.Spectrum.Storage.Enums;
    using Microsoft.Spectrum.Storage.Models;
    using Microsoft.Spectrum.Storage.Table.Azure.DataAccessLayer;
    using System.Globalization;

    [AllowAnonymous]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            IEnumerable<MeasurementStationInfo> measurementStations = PortalGlobalCache.Instance.MeasurementStationTableOperation.GetAllMeasurementStationInfoPublic();

            IEnumerable<StationViewModel> viewModels = measurementStations.Select(x => new StationViewModel
            {
                StationId = x.Identifier.Id.ToString(),
                StationName = x.StationDescription.StationName,
                Address = this.GetAddressSting(x.Address),
                Latitude = x.GpsDetails.Latitude.ToString(),
                Longitude = x.GpsDetails.Longitude.ToString(),
                Availability = ((StationAvailability)x.StationAvailability).ToString().ToLower(),
                RadioType = x.DeviceDescription.RadioType,
            });

            return this.View(viewModels);
        }

        public PartialViewResult GetStationInfo(string stationId)
        {
            MeasurementStationInfo measurementStationInfo = PortalGlobalCache.Instance.MeasurementStationTableOperation.GetMeasurementStationInfoPublic(new Guid(stationId));

            if (measurementStationInfo != null)
            {
                return this.PartialView("MeasurementStationInfoPartial", measurementStationInfo);
            }

            return this.PartialView("NoDataFoundPartial");
        }

        // [NOTE: Dec-02-2016] Enable this method have PSD Min, Max and Avg data sample to be rendered on chart.
        //public JsonResult GetChartData(ChartInput input)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        ISpectrumDataProcessorMetadataStorage spectrumDataProcessorMetadataStorage = new SpectrumDataProcessorMetadataStorage(Utility.GetAzureTableContext(input.StationStorage), PortalGlobalCache.Instance.Logger);

        //        SpectrumDataProcessorStorage dataProcessor = new SpectrumDataProcessorStorage(spectrumDataProcessorMetadataStorage, PortalGlobalCache.Instance.Logger);

        //        DateTime requestedDate = this.GetDateByTimeRangeKind(DateTime.Parse(input.StartDateIso), Convert.ToDouble(input.StartTime), (TimeRangeKind)input.TimeScale);

        //        var spectralData = dataProcessor.GetSpectralDataByFrequency(new Guid(input.MeasurementStationId), (Common.TimeRangeKind)input.TimeScale, requestedDate, (long)MathLibrary.MHzToHz(input.StartFrequency), (long)MathLibrary.MHzToHz(input.StopFrequency));

        //        if (spectralData.Count() > 0)
        //        {
        //            if (input.OutlierThresholdPercentage > 0.0001)
        //            {
        //                input.RemoveOutliers = true;
        //            }

        //            List<Tuple<long, double>> avgData = new List<Tuple<long, double>>();
        //            List<Tuple<long, double>> minData = new List<Tuple<long, double>>();
        //            List<Tuple<long, double>> maxData = new List<Tuple<long, double>>();

        //            foreach (SpectrumFrequency spectrumFerquency in spectralData)
        //            {
        //                double standardDeviationOfAverage = double.NaN;
        //                double standardDeviationOfMaximum = double.NaN;
        //                double standardDeviationOfMinimum = double.NaN;
        //                double average = double.NaN;
        //                double maximum = double.NaN;
        //                double minimum = double.NaN;
        //                double averageOfMaximum = double.NaN;
        //                double averageOfMinimum = double.NaN;

        //                foreach (SpectralDensityReading reading in spectrumFerquency.SpectralDensityReadings)
        //                {
        //                    switch (reading.Kind)
        //                    {
        //                        case ReadingKind.Average:
        //                            {
        //                                average = reading.DataPoint;
        //                            }

        //                            break;
        //                        case ReadingKind.Minimum:
        //                            {
        //                                minimum = reading.DataPoint;
        //                            }

        //                            break;
        //                        case ReadingKind.Maximum:
        //                            {
        //                                maximum = reading.DataPoint;
        //                            }

        //                            break;
        //                        case ReadingKind.StandardDeviationOfAverage:
        //                            {
        //                                standardDeviationOfAverage = reading.DataPoint;
        //                            }

        //                            break;
        //                        case ReadingKind.StandardDeviationOfMaximum:
        //                            {
        //                                standardDeviationOfMaximum = reading.DataPoint;
        //                            }

        //                            break;
        //                        case ReadingKind.StandardDeviationOfMinimum:
        //                            {
        //                                standardDeviationOfMinimum = reading.DataPoint;
        //                            }

        //                            break;
        //                        case ReadingKind.AverageOfMaximum:
        //                            {
        //                                averageOfMaximum = reading.DataPoint;
        //                            }

        //                            break;
        //                        case ReadingKind.AverageOfMinimum:
        //                            {
        //                                averageOfMinimum = reading.DataPoint;
        //                            }

        //                            break;
        //                    }

        //                    if (input.RemoveOutliers)
        //                    {
        //                        // The way we are filtering outliers, it doesn't make sense to do it with the average value
        //                        if (!double.IsNaN(average))
        //                        {
        //                            avgData.Add(new Tuple<long, double>(spectrumFerquency.StartHz, average));
        //                        }

        //                        // The CDF tells us the likelyhood that we will have a sample less than the sample that we are checking
        //                        //      If it is within out threshold on either the top or the bottom, then we won't plot it.
        //                        if (!double.IsNaN(minimum) && !double.IsNaN(averageOfMinimum) && !double.IsNaN(standardDeviationOfMinimum))
        //                        {
        //                            Normal normalMinimumDistribution = new Normal(averageOfMinimum, standardDeviationOfMinimum);
        //                            double cdfMin = normalMinimumDistribution.CumulativeDistribution(minimum);

        //                            if (cdfMin < (1 - (input.OutlierThresholdPercentage / 100 / 2)) &&
        //                                cdfMin > (input.OutlierThresholdPercentage / 100 / 2))
        //                            {
        //                                minData.Add(new Tuple<long, double>(spectrumFerquency.StartHz, minimum));
        //                            }
        //                            else
        //                            {
        //                                minData.Add(new Tuple<long, double>(spectrumFerquency.StartHz, normalMinimumDistribution.InverseCumulativeDistribution(input.OutlierThresholdPercentage / 100 / 2)));
        //                            }
        //                        }

        //                        if (!double.IsNaN(maximum) && !double.IsNaN(averageOfMaximum) && !double.IsNaN(standardDeviationOfMaximum))
        //                        {
        //                            Normal normalMaximumDistribution = new Normal(averageOfMaximum, standardDeviationOfMaximum);
        //                            double cdfMax = normalMaximumDistribution.CumulativeDistribution(maximum);

        //                            if (cdfMax < (1 - (input.OutlierThresholdPercentage / 100 / 2)) &&
        //                                cdfMax > (input.OutlierThresholdPercentage / 100 / 2))
        //                            {
        //                                maxData.Add(new Tuple<long, double>(spectrumFerquency.StartHz, maximum));
        //                            }
        //                            else
        //                            {
        //                                maxData.Add(new Tuple<long, double>(spectrumFerquency.StartHz, normalMaximumDistribution.InverseCumulativeDistribution(1 - (input.OutlierThresholdPercentage / 100 / 2))));
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (!double.IsNaN(average))
        //                        {
        //                            avgData.Add(new Tuple<long, double>(spectrumFerquency.StartHz, average));
        //                        }

        //                        if (!double.IsNaN(minimum))
        //                        {
        //                            minData.Add(new Tuple<long, double>(spectrumFerquency.StartHz, minimum));
        //                        }

        //                        if (!double.IsNaN(maximum))
        //                        {
        //                            maxData.Add(new Tuple<long, double>(spectrumFerquency.StartHz, maximum));
        //                        }
        //                    }
        //                }
        //            }

        //            Tuple<long, double>[] avgSampledData = null;
        //            Tuple<long, double>[] minSampledData = null;
        //            Tuple<long, double>[] maxSampledData = null;

        //            Task getAvgSampledData = new Task(() =>
        //            {
        //                avgSampledData = this.DownSampleData(avgData, PortalConstants.DownsampleThreshold);
        //            });

        //            getAvgSampledData.Start();

        //            Task getMinSampledData = new Task(() =>
        //            {
        //                minSampledData = this.DownSampleData(minData, PortalConstants.DownsampleThreshold);
        //            });

        //            getMinSampledData.Start();

        //            Task getMaxSampledData = new Task(() =>
        //            {
        //                maxSampledData = this.DownSampleData(maxData, PortalConstants.DownsampleThreshold);
        //            });

        //            getMaxSampledData.Start();

        //            Task.WaitAll(getAvgSampledData, getMinSampledData, getMaxSampledData);

        //            ChartData[] maxChartData = new ChartData[maxSampledData.Length];

        //            for (int i = 0; i < maxSampledData.Length; i++)
        //            {
        //                maxChartData[i] = new ChartData();
        //                maxChartData[i].Frequency = maxSampledData[i].Item1;
        //                maxChartData[i].Value = maxSampledData[i].Item2;
        //            }

        //            ChartData[] averageChartData = new ChartData[avgSampledData.Length];

        //            for (int i = 0; i < avgSampledData.Length; i++)
        //            {
        //                averageChartData[i] = new ChartData();
        //                averageChartData[i].Frequency = avgSampledData[i].Item1;
        //                averageChartData[i].Value = avgSampledData[i].Item2;
        //            }

        //            ChartData[] minChartData = new ChartData[minSampledData.Length];

        //            for (int i = 0; i < minSampledData.Length; i++)
        //            {
        //                minChartData[i] = new ChartData();

        //                minChartData[i].Frequency = minSampledData[i].Item1;
        //                minChartData[i].Value = minSampledData[i].Item2;
        //            }

        //            return this.Json(new { Result = "Success", AverageData = averageChartData, MaxData = maxChartData, MinData = minChartData }, JsonRequestBehavior.AllowGet);
        //        }

        //        return this.Json(new { Result = "Error", Message = "No Data Found" }, JsonRequestBehavior.AllowGet);
        //    }

        //    var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);

        //    return this.Json(new { Result = "Error", Message = "Model Error", Data = modelStateErrors }, JsonRequestBehavior.AllowGet);
        //}


        // [NOTE: Dec-02-2016] PSD avg data only method.        
        public JsonResult GetChartData(ChartInput input)
        {
            if (ModelState.IsValid)
            {
                ISpectrumDataProcessorMetadataStorage spectrumDataProcessorMetadataStorage = new SpectrumDataProcessorMetadataStorage(Utility.GetAzureTableContext(input.StationStorage), PortalGlobalCache.Instance.Logger);

                SpectrumDataProcessorStorage dataProcessor = new SpectrumDataProcessorStorage(spectrumDataProcessorMetadataStorage, PortalGlobalCache.Instance.Logger);

                DateTime requestedDate = this.GetDateByTimeRangeKind(DateTime.ParseExact(input.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture), Convert.ToDouble(input.StartTime), (TimeRangeKind)input.TimeScale);

                double fftBinWidth = PortalGlobalCache.Instance.MeasurementStationTableOperation.GetFFTBinWidth(Guid.Parse(input.MeasurementStationId), requestedDate, GetEndTimeRangeKind(requestedDate, (TimeRangeKind)input.TimeScale));

                var spectralData = dataProcessor.GetSpectralDataByFrequency(new Guid(input.MeasurementStationId), (Common.TimeRangeKind)input.TimeScale, requestedDate, (long)MathLibrary.MHzToHz(input.StartFrequency), (long)MathLibrary.MHzToHz(input.StopFrequency));

                if (spectralData.Count() > 0)
                {
                    if (input.OutlierThresholdPercentage > 0.0001)
                    {
                        input.RemoveOutliers = true;
                    }

                    List<Tuple<long, double>> avgData = new List<Tuple<long, double>>();

                    foreach (SpectrumFrequency spectrumFerquency in spectralData)
                    {
                        double average = double.NaN;

                        foreach (SpectralDensityReading reading in spectrumFerquency.SpectralDensityReadings)
                        {
                            switch (reading.Kind)
                            {
                                case ReadingKind.Average:
                                    {
                                        average = reading.DataPoint;
                                    }
                                    break;
                            }

                            if (!double.IsNaN(average))
                            {
                                avgData.Add(new Tuple<long, double>(spectrumFerquency.StartHz, average));
                            }
                        }
                    }

                    Tuple<long, double>[] avgSampledData = null;

                    Task getAvgSampledData = new Task(() =>
                    {
                        avgSampledData = this.DownSampleData(avgData, PortalConstants.DownsampleThreshold);
                    });

                    getAvgSampledData.Start();

                    Task.WaitAll(getAvgSampledData);

                    ChartData[] averageChartData = new ChartData[avgSampledData.Length];
                    ChartData[] maxChartData = new ChartData[0];
                    ChartData[] minChartData = new ChartData[0];

                    for (int i = 0; i < avgSampledData.Length; i++)
                    {
                        averageChartData[i] = new ChartData();
                        averageChartData[i].Frequency = avgSampledData[i].Item1;
                        averageChartData[i].Value = avgSampledData[i].Item2;
                    }

                    return this.Json(new { Result = "Success", AverageData = averageChartData, MaxData = maxChartData, MinData = minChartData, FFTBinWidth = fftBinWidth }, JsonRequestBehavior.AllowGet);
                }

                return this.Json(new { Result = "Error", Message = "No Data Found", FFTBinWidth = fftBinWidth }, JsonRequestBehavior.AllowGet);
            }

            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);

            return this.Json(new { Result = "Error", Message = "Model Error", Data = modelStateErrors }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOccupancyChartData(ChartInput input)
        {
            const int CalculatedNoiseFloorRangeMHz = 50;

            if (ModelState.IsValid)
            {
                ISpectrumDataProcessorMetadataStorage spectrumDataProcessorMetadataStorage = new SpectrumDataProcessorMetadataStorage(Utility.GetAzureTableContext(input.StationStorage), PortalGlobalCache.Instance.Logger);

                SpectrumDataProcessorStorage dataProcessor = new SpectrumDataProcessorStorage(spectrumDataProcessorMetadataStorage, PortalGlobalCache.Instance.Logger);

                DateTime requestedDate = this.GetDateByTimeRangeKind(DateTime.Parse(input.StartDateIso), Convert.ToDouble(input.StartTime), (TimeRangeKind)input.TimeScale);

                double occupancyStartFrequency = input.StartFrequency - CalculatedNoiseFloorRangeMHz;
                double occupancyEndFrequency = input.StopFrequency + CalculatedNoiseFloorRangeMHz;

                if (occupancyStartFrequency < 0)
                {
                    occupancyStartFrequency = 0;
                }

                if (occupancyEndFrequency < 0)
                {
                    occupancyEndFrequency = 0;
                }

                var spectralData = dataProcessor.GetSpectralDataByFrequency(new Guid(input.MeasurementStationId), (Common.TimeRangeKind)input.TimeScale, requestedDate, (long)MathLibrary.MHzToHz(occupancyStartFrequency), (long)MathLibrary.MHzToHz(occupancyEndFrequency));
                var orderedSpectralData = spectralData.OrderBy(frequency => frequency.StartHz).ToList();

                double[] averageNoiseFloor = new double[orderedSpectralData.LongCount()];
                double[] maxNoiseFloor = new double[orderedSpectralData.LongCount()];
                double currentTotalAverage = 0;
                double currentTotalMax = 0;
                int outOfCalculatedNoiseFloorRangeIndex = 0;
                int outOfCalculatedMaxNoiseFloorRangeSamples = 0;
                int outOfCalculatedAverageNoiseFloorRangeSamples = 0;

                // Here we are calculating the noise floor by taking the average of the average power at a frequency +/- 50 MHz
                for (int i = 0; i < orderedSpectralData.Count; i++)
                {
                    SpectrumFrequency frequency = orderedSpectralData[i];

                    while (frequency.StartHz - orderedSpectralData[outOfCalculatedNoiseFloorRangeIndex].StartHz > (long)MathLibrary.MHzToHz(CalculatedNoiseFloorRangeMHz) * 2)
                    {
                        foreach (SpectralDensityReading backReading in orderedSpectralData[outOfCalculatedNoiseFloorRangeIndex].SpectralDensityReadings)
                        {
                            if (backReading.Kind == ReadingKind.Average)
                            {
                                double averageReading = backReading.DataPoint;
                                if (!double.IsNaN(averageReading))
                                {
                                    currentTotalAverage -= averageReading;
                                    outOfCalculatedAverageNoiseFloorRangeSamples--;
                                }
                            }

                            if (backReading.Kind == ReadingKind.AverageOfMaximum)
                            {
                                double maxReading = backReading.DataPoint;
                                if (!double.IsNaN(maxReading))
                                {
                                    currentTotalMax -= maxReading;
                                    outOfCalculatedMaxNoiseFloorRangeSamples--;
                                }
                            }
                        }

                        outOfCalculatedNoiseFloorRangeIndex++;
                    }

                    foreach (SpectralDensityReading reading in frequency.SpectralDensityReadings)
                    {
                        if (reading.Kind == ReadingKind.Average)
                        {
                            double averageReading = reading.DataPoint;
                            if (!double.IsNaN(averageReading))
                            {
                                outOfCalculatedAverageNoiseFloorRangeSamples++;
                                currentTotalAverage += averageReading;
                                averageNoiseFloor[i] = currentTotalAverage / outOfCalculatedAverageNoiseFloorRangeSamples;
                            }
                        }

                        if (reading.Kind == ReadingKind.AverageOfMaximum)
                        {
                            double maxReading = reading.DataPoint;
                            if (!double.IsNaN(maxReading))
                            {
                                outOfCalculatedMaxNoiseFloorRangeSamples++;
                                currentTotalMax += maxReading;
                                maxNoiseFloor[i] = currentTotalMax / outOfCalculatedMaxNoiseFloorRangeSamples;
                            }
                        }
                    }
                }

                // Now that we have our noise floor, we can calculate the difference between the noise floor and every point 
                //      We can use that along with the standard deviation at that frequency to figure out the percentage occupancy
                //      by computing the z score
                List<Tuple<long, double>> maxData = new List<Tuple<long, double>>();
                List<Tuple<long, double>> avgData = new List<Tuple<long, double>>();
                for (int i = 0; i < orderedSpectralData.Count; i++)
                {
                    if ((orderedSpectralData[i].StartHz < (long)MathLibrary.MHzToHz(input.StartFrequency)) ||
                        (orderedSpectralData[i].StartHz > (long)MathLibrary.MHzToHz(input.StopFrequency)))
                    {
                        continue;
                    }

                    double standardDeviationOfAverage = double.NaN;
                    double standardDeviationOfMaximum = double.NaN;
                    double average = double.NaN;
                    double maximumAverage = double.NaN;

                    foreach (var reading in orderedSpectralData[i].SpectralDensityReadings)
                    {
                        if (reading.Kind == ReadingKind.Average)
                        {
                            average = reading.DataPoint;
                        }
                        else if (reading.Kind == ReadingKind.AverageOfMaximum)
                        {
                            maximumAverage = reading.DataPoint;
                        }
                        else if (reading.Kind == ReadingKind.StandardDeviationOfAverage)
                        {
                            standardDeviationOfAverage = reading.DataPoint;
                        }
                        else if (reading.Kind == ReadingKind.StandardDeviationOfMaximum)
                        {
                            standardDeviationOfMaximum = reading.DataPoint;
                        }
                    }

                    // The CDF tells us the likelyhood that we will have a sample less than the noise floor, so the occupancy 1 - that value
                    // then we need to multiply by 100 to have the value in terms of percentages
                    if (!double.IsNaN(average) && !double.IsNaN(standardDeviationOfAverage))
                    {
                        Normal normalAverageDistribution = new Normal(average, standardDeviationOfAverage);
                        avgData.Add(new Tuple<long, double>(orderedSpectralData[i].StartHz, 100 * (1 - normalAverageDistribution.CumulativeDistribution(averageNoiseFloor[i]))));
                    }

                    if (!double.IsNaN(maximumAverage) && !double.IsNaN(standardDeviationOfMaximum))
                    {
                        Normal normalMaximumDistribution = new Normal(maximumAverage, standardDeviationOfMaximum);
                        maxData.Add(new Tuple<long, double>(orderedSpectralData[i].StartHz, 100 * (1 - normalMaximumDistribution.CumulativeDistribution(maxNoiseFloor[i]))));
                    }
                }

                Tuple<long, double>[] avgSampledData = null;
                Tuple<long, double>[] maxSampledData = null;

                Task getAvgSampledData = new Task(() =>
                {
                    avgSampledData = this.DownSampleData(avgData, PortalConstants.DownsampleThreshold);
                });

                getAvgSampledData.Start();

                Task getMaxSampledData = new Task(() =>
                {
                    maxSampledData = this.DownSampleData(maxData, PortalConstants.DownsampleThreshold);
                });

                getMaxSampledData.Start();

                Task.WaitAll(getAvgSampledData, getMaxSampledData);

                ChartData[] averageChartData = new ChartData[avgSampledData.Length];
                ChartData[] maxChartData = new ChartData[maxSampledData.Length];

                for (int i = 0; i < avgSampledData.Length; i++)
                {
                    averageChartData[i] = new ChartData();

                    if (avgSampledData[i] != null)
                    {
                        averageChartData[i].Frequency = avgSampledData[i].Item1;
                        averageChartData[i].Value = avgSampledData[i].Item2;
                    }
                }

                for (int i = 0; i < maxSampledData.Length; i++)
                {
                    maxChartData[i] = new ChartData();

                    if (maxSampledData[i] != null)
                    {
                        maxChartData[i].Frequency = maxSampledData[i].Item1;
                        maxChartData[i].Value = maxSampledData[i].Item2;
                    }
                }

                if (maxChartData.Length > 0 || averageChartData.Length > 0)
                {
                    return this.Json(new { Result = "Success", AverageData = averageChartData, MaxData = maxChartData }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return this.Json(new { Result = "Error", Message = "No Data Found" }, JsonRequestBehavior.AllowGet);
                }
            }

            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);

            return this.Json(new { Result = "Error", Message = "Model Error", Data = modelStateErrors }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUserInfo()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                UserPrincipal userPrincipal = (UserPrincipal)this.User;
                UserInfo userInfo = PortalGlobalCache.Instance.UserManager.GetUserInfoByAccessToken(userPrincipal.AccessToken);

                if (userInfo != null)
                {
                    return this.Json(
                    new
                    {
                        Result = "Success",
                        Data = new
                        {
                            FirstName = userInfo.User.FirstName,
                            LastName = userInfo.User.LastName,
                            Email = userInfo.User.AccountEmail,
                            Phone = userInfo.User.Phone
                        }
                    },
                    JsonRequestBehavior.AllowGet);
                }
            }

            return this.Json(new { Result = "Error", Message = "No Data Found" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ReportProblem(IssueReport issueReport)
        {
            if (ModelState.IsValid)
            {
                PortalGlobalCache.Instance.PortalTableOperations.SaveIssueReport(issueReport);

                return this.Json(new { Result = "Success", });
            }

            var modelStateErrors = this.ModelState.Keys.SelectMany(key => this.ModelState[key].Errors);

            return this.Json(new { Result = "Error", Message = "Model Error", Data = modelStateErrors });
        }

        public PartialViewResult ViewStationDetails(Guid measurementStationId)
        {
            IMeasurementStationTableOperations measurementStationOperations = PortalGlobalCache.Instance.MeasurementStationTableOperation;
            MeasurementStationInfo stationInfo = measurementStationOperations.GetMeasurementStationInfoPrivate(measurementStationId);

            this.ViewData["measurementStationId"] = measurementStationId;
            StationRegistrationInputs registrationDetails = Utility.ConvertToStationRegistrationInputs(stationInfo, stationInfo.PrivateData.PrimaryContactEmail);

            return this.PartialView("ViewSpectrumObservatoryDetailsPartial", registrationDetails);
        }

        public PartialViewResult First(RawDataInput input)
        {
            return this.PartialView("RawDataPartial", this.GetRawDataPageWise(input, 0));
        }

        public PartialViewResult Next(int timeScale, string startDate, string startDateIso, string startTime, string measurementStationId, int typeId, int pageIndex)
        {
            RawDataInput input = new RawDataInput()
            {
                TimeScale = timeScale,
                StartDate = startDate,
                StartDateIso = startDateIso,
                StartTime = startTime,
                MeasurementStationId = measurementStationId,
                TypeId = typeId
            };

            return this.Next(input, pageIndex);
        }

        public PartialViewResult Previous(int timeScale, string startDate, string startDateIso, string startTime, string measurementStationId, int typeId, int pageIndex)
        {
            RawDataInput input = new RawDataInput()
            {
                TimeScale = timeScale,
                StartDate = startDate,
                StartDateIso = startDateIso,
                StartTime = startTime,
                MeasurementStationId = measurementStationId,
                TypeId = typeId
            };

            return this.Previous(input, pageIndex);
        }

        public PartialViewResult All(RawDataInput input)
        {
            return this.PartialView("RawDataPartial", this.GetRawData(input));
        }

        private PartialViewResult Next(RawDataInput input, int pageIndex)
        {
            return this.PartialView("RawDataPartial", this.GetRawDataPageWise(input, pageIndex + 1));
        }

        private PartialViewResult Previous(RawDataInput input, int pageIndex)
        {
            return this.PartialView("RawDataPartial", this.GetRawDataPageWise(input, pageIndex - 1));
        }

        private IEnumerable<ScanFileInformation> GetRawData(RawDataInput input)
        {
            MeasurementStationInfo measurementStationInfo = PortalGlobalCache.Instance.MeasurementStationTableOperation.GetMeasurementStationInfoPublic(new Guid(input.MeasurementStationId));
            ISpectrumDataProcessorMetadataStorage spectrumMetadataOperations = new SpectrumDataProcessorMetadataStorage(Utility.GetAzureTableContext(measurementStationInfo.Identifier.StorageAccountName), PortalGlobalCache.Instance.Logger);
            DateTime requestedStartDate = this.GetDateByTimeRangeKind(DateTime.Parse(input.StartDateIso), Convert.ToDouble(input.StartTime), (TimeRangeKind)input.TimeScale);
            DateTime requestedEndDate;

            if ((TimeRangeKind)input.TimeScale == TimeRangeKind.Hourly)
            {
                requestedEndDate = requestedStartDate.AddHours(1);
            }
            else
            {
                requestedEndDate = requestedStartDate.AddDays(1);
            }

            this.ViewData["measurementStationId"] = input.MeasurementStationId;

            return spectrumMetadataOperations.GetSpectralMetadataInfoByTimeRange(Guid.Parse(input.MeasurementStationId), requestedStartDate, requestedEndDate, input.TypeId);
        }

        private IEnumerable<ScanFileInformation> GetRawDataPageWise(RawDataInput input, int pageIndex)
        {
            this.ViewBag.TimeScale = input.TimeScale;
            this.ViewBag.StartDate = input.StartDate;
            this.ViewBag.StartDateIso = input.StartDateIso;
            this.ViewBag.StartTime = input.StartTime;
            this.ViewBag.MeasurementStationId = input.MeasurementStationId;
            this.ViewBag.TypeId = input.TypeId;

            IEnumerable<ScanFileInformation> rawDataFiles = this.GetRawData(input);

            this.ViewBag.TotalCount = rawDataFiles.Count();
            this.ViewBag.PageIndex = pageIndex;

            return rawDataFiles.Skip(pageIndex * Constants.PageSize).Take(Constants.PageSize);
        }

        private string GetAddressSting(Storage.Models.Address address)
        {
            if (address != null)
            {
                StringBuilder addressBuilder = new StringBuilder();

                if (!string.IsNullOrEmpty(address.AddressLine1))
                {
                    addressBuilder.Append(address.AddressLine1);
                    addressBuilder.Append(", ");
                }

                if (!string.IsNullOrEmpty(address.AddressLine2))
                {
                    addressBuilder.Append(address.AddressLine2);
                    addressBuilder.Append(", ");
                }

                if (!string.IsNullOrEmpty(address.Location))
                {
                    addressBuilder.Append(address.Location);
                    addressBuilder.Append(", ");
                }

                if (!string.IsNullOrEmpty(address.Country))
                {
                    addressBuilder.Append(address.Country);
                    addressBuilder.Append(", ");
                }

                return addressBuilder.ToString();
            }

            return string.Empty;
        }

        private Tuple<long, double>[] DownSampleData(List<Tuple<long, double>> data, int threshold)
        {
            int dataLength = data.Count;
            if (threshold >= dataLength || threshold == 0)
            {
                return data.ToArray(); // Nothing to do
            }

            Tuple<long, double>[] downSampled = new Tuple<long, double>[threshold];

            // Bucket size. Leave room for start and end data points
            double bucketSize = (double)(dataLength - 2) / (threshold - 2);

            int selectedPoint = 0;
            Tuple<long, double> maxAreaPoint = new Tuple<long, double>(0, 0);
            int nextA = 0;

            downSampled[0] = data[selectedPoint]; // Always add the first point

            for (int i = 1; i < threshold - 2; i++)
            {
                // Calculate point average for next bucket (containing c)
                double avgX = 0;
                double avgY = 0;
                int avgRangeStart = (int)(Math.Floor((i + 1) * bucketSize) + 1);
                int avgRangeEnd = (int)(Math.Floor((i + 2) * bucketSize) + 1);
                avgRangeEnd = avgRangeEnd < dataLength ? avgRangeEnd : dataLength;

                int avgRangeLength = avgRangeEnd - avgRangeStart;

                for (; avgRangeStart < avgRangeEnd; avgRangeStart++)
                {
                    avgX += data[avgRangeStart].Item1;
                    avgY += data[avgRangeStart].Item2;
                }

                avgX /= avgRangeLength;

                avgY /= avgRangeLength;

                // Get the range for this bucket
                int rangeOffs = (int)(Math.Floor(i * bucketSize) + 1);
                int rangeTo = (int)(Math.Floor((i + 1) * bucketSize) + 1);

                // Point a
                double pointAx = data[selectedPoint].Item1;
                double pointAy = data[selectedPoint].Item2;

                double maxArea = -1;

                for (; rangeOffs < rangeTo; rangeOffs++)
                {
                    // Calculate triangle area over three buckets
                    double area = Math.Abs(((pointAx - avgX) * (data[rangeOffs].Item2 - pointAy)) -
                                           ((pointAx - data[rangeOffs].Item1) * (avgY - pointAy))) * 0.5;

                    if (area > maxArea)
                    {
                        maxArea = area;
                        maxAreaPoint = data[rangeOffs];
                        nextA = rangeOffs; // Next a is this b
                    }
                }

                // Pick this point from the bucket
                downSampled[i] = maxAreaPoint;

                // This a is the next a (chosen b)
                selectedPoint = nextA;
            }

            // Always add last two points
            downSampled[threshold - 2] = data[dataLength - 2];
            downSampled[threshold - 1] = data[dataLength - 1];

            return downSampled;
        }

        public JsonResult AllowRawDataDownload(string stationId)
        {
            UserPrincipal user = this.User as UserPrincipal;

            if (user == null || string.IsNullOrWhiteSpace(stationId))
            {
                return Json(new { allowDownload = false }, JsonRequestBehavior.AllowGet);
            }
            else if (user.Role == UserRoles.SiteAdmin)
            {
                return Json(new { allowDownload = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                bool isSiteAdmin = user.IsInRole(UserRoles.StationAdmin.ToString(), stationId);

                return Json(new { allowDownload = isSiteAdmin }, JsonRequestBehavior.AllowGet);
            }
        }

        private DateTime GetDateByTimeRangeKind(DateTime inputDate, double startTime, TimeRangeKind timeRangeKind)
        {
            // No action required for daily
            switch (timeRangeKind)
            {
                case TimeRangeKind.Hourly:
                    {
                        return inputDate.AddHours(startTime);
                    }

                case TimeRangeKind.Weekly:
                    {
                        // find out start date of week
                        int delta = DayOfWeek.Sunday - inputDate.DayOfWeek;
                        return inputDate.AddDays(delta);
                    }

                case TimeRangeKind.Monthly:
                    {
                        int delta = 1 - inputDate.Day;
                        return inputDate.AddDays(delta);
                    }

                default: return inputDate;
            }
        }

        private static DateTime GetEndTimeRangeKind(DateTime startTime, TimeRangeKind timeRangeKind)
        {
            // No action required for daily
            switch (timeRangeKind)
            {
                case TimeRangeKind.Hourly:
                    {
                        return startTime.AddHours(1);
                    }

                case TimeRangeKind.Weekly:
                    {
                        // find out start date of week
                        int delta = DayOfWeek.Saturday - startTime.DayOfWeek;
                        return startTime.AddDays(delta + (int)DayOfWeek.Saturday);
                    }

                case TimeRangeKind.Monthly:
                    {
                        int delta = 1 - startTime.Day;
                        return startTime.AddDays(delta + DateTime.DaysInMonth(startTime.Year, startTime.Month));
                    }

                default: return startTime;
            }
        }

    }
}