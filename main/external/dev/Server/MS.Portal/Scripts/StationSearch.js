/// <reference path="StationSearch.js" />
$(function () {
    //instance of bingMap is created in MapPartial
    var mapDiv = $("#mapDiv");
    var generatedMapDiv = mapDiv.first();
    $(generatedMapDiv).height($(window).height());
    $(generatedMapDiv).width(mapDiv.width());

    bingMap.viewchangeend(onViewChangeEnd);

    setTimeout(
  function () {
      InsertPushpins(bingMap);
  }, 1000);

    InitializeSlider();

    $("#date").datepicker({
        dateFormat: "yy-mm-dd",
        defaultDate: new Date(),
        setDate: new Date()
    });

    $("#dateRawData").datepicker({
        dateFormat: "yy-mm-dd",
        defaultDate: new Date(),
        setDate: new Date()
    });

    $("#tsHourly").click(onTimerangeSelection);
    $("#tsDaily").click(onTimerangeSelection);
    $("#tsWeekly").click(onTimerangeSelection);
    $("#tsMonthly").click(onTimerangeSelection);
    $("#tsHourlyRawData").click(onRawDataTimerangeSelection);
    $("#tsDailyRawData").click(onRawDataTimerangeSelection);

    $("#ViewRawData").click(onViewRawData);
});

var allStationsView = function () {
    $("#StationList").removeClass("hidden");
    $("#homeLPane").addClass("hidden");
    $("#stationInfo").addClass("hidden");
    $("#Radio20").attr('checked', 'checked');
};

window.onload = function () {
    var query = window.location.search.substring(1);
    if (query == "stationSearch") {
        allStationsView();
    }
};

var availablePushpins = [];
var pushpinOptions = {
    zIndex: 100,
    height: null,
    width: null
};

function InsertPushpins(bingMap) {
    //Iterate through stations
    bingMap.deferred.then(function () {
        bingMap.ClearMapEntities();
        var locationArray = [];
        $(".station").each(function () {

            var latitude = $(this).find(".lat")[0].textContent;
            var longitude = $(this).find(".lon")[0].textContent;
            var name = this.Id;
            var availability = $(this).find(".stat")[0].textContent;

            var location = new Microsoft.Maps.Location(latitude, longitude);
            locationArray.push(location);

            pushpinOptions.icon = getPushpinIcon(availability);
            var pushpin = bingMap.addPushPin(location, pushpinOptions);
            var infoBoxhtml = $("#info_" + this.id).html();

            var infoBox = new Microsoft.Maps.Infobox(location,
            {
                title: name,
                pushpin: pushpin,
                htmlContent: infoBoxhtml,
                //visible: false
            });

            bingMap.addInfoBox(infoBox);
            Microsoft.Maps.Events.addHandler(pushpin, 'click', bingMap.DisplayInfobox);

            var pushpinInfo = { "div": this, "pushpin": pushpin };

            availablePushpins.push(pushpinInfo);
        });

        bingMap.BoundingBoxView(locationArray);
    });
}

var getPushpinIcon = function (availability) {
    switch (availability) {
        case "online": return '/Content/Images/pushpins_0004_Online.png';
            break;
        case "offline": return '/Content/Images/pushpins_0003_Offline.png';
            break;
        case "decommissioned": return '/Content/Images/pushpins_0002_Decommission.png';
            break;
        case "upcoming": return '/Content/Images/pushpins_0001_Upcoming.png';
            break;
        case "downformaintenance": return '/Content/Images/pushpins_0000_Maintenance.png';
            break;
    }
}

$("#location").keydown(function (event) {
    var code = event.which; // recommended to use e.which, it's normalized across browsers    
    if (code == 13) {
        event.preventDefault();
        FindLocation();
    }
});

var FindLocation = function () {
    var searchRequest = {
        where: $('#location').val(),
        callback: onLocationFound,
        errorCallback: onLocationSearchFailed,
        userData: {
            callback: onSearchSuccess
        }
    }

    bingMap.locationFinder(searchRequest, bingMap.SearchRequestType.searchRequest);
};

var onLocationFound = function (searchResult, userData) {
    if (searchResult) {
        var searchRegion = searchResult.searchRegion;

        if (searchRegion) {

            if (searchRegion.matchCode == Microsoft.Maps.Search.MatchCode.none) {
                alert("Invalid Search");

                return;
            }

            if (searchRegion.address.countryRegion) {
                userData.callback(
                    searchRegion.explicitLocation.location.latitude,
                    searchRegion.explicitLocation.location.longitude,
                    searchRegion.address);
            }
            else {
                var reverseGeocoderequest = {
                    location: searchRegion.explicitLocation.location,
                    count: 10,
                    callback: geocodeCallback,
                    errorCallback: onSerachFailed,
                }

                bingMap.locationFinder(reverseGeocoderequest, bingMap.SearchRequestType.reverseGeocodeRequest);
            }
        }
        else {
            alert("Invalid Search");
        }
    }
};

var geocodeCallback = function (result, userData) {
    if (result.name) {
        var searchRequest = {
            where: result.name,
            callback: onLocationFound,
            errorCallback: onLocationSearchFailed,
            userData: { callback: onSearchSuccess }
        }

        bingMap.locationFinder(searchRequest, bingMap.SearchRequestType.searchRequest)
    }
    else {
        // Unknown location.
    }
};

var onSearchSuccess = function (latitude, longitude, address) {
    // Here we will get address, if search using latitude and longitude - If needed in future
    //LatitudeControl.val(latitude);
    //LongitudeControl.val(longitude);
    //if (AddressControl.val() == "") {
    //    AddressControl.val(address.formattedAddress);
    //}

    $("#StationList").removeClass("hidden");
    $("#homeLPane").addClass("hidden");
    $("#stationInfo").addClass("hidden");

    bingMap.ZoomtoLocation(latitude, longitude, 7);
};

var ZoomIn = function () {
    var currentCenterOfMap = bingMap.getMapCenter();
    var newZoom = bingMap.getMapZoom() + 1;
    bingMap.ZoomtoLocation(currentCenterOfMap.latitude, currentCenterOfMap.longitude, newZoom);
};

var ZoomOut = function () {
    var currentCenterOfMap = bingMap.getMapCenter();
    var newZoom = bingMap.getMapZoom() - 1;
    bingMap.ZoomtoLocation(currentCenterOfMap.latitude, currentCenterOfMap.longitude, newZoom);
};

var onViewChangeEnd = function (e) {
    var LocationRectViewPort = bingMap.getBoundries();
    $(".station").addClass("hidden");

    $(availablePushpins).each(function () {
        if (LocationRectViewPort.contains(this.pushpin._location)) {
            $(this.div).removeClass("hidden");
        }
    });
}

var onLocationSearchFailed = function (error) {
    //unimplemented
};

function onSerachFailed(result, userData) {
    //TODO: Display error messages here.
}

var LocateStation = function (latitude, longitude) {
    bingMap.ZoomtoLocation(latitude, longitude, 14);

    //find out pushpin
    var requiredpushpin = "";
    $(availablePushpins).each(function () {
        if (this.pushpin._location.latitude == latitude && this.pushpin._location.longitude == longitude) {
            requiredpushpin = this.pushpin;
        }
    });

    requiredpushpin.setOptions({ state: Microsoft.Maps.EntityState.selected, visible: true, offset: new Microsoft.Maps.Point(0, 25) });
    requiredpushpin._infobox.setLocation(requiredpushpin.getLocation());
}

var showStationInfo = function (stationId, accordianSection) {

    SpectrumObservatory.Utils.preloader.show();

    $("#stationDetailsFull").load("/Home/ViewStationDetails", { measurementStationId: stationId }, function () {

        $("#stationDetails").load("/Home/GetStationInfo", { stationId: stationId }, function () {

            var latitude = $("#siLatitude").text();
            var longitude = $("#siLongitude").text();

            bingMap.ZoomtoLocation(latitude, longitude, 14);

            $("#homeLPane").addClass("hidden");
            $("#StationList").addClass("hidden");
            $("#stationInfo").removeClass("hidden");

            var headerHeight = $(window).height() - $(".container").height();
            var chartHeight = $("#ChartAccordian").height();
            var problemHeight = $("#ProblemAccordian").height();
            var stationInfoHeight = $("#stationDetails").height();
            var rawdDataHeight = $("#RawDataAccordian").height();

            if (accordianSection == "chart") {
                $("#Accordion2").prop("checked", true);

                var chartGenerated = ChartsModule.isChartGenerated();
                if (chartGenerated) {
                    $("#mapSpace").addClass("hidden");
                    $("#SearchBox").addClass("hidden");
                    $("#map-controls").addClass("hidden");
                }

                var ht = $("#ChartAccordian").height() - $("#ChartAccordian").find("section").height();
                var totalHeight = headerHeight + chartHeight + problemHeight + rawdDataHeight + ht;
                $("#ChartAccordian").find("section").height($(window).height() - totalHeight - 20);

            }
            else if (accordianSection == "stationInfo") {
                var ht = $("#StationInfoAccordian").height() - $("#StationInfoAccordian").find("section").height();
                var totalHeight = headerHeight + chartHeight + problemHeight + rawdDataHeight + ht;
                $("#StationInfoAccordian").find("section").height($(window).height() - totalHeight - 20);

                ShowStationDetails();
            }
            else if (accordianSection == "rawdata") {
                $("#Accordion4").prop("checked", true);

                var ht = $("#RawDataAccordian").height() - $("#RawDataAccordian").find("section").height();
                var totalHeight = headerHeight + chartHeight + problemHeight + rawdDataHeight + ht;
                $("#RawDataAccordian").find("section").height($(window).height() - totalHeight - 20);

                ShowRawData();
            }
        });

    });
    setTimeout(function () {
        SpectrumObservatory.Utils.preloader.hide();
    }, 4000);
    // SpectrumObservatory.Utils.preloader.hide();
}

var isGrapghResized = false;

var MoreStationInfo = function () {
    $("#liMoreInfo").toggleClass("hidden");
}

var ShowMap = function () {
    $("#stationDetailsFull").addClass("hidden");
    $("#chartSpace").addClass("hidden");
    $("#mapSpace").removeClass("hidden");
    $("#SearchBox").removeClass("hidden");
    $("#map-controls").removeClass("hidden");
    $("#problemSpace").addClass("hidden");
    $("#rawDataSpace").addClass("hidden");
}

var ShowChart = function () {
    $("#chartSpace").removeClass("hidden");
    $("#problemSpace").addClass("hidden");
    $("#stationDetailsFull").addClass("hidden");
    $("#rawDataSpace").addClass("hidden");
    isGrapghResized = false;

    var chartGenerated = ChartsModule.isChartGenerated();
    if (chartGenerated) {
        $("#mapSpace").addClass("hidden");
        $("#SearchBox").addClass("hidden");
        $("#map-controls").addClass("hidden");
        isGrapghResized = true;
    }
    else {
        $("#mapSpace").removeClass("hidden");
        $("#SearchBox").removeClass("hidden");
        $("#map-controls").removeClass("hidden");
    }
}

var ShowRawData = function (e) {
    
    //$.getJSON("/Home/AllowRawDataDownload", { stationId: $("#siStationId").val() }, function (result) {
    //    if (result.allowDownload === false) {
    //        $("#rawDataType option[value='1']").remove();
    //    }
    //}).fail(function () {
    //    alert("call failed");
    //    $("#rawDataType option[value='1']").remove();
    //}).always(function () {        
       
    //});

    $("#rawDataSpace").removeClass("hidden");
    $("#stationDetailsFull").addClass("hidden");
    $("#problemSpace").addClass("hidden");
    $("#chartSpace").addClass("hidden");
    $("#mapSpace").addClass("hidden");
    $("#SearchBox").addClass("hidden");
    $("#map-controls").addClass("hidden");

   // e.stopPropagation();
}

var ShowReportForm = function () {
    $("#problemSpace").removeClass("hidden");
    $("#stationDetailsFull").addClass("hidden");
    $("#chartSpace").addClass("hidden");
    $("#mapSpace").addClass("hidden");
    $("#SearchBox").addClass("hidden");
    $("#map-controls").addClass("hidden");
    $("#rawDataSpace").addClass("hidden");

    getUserInfo();
}

var ShowStationDetails = function () {
    $("#problemSpace").addClass("hidden");
    $("#chartSpace").addClass("hidden");
    $("#mapSpace").addClass("hidden");
    $("#SearchBox").addClass("hidden");
    $("#map-controls").addClass("hidden");
    $("#rawDataSpace").addClass("hidden");
    $("#stationDetailsFull").removeClass("hidden");
}

$('.decrement').click(function () {
    var currentValue = $("#slider-range-min").slider("value");
    $("#slider-range-min").slider("value", currentValue - 1);
});

$('.increment').click(function (e) {

    var currentValue = $("#slider-range-min").slider("value");

    if (currentValue >= 23) {
        return false;
    }

    $("#slider-range-min").slider("value", currentValue + 1);
});

$('.rawDataDecrement').click(function () {
    var currentValue = $("#rawDataslider-range-min").slider("value");
    $("#rawDataslider-range-min").slider("value", currentValue - 1);
});

$('.rawDataIncrement').click(function (e) {

    var currentValue = $("#rawDataslider-range-min").slider("value");

    if (currentValue >= 23) {
        return false;
    }

    $("#rawDataslider-range-min").slider("value", currentValue + 1);
});

var InitializeSlider = function () {
    var currentUtcTime = new Date();
    $('.slider-container button').show();
    $("#slider-range-min").slider({
        min: 0,
        max: 24,
        value: currentUtcTime.getUTCHours(),
        slide: function (event, ui) {
            if (ui.value >= 24) {
                event.preventDefault();
            }
            else {
                updateTimescaleValue(ui.value);
            }
        },
        change: function (event, ui) {
            updateTimescaleValue(ui.value);
        }
    });

    $('.ui-slider').removeClass("custom");

    updateTimescaleValue($("#slider-range-min").slider("option", "value"));

    var currentUtcTime = new Date();
    $('.rawDataSlider-container button').show();
    $("#rawDataslider-range-min").slider({
        min: 0,
        max: 24,
        value: currentUtcTime.getUTCHours(),
        slide: function (event, ui) {
            if (ui.value >= 24) {
                event.preventDefault();
            }
            else {
                updateRawDataTimescaleValue(ui.value);
            }
        },
        change: function (event, ui) {
            updateRawDataTimescaleValue(ui.value);
        }
    });

    $('.ui-slider').removeClass("custom");

    updateTimescaleValue($("#rawDataslider-range-min").slider("option", "value"));
}

var updateTimescaleValue = function (value) {
    $("#amount").val(value + ":00 - " + (value + 1) % 24 + ":00 Hrs");
}

var updateRawDataTimescaleValue = function (value) {
    $("#amountRawData").val(value + ":00 - " + (value + 1) % 24 + ":00 Hrs");
}

var ReportProblem = function () {
    $(".err").addClass("hidden");

    var inputValid = validateProblemInputs();

    if (!inputValid) {
        return;
    }

    var issueData = getProblemData();

    // save Problem

    $.ajax({
        url: '/Home/ReportProblem',
        data: issueData,
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        beforeSend: function () {
            // TODO: Enable busy cursor here.
        },
        success: function (result) {
            $("#message").text("Problem Reported Successfully")
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $("#message").text("Problem Report Failed")
        }
    });
}

var validateProblemInputs = function () {

    var isInputValid = true;

    if ($("#rpFirstName").val() == "") {
        $("#errrpFirstName").removeClass("hidden");
        isInputValid = false;
    }

    if ($("#rpEmail").val() == "") {
        $("#errrpEmail").removeClass("hidden");
        isInputValid = false;
    }

    if ($("#rpSubject").val() == "") {
        $("#errrpSubject").removeClass("hidden");
        isInputValid = false;
    }

    if ($("#rpComment").val() == "") {
        $("#errrpComment").removeClass("hidden");
        isInputValid = false;
    }

    return isInputValid;
}

var getProblemData = function () {
    var now = new Date();

    var data = $('#problemInput').serialize();

    var problemData =
        {
            ReportDate: new Date(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate(),
                         now.getUTCHours(), now.getUTCMinutes(), now.getUTCSeconds(), now.getUTCMilliseconds()),
            FirstName: $("#rpFirstName").val(),
            LastName: $("#rpLastName").val(),
            Email: $("#rpEmail").val(),
            Subject: $("#rpSubject").val(),
            IssueDescription: $("#rpComment").val(),
            StationId: $("#siStationId").val()
        };

    return JSON.stringify(problemData);
}

var getUserInfo = function () {
    var jqxhr = $.getJSON("/Home/GetUserInfo", function (result) {
        if (result.Result == "Success") {
            var userInfo = result.Data;

            $("#rpFirstName").val(userInfo.FirstName);
            $("#rpLastName").val(userInfo.LastName);
            $("#rpEmail").val(userInfo.Email);
            $("#rpPhone").val(userInfo.Phone);
        }
    })

           .fail(function (result) {
               alert("error");
           });
}

var getRawDataInputData = function () {

    var slectedDateString = $("#dateRawData").val();
    var dateParts = slectedDateString.match(/(\d+)/g);
    var years = parseInt(dateParts[0], 10);
    var months = parseInt(dateParts[1], 10) - 1;
    var days = parseInt(dateParts[2], 10);

    var RawDataInput = {
        TimeScale: $('input[name="rawDataTimeScale"]:checked', '#rawDataTimeScaleSelector').val(),
        StartDateIso: new Date(Date.UTC(years, months, days, 0, 0, 0, 0)).toISOString(),
        StartDate: $("#dateRawData").val(),
        StartTime: $("#rawDataslider-range-min").slider("value"),
        MeasurementStationId: $("#siStationId").val(),
        TypeId: $("#rawDataType").val()
    };

    return RawDataInput;
};

var validateRawDataInputs = function () {
    var isValid = true;

    if ($("#dateRawData").val() == "") {
        $("#errRawDataDate").removeClass("hidden");
        isValid = false;
    }

    return isValid;
};

var onViewRawData = function () {
    $(".error").addClass("hidden");
    var isFormValid = validateRawDataInputs();

    if (!isFormValid) {
        return;
    }

    SpectrumObservatory.Utils.preloader.show();

    var inputData = getRawDataInputData();

    $("#rawData").load("/Home/All", { input: inputData }, function () {
        SpectrumObservatory.Utils.preloader.hide();
    });
}

var onTimerangeSelection = function () {
    var timeScale = $('input[name="timeScale"]:checked', '#timeScaleSelector').val();

    if (timeScale == 0) {
        $("#liSpinner").removeClass("hidden");
    }
    else {
        $("#liSpinner").addClass("hidden");
    }
}


var onRawDataTimerangeSelection = function () {
    var timeScale = $('input[name="rawDataTimeScale"]:checked', '#rawDataTimeScaleSelector').val();

    if (timeScale == 0) {
        $("#liRawDataSpinner").removeClass("hidden");
    }
    else {
        $("#liRawDataSpinner").addClass("hidden");
    }
}

var AdjustAccordianHeight = function (accordianName) {
    // Minimize all sections

    $("#StationInfoAccordian").find("section").height(0);
    $("#ChartAccordian").find("section").height(0);
    $("#ProblemAccordian").find("section").height(0);
    $("#RawDataAccordian").find("section").height(0);

    var headerHeight = $(window).height() - $(".container").height();
    var chartHeight = $("#ChartAccordian").height();
    var problemHeight = $("#ProblemAccordian").height();
    var stationInfoHeight = $("#StationInfoAccordian").height();
    var rawDataHeight = $('#RawDataAccordian').height();

    var sectionHeight = "";

    switch (accordianName) {
        case "StationInfoAccordian":
            {
                var ht = $("#StationInfoAccordian").height() - $("#StationInfoAccordian").find("section").height();
                sectionHeight = headerHeight + (3 * Math.min(chartHeight, problemHeight, rawDataHeight)) + ht + 20;
                break;
            }
        case "ChartAccordian":
            {
                var ht = $("#ChartAccordian").height() - $("#ChartAccordian").find("section").height();
                sectionHeight = headerHeight + (3 * Math.min(stationInfoHeight, problemHeight, rawDataHeight)) + ht + 20;
                break;
            }
        case "ProblemAccordian":
            {
                var ht = $("#ProblemAccordian").height() - $("#ProblemAccordian").find("section").height();
                sectionHeight = headerHeight + (3 * Math.min(stationInfoHeight, chartHeight, rawDataHeight)) + ht + 20;
                break;
            }
        case "RawDataAccordian":
            {
                var ht = $("#RawDataAccordian").height() - $("#RawDataAccordian").find("section").height();
                sectionHeight = headerHeight + (3 * Math.min(stationInfoHeight, chartHeight, problemHeight)) + ht + 20;
                break;
            }
    }

    var id = "#" + accordianName;
    $(id).find("section").height($(window).height() - sectionHeight);
}

$(document).ready(function () {
    $.ajaxSetup({ cache: false });
    $("#SearchList").height($(window).height() - ($(window).height() * 0.12));
    // TODO: Enable this later.  
    //SpectrumObservatory.Utils.slimScroll.enableSlimScroll('#SearchList');
    //SpectrumObservatory.Utils.slimScroll.enableSlimScroll('#StationInfoAccordian');

    //SpectrumObservatory.Utils.slimScroll.enableSlimScroll('#left-pane .container');
});

$(window).resize(function () {
    // TODO: Enable this later.
    $("#SearchList").height($(window).height() - ($(window).height() * 0.12));

    var selectedSection = $('input[name="stInfo"]:checked', '#stationInfo').val();
    if (selectedSection != undefined) {

        var headerHeight = $(window).height() - $("#stationInfo").height();
        var chartHeight = $("#ChartAccordian").height();
        var problemHeight = $("#ProblemAccordian").height();
        var stationInfoHeight = $("#StationInfoAccordian").height();
        var rawDataHeight = $('#RawDataAccordian').height();

        var totalHeight = 0;
        switch (selectedSection) {
            case "StationInfoAccordian":
                {
                    totalHeight = chartHeight + problemHeight - headerHeight;
                    break;
                }
            case "ChartAccordian":
                {
                    // Graph resize calling window resize
                    if (isGrapghResized) {
                        isGrapghResized = false;
                        return;
                    }
                    totalHeight = stationInfoHeight + problemHeight - headerHeight;
                    break;
                }
            case "ProblemAccordian":
                {
                    totalHeight = chartHeight + stationInfoHeight - headerHeight;
                    break;
                }
            case "RawDataAccordian":
                {
                    totalHeight = chartHeight + rawDataHeight - headerHeight;
                    break;
                }
        }

        var id = "#" + selectedSection;
        $(id).find("section").height($(id).find("section").height() - totalHeight);
        //SpectrumObservatory.Utils.slimScroll.enableSlimScroll(id);
    }
});