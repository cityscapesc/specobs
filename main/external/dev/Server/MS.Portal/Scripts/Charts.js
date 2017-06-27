var ChartsModule = (function () {

    ///Private Properties
    var x = "";
    var y = "";
    var plot = "";
    var frequencyList = [];
    var psdMinData = [];
    var psdAvgData = [];
    var psdMaxData = [];
    var chartType = "";

    //private method
    var getInputData = function () {

        startFrequency = $("#StartFrequency").val();
        endFrequency = $("#EndFrequency").val();
        outlierPercentage = $("#OutlierPercentage").val();

        var slectedDateString = $("#date").val();
        var dateParts = slectedDateString.match(/(\d+)/g);
        var years = parseInt(dateParts[0], 10);
        var months = parseInt(dateParts[1], 10) - 1;
        var days = parseInt(dateParts[2], 10);

        var ChartInput = {
            TimeScale: $('input[name="timeScale"]:checked', '#timeScaleSelector').val(),
            StartDateIso: new Date(Date.UTC(years, months, days, 0, 0, 0, 0)).toISOString(),
            StartDate: $("#date").val(),
            StartTime: $("#slider-range-min").slider("value"),
            StartFrequency: startFrequency,
            StopFrequency: endFrequency,
            OutlierThresholdPercentage: outlierPercentage,
            MeasurementStationId: $("#siStationId").val(),
            StationStorage: $("#siStorageAccountName").val(),
        };

        return ChartInput;
    };

    var validateInputs = function () {
        var isValid = true;

        if ($("#StartFrequency").val() == "") {
            $("#errStartFrequency").removeClass("hidden");
            isValid = false;
        }

        if ($("#EndFrequency").val() == "") {
            $("#errEndFrequency").removeClass("hidden");
            isValid = false;
        }

        if ($("#OutlierPrecentage").val() == "") {
            $("#errOutlierPercentage").removeClass("hidden");
            isValid = false;
        }

        if ($("#date").val() == "") {
            $("#errdate").removeClass("hidden");
            isValid = false;
        }

        return isValid;
    };

    var drawPSDChart = function (avg, min, max) {
        var seriesAverage = {
            data: avg,
            label: "000.00",
            //color: "#eb9a21",
            color: "#de3c07",
            lines: { show: true, lineWidth: 0.8 }
        };

        // [NOTE - Dec-02-2016]: Disabling Min, Max temporarily
        //var seriesMinimum = {
        //    data: min,
        //    label: "000.00",
        //    color: "#de3c07",
        //    lines: { show: true, lineWidth: 0.8 }
        //};
        //var seriesMaximum = {
        //    data: max,
        //    label: "000.00",
        //    color: "#1b95e0",
        //    lines: { show: true, lineWidth: 0.8 }
        //};

        //var data = [seriesMinimum, seriesAverage, seriesMaximum];

        var data = [seriesAverage];

        var xMax, xMin;
        if (max != null && max.length != 0) {
            xMax = max[max.length - 1][0];
            xMin = max[0][0];

        }
        else if (min != null && min.length != 0) {
            xMax = min[min.length - 1][0];
            xMin = min[0][0];
        }
        else if (avg != null && avg.length != 0) {
            xMax = avg[avg.length - 1][0];
            xMin = avg[0][0];
        }
        else {
            xMax = 0;
            xMin = 0;
        }

        var options = {
            grid: { hoverable: true, clickable: true, autoHighlight: false },
            crosshair: { mode: "x" },
            xaxis: { zoomRange: [xMin, xMax], panRange: [xMin, xMax], tickDecimals: 2 },
            yaxis: { zoomRange: [-200, 50], panRange: [-200, -50], labelWidth: 40 }, // Here zoomRange and panRange is set to some approx values.            
            pan: { interactive: true },
            legend: { noColumns: 3, container: $("#psdLegends") },
            hooks: { processOffset: addTopOffset }
        };

        $("#PSDChartdiv").empty();
        $("#chartContainer").removeClass("hidden");
        plot = $.plot($("#PSDChartdiv"), data, options);

        $("#PSDChartdiv").bind('plothover', onPSDPlotHover);

    };

    var insertLabels = function (xAxisLable, yAxisLable, chartType) {
        var html = ['<div class="tickLabels" style="font-size:smaller;">'];
        if (yAxisLable != "") {
            html.push('<div style="position:absolute;left:-20px;padding-left:8px" class="vertical">' + yAxisLable + '</div>');
            html.push('<div class="xAxisLabel"; style="position:absolute;left:44%;text-align:center;font-family:Arial; margin-top:0px; font-weight:bold; font-size:12px; color:#333;bottom:-12px">' + xAxisLable + '</div>');
        }
        else {
            html.push('<div style="position:absolute;left:590px;text-align:center;font-family:Arial; margin-top:0px; font-weight:bold; font-size:12px; color:#333">' + xAxisLable + '</div>');
        }
        html.push('</div>');

        if (chartType == "psd") {
            $("#PSDChartdiv").append(html.join(""));
            $("#PSDChartdiv .vertical").css("top", "67%");
        }
        else if (chartType == "occupancy") {
            $("#OccupancyDiv").append(html.join(""));
            $("#OccupancyDiv .vertical").css("top", "57%");
        }
    }

    var getZoomControls = function () {
        // add zoom out button 
        $('<div class="button" style="right:20px;top:25px">Zoom Out</div>').appendTo($("#PSDChartdiv")).click(function (e) {
            e.preventDefault();
            plot.zoomOut();
        });

        // add zoom in button 
        $('<div class="button" style="right:90px;top:25px">Zoom In</div>').appendTo($("#PSDChartdiv")).click(function (e) {
            e.preventDefault();
            plot.zoom();
        });
    };

    var addArrow = function (file, right, top, offset) {
        $('<img class="button" src="/Content/Images/Arrow-' + file + '.png" style="right:' + right + 'px;top:' + top + 'px">').appendTo($("#PSDChartdiv")).click(function (e) {
            e.preventDefault();
            plot.pan(offset);
        });
    }

    var addTopOffset = function (plot, canvascontext) {
        canvascontext.top = 20;
    };

    var updateCssForPSD = function () {
        legends = $("#psdLegends .legendLabel");

        $('#psdLegends #xlegends').text('MHz: ' + "000.00");

        if (prevHoveredPowerPoints != null && prevHoveredPowerPoints.length > 0) {
            var dataset = plot.getData();
            var count = 0;
            for (i = 0; i < dataset.length; ++i) {
                var series = dataset[i]
                if ("undefined" != typeof series.label) {
                    legends.eq(count).text(series.label.replace(/000.00*/, prevHoveredPowerPoints[count]));
                    count++;
                }
            }
        };

        $("#psdLegends #xlegends").remove();

        if (legends.length > 0) {
            $("#psdLegends").append(xlegends.join(""));
            $('#psdLegends #xlegends').text((prevHoveredFrequency == null) ? 'MHz: 000.00' : 'MHz: ' + prevHoveredFrequency);
        }
    }

    function updateLegend(item) {

        var legends = $("#psdLegends .legendLabel");

        updateLegendTimeout = null;
        prevHoveredPowerPoints = [];
        var y, pos = latestPosition;
        var x;
        var axes = plot.getAxes();
        if (pos.x < axes.xaxis.min || pos.x > axes.xaxis.max ||
            pos.y < axes.yaxis.min || pos.y > axes.yaxis.max)
            return;
        var legandCounter = 0;
        var i, j, dataset = plot.getData();
        for (i = 0; i < dataset.length; ++i) {
            var series = dataset[i];
            //below code is done since the length property of the data points is returned undefined. 
            // if the datapoints is unavailable in data array. hence this workaround done here.
            var dataPointsLength = series.data.length;

            if ("undefined" == jQuery.type(dataPointsLength) || dataPointsLength == 0) {
            }
            else {
                if (item) {
                    if (series.data[item.dataIndex]) {
                        y = series.data[item.dataIndex][1];
                        x = series.data[item.dataIndex][0];
                    }
                }
                else {
                    $("#tooltip").remove();
                    // find the nearest points, x-wise
                    for (j = 0; j < series.data.length; ++j)
                        if (series.data[j][0] > pos.x)
                            break;

                    x = pos.x;
                    // now interpolate

                    var p1 = series.data[j - 1], p2 = series.data[j];
                    if (p1 == null)
                        y = p2[1];
                    else if (p2 == null)
                        y = p1[1];
                    else
                        y = p1[1] + (p2[1] - p1[1]) * (pos.x - p1[0]) / (p2[0] - p1[0]);

                }
                legends.eq(legandCounter).text(series.label.replace(/000.00*/, y.toFixed(2)));
                prevHoveredPowerPoints[legandCounter] = y.toFixed(2);
                prevHoveredFrequency = x.toFixed(2);
                $("#xlegends").text('Frequency MHz: ' + x.toFixed(2));
                //Increment the legend counter.
                legandCounter++;
            }
        }
    }

    function onPSDPlotHover(event, pos, item) {
        plot.setCrosshair({ x: pos.x });
        latestPosition = pos;

        if (chartType == "Power Spectral Density") {
            updateLegend(item);
        }
        else if (chartType == "Occupancy") {
            updateLegendOccupancy(item);
        }
    }

    // Function to update Occupancy chart legends.
    function updateLegendOccupancy(item) {
        updateLegendTimeout = null;
        var y, pos = latestPosition;
        var x;
        var axes = plot.getAxes();
        if (pos.x < axes.xaxis.min || pos.x > axes.xaxis.max ||
            pos.y < axes.yaxis.min || pos.y > axes.yaxis.max)
            return;
        var i, j, dataset = plot.getData();
        for (i = 0; i < dataset.length; ++i) {
            var series = dataset[i];

            if (item) {
                if (series.data.length > 0) {
                    y = series.data[item.dataIndex][1];
                    x = series.data[item.dataIndex][0];
                }
            }
            else {

                // find the nearest points, x-wise
                for (j = 0; j < series.data.length; ++j)
                    if (series.data[j][0] > pos.x)
                        break;

                x = pos.x;
                // now interpolate
                if (series.data.length > 0) {
                    var p1 = series.data[j - 1], p2 = series.data[j];
                    if (p1 == null)
                        y = p2[1];
                    else if (p2 == null)
                        y = p1[1];
                    else
                        y = p1[1] + (p2[1] - p1[1]) * (pos.x - p1[0]) / (p2[0] - p1[0]);
                }
                else {
                    y = 0;
                }
            }

            if ((typeof y !== 'undefined') && (typeof x !== x) && (!isNaN(y)) && (!isNaN(x)) && (typeof series.label !== 'undefined')) {
                $('#psdLegends .legendLabel').eq(i).text(series.label.replace(/0.00%*/, y.toFixed(2) + '%'));
                $("#xlegends").text('Frequency MHz: ' + x.toFixed(2));
            }
        }
    }

    var drawOccupancyChart = function (avg, max) {
        var seriesAverage = {
            data: avg,
            label: "0.00%",
            color: "#eb9a21",
            lines: { show: true, lineWidth: 0.8, fill: 1, fillColor: "#FCD8A1" }
        };
        var seriesMaximum = {
            data: max,
            label: "0.00%",
            color: "#1b95e0",
            lines: { show: true, lineWidth: 0.8, fill: 1, fillColor: "#9A99FF" }
        };

        var data = [seriesAverage, seriesMaximum];

        options = {
            grid: { hoverable: true, clickable: true },
            crosshair: { mode: "x" },
            yaxis: {
                min: 0.0, max: 100.0
            },
            //xaxis: {
            //    min: frequencyList[0], max: frequencyList[frequencyList.length - 1]
            //},
            legend: { noColumns: 2, container: $("#psdLegends") },
            hooks: { processOffset: addTopOffset }
        };

        $("#PSDChartdiv").empty();
        $("#chartContainer").removeClass("hidden");
        plot = $.plot($("#PSDChartdiv"), data, options);

        $("#PSDChartdiv").bind("plothover", onPSDPlotHover);
    }

    //Privilaged methods, they can access both private methods and properties
    return {
        createChart: function () {

            $(".error").addClass("hidden");
            var isFormValid = validateInputs();

            if (!isFormValid) {
                return;
            }

            var data = getInputData();
            chartType = $("#chartType").val();

            $("#PSDChartdiv").empty();
            $('#preloader').removeClass("hidden");
            $("#psdMinContainer").removeClass("hidden");
            $("#chartContainer").addClass("hidden");
            $("#psdErrorBlock").addClass("hidden");
            $("#occupancyYLegend").addClass("hidden");
            $("#psdYLegend").addClass("hidden");

            frequencyList = null;
            psdAvgData = [];
            psdMaxData = [];
            psdMinData = [];

            var url = "";
            if (chartType == "Power Spectral Density") {
                url = "/Home/GetChartData/input";
            }
            else if (chartType == "Occupancy") {
                url = "/Home/GetOccupancyChartData/input";
            }

            var jqxhr = $.getJSON(url, data, function (result) {
                if (result.Result == "Success") {

                    $("#xlegends").text('Frequency MHz: ' + data.StartFrequency);

                    if (chartType == "Power Spectral Density") {

                        averageData = result.AverageData;
                        for (var i = 0; i < averageData.length; i++) {
                            psdAvgData.push([averageData[i].Frequency / 1000000, averageData[i].Value]);
                        }

                        minData = result.MinData;
                        for (var i = 0; i < minData.length; i++) {
                            psdMinData.push([minData[i].Frequency / 1000000, minData[i].Value]);
                        }

                        maxData = result.MaxData;
                        for (var i = 0; i < maxData.length; i++) {
                            psdMaxData.push([maxData[i].Frequency / 1000000, maxData[i].Value]);
                        }

                        drawPSDChart(psdAvgData, psdMinData, psdMaxData);
                        $("#psdYLegend").removeClass("hidden");
                        $("#siTitle").text("Power Spectral Density Chart");

                        $('#fftBinWidth').text(result.FFTBinWidth + " (Hz)");
                    }
                    else if (chartType == "Occupancy") {
                        averageData = result.AverageData;
                        for (var i = 0; i < averageData.length; i++) {
                            psdAvgData.push([averageData[i].Frequency / 1000000, averageData[i].Value]);
                        }

                        maxData = result.MaxData;
                        for (var i = 0; i < maxData.length; i++) {
                            psdMaxData.push([maxData[i].Frequency / 1000000, maxData[i].Value]);
                        }

                        drawOccupancyChart(psdAvgData, psdMaxData);
                        $("#psdMinContainer").addClass("hidden");
                        $("#occupancyYLegend").removeClass("hidden");
                        $("#siTitle").text("Occupancy Chart");
                    }
                }
                else if (result.Result == "Error" && result.Message == "No Data Found") {
                    if (chartType == "Power Spectral Density") {
                        drawPSDChart(null, null, null);
                        $('#fftBinWidth').text(result.FFTBinWidth+ " (Hz)");
                    }
                    else if (chartType == "Occupancy") {
                        drawOccupancyChart(null, null);
                    }
                    $("#psdErrorBlock").removeClass("hidden")
                }

                var timeRange = "";
                var selectedDate = new Date(data.StartDateIso);
                var selectedDateString = data.StartDate;

                switch (data.TimeScale) {
                    case "0":
                        timeRange = data.StartDate + " " + data.StartTime + ":00" + " - " + data.StartDate + " " + (parseInt(data.StartTime) + 1) + ":00";
                        break;
                    case "1":
                        timeRange = data.StartDate + " 00:00" + " - " + data.StartDate + " 23:59";
                        break;
                    case "2":
                        var delta = 0 - selectedDate.getUTCDay();
                        var startDate = new Date(new Date(selectedDate).setUTCDate(selectedDate.getUTCDate() + delta));
                        var stopDate = new Date(new Date(selectedDate).setUTCDate(selectedDate.getUTCDate() + delta + 6));

                        timeRange = startDate.getUTCFullYear() + "-" + (startDate.getUTCMonth() + 1) + "-" + startDate.getUTCDate() + " 00:00" + " - " + stopDate.getUTCFullYear() + "-" + (stopDate.getUTCMonth() + 1) + "-" + stopDate.getUTCDate() + " 23:59";
                        break;
                    case "3":
                        var startDate = new Date(selectedDate.getUTCFullYear(), selectedDate.getUTCMonth(), 1);
                        var stopDate = new Date(selectedDate.getUTCFullYear(), selectedDate.getUTCMonth() + 1, 0);

                        timeRange = startDate.getUTCFullYear() + "-" + (startDate.getUTCMonth() + 1) + "-" + startDate.getUTCDate() + " 00:00" + " - " + stopDate.getUTCFullYear() + "-" + (stopDate.getUTCMonth() + 1) + "-" + stopDate.getUTCDate() + " 23:59";
                        break;
                }

                // set meta data
                $("#mdStationName").text($("#siStationName").text());
                $("#mdGPSDetails").text($("#siLatitude").text() + " , " + $("#siLongitude").text());
                $("#mdFrequency").text(data.StartFrequency + "MHz" + "-" + data.StopFrequency + "MHz");
                $("#mdTimeRange").text(timeRange);

                // Show chart area
                $("#chartSpace").removeClass("hidden");
                $("#mapSpace").addClass("hidden");
                $("#SearchBox").addClass("hidden");
                $("#map-controls").addClass("hidden");

                $('#preloader').addClass("hidden");
            })

            .fail(function (result) {
                alert("error");
                $('.preloader').hide();
            });
        },

        selectReadingKind: function () {
            var minData = null;
            var avgData = null;
            var maxData = null;

            if (chartType == "Power Spectral Density") {
                if ($("#psdMinimumCheckbox").prop("checked") == true) {
                    minData = psdMinData;
                }
                if ($("#psdAverageCheckbox").prop("checked") == true) {
                    avgData = psdAvgData;
                }
                if ($("#psdMaximumCheckbox").prop("checked") == true) {
                    maxData = psdMaxData;
                }

                drawPSDChart(avgData, minData, maxData);
            }
            else if (chartType == "Occupancy") {
                if ($("#psdAverageCheckbox").prop("checked") == true) {
                    avgData = psdAvgData;
                }
                if ($("#psdMaximumCheckbox").prop("checked") == true) {
                    maxData = psdMaxData;
                }

                drawOccupancyChart(avgData, maxData);
            }
        },

        isChartGenerated: function () {
            if (plot != "") {
                return true;
            }
            else {
                return false;
            }
        },
    }
})();


(function () {
    $("#ViewChart").click(ChartsModule.createChart);
    $("#psdMinimumCheckbox").click(ChartsModule.selectReadingKind);
    $("#psdAverageCheckbox").click(ChartsModule.selectReadingKind);
    $("#psdMaximumCheckbox").click(ChartsModule.selectReadingKind);
    $("#PSDChartdiv").height($(window).height() - 190);
})();