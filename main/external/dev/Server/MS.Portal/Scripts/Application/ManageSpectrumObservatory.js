/// <reference path="Utils.js" />
/// <reference path="SpectrumObservatoryRegistrationCommon.js"/>

SpectrumObservatory.Utils = (function (util) {
    "use strict";

    util = util || {};

    var stationRegistrationDetails = util.StationRegistrationDetails || {};

    var manageStationRegistrationUrls = {
        EditStationDetails: "/Manage/EditStationDetails",
        ManageSpectrumObservatories: "/Manage/BackToManage",
        LoadRawIQPolicy: "/SpectrumObservatoryRegistration/LoadRawIQScanPolicy"
    };

    stationRegistrationDetails.Startups = function () {
        $("#Radio4").attr('checked', 'checked');
    };

    stationRegistrationDetails.RegisterEventHandlers = function () {
        var registrationInputs,
            radioTypeSelector,
            rfSenorConfigs,
            rawIqConfigEnabled,
            inputsToBeValidated;

        radioTypeSelector = "#radioType";
        rfSenorConfigs = $("input[name='RFSensorConfigurationEndToEnd.Index']");

        // Logic to enable auto-complete for the existing cable types in the RF Sensor Configuration list.
        rfSenorConfigs.each(function (index, value) {
            var rfSensorConfigIndex,
                cableTypeIndex,
                cables,
                cableTypeSelector;

            rfSensorConfigIndex = $(this).val();
            cables = $("input[name='RFSensorConfigurationEndToEnd[" + rfSensorConfigIndex + "].Cables.Index'")

            cables.each(function (index, value) {
                cableTypeIndex = $(this).val();
                cableTypeSelector = "input[name='RFSensorConfigurationEndToEnd[" + rfSensorConfigIndex + "].Cables[" + cableTypeIndex + "].CableType']";

                SpectrumObservatory.Utils.StationRegistration.AutoCompleteFields.enableAutoCompleteForCableType(cableTypeSelector);
            });
        });

        SpectrumObservatory.Utils.StationRegistration.AutoCompleteFields.enableAutoCompleteForRadioType(radioTypeSelector);

        var onUpdateClick = function (event) {
            var psdConfigEnabled;

            psdConfigEnabled = $("input:radio[name='ClientAggregationConfiguration.OutputData']:checked").val() === "True" ? true : false;            

            // enable or disable PSD configuration controls based on outputdata is selected or not.
            if (!psdConfigEnabled) {
                inputsToBeValidated = $('#stationRegistration .mandatory input').not('#psdConfig input');

                $("input[name='ClientAggregationConfiguration.MinutesOfDataPerScanFile']").val(60);
                $("input[name='ClientAggregationConfiguration.SecondsOfDataPerSample']").val(60);

            } else {
                inputsToBeValidated = $('#stationRegistration .mandatory input').not('#rawIqConfig input');
            }

            if (inputsToBeValidated.valid()) {
                registrationInputs = $('#stationRegistration').serialize();

                $('#stationRegistration input[type=radio]').each(function () {
                    if (this.checked) {
                        registrationInputs += '&' + this.name + '=true';
                    }
                });

                SpectrumObservatory.Utils.ajaxCall.doPost(manageStationRegistrationUrls.EditStationDetails, registrationInputs)
                                                  .then(function (data) {
                                                      if (data.ViewData && !data.ServerValidationErrors) {
                                                          // TODO: Back to list of measurement stations.
                                                          $('#rightPanePlaceholder').empty();
                                                          $('#rightPanePlaceholder').html(data.ViewData);

                                                          $('#transitionButtons .preview').hide();
                                                          $('#transitionButtons .next').hide();

                                                          $('#transitionButtons .next').click(onUpdateClick);
                                                          $('#transitionButtons .prev').click();
                                                      }
                                                      if (data.ViewData && data.ServerValidationErrors) {
                                                          $('#rightPanePlaceholder').empty();
                                                          $('#rightPanePlaceholder').html(data.ViewData);

                                                          SpectrumObservatory.Utils.StationRegistration.AjaxCallbacks.enableUnobtrusiveValidation();
                                                          $('#stationRegistration .mandatory input').valid()

                                                          $('#transitionButtons .next').click(onUpdateClick);
                                                          $('#transitionButtons .prev').click();
                                                      }
                                                      else if (data.ErrorMessage) {
                                                          // TODO: Have to replace this alert box with notification bar.
                                                          alert(data.ErrorMessage);
                                                          console.log(data.ErrorMessage);
                                                      }
                                                  });
            }
        }

        var onCancelClick = function (event) {
            SpectrumObservatory.Utils.ajaxCall.doPost(manageStationRegistrationUrls.ManageSpectrumObservatories, registrationInputs)
                                                 .then(function (viewData) {
                                                     if (viewData) {
                                                         $('#rightPanePlaceholder').empty();
                                                         $('#rightPanePlaceholder').html(viewData);
                                                     }

                                                     $('#transitionButtons .next').click(onUpdateClick);
                                                     $('#transitionButtons .prev').click(onCancelClick);

                                                 }).fail(function (jqXHR, textStatus, errorThrown) {
                                                     alert(errorThrown);

                                                     console.log(errorThrown);
                                                 });
        }

        $('#transitionButtons .next').click(onUpdateClick);

        $('#transitionButtons .prev').click(onCancelClick);

        $('.device select').change(SpectrumObservatory.Utils.StationRegistration.Callbacks.onDeviceTypeChanged);

        $('#loadIQPolicyBtn').click(function () {
            SpectrumObservatory.Utils.StationRegistration.Callbacks.onLoadRawIQPolicyClick(manageStationRegistrationUrls.LoadRawIQPolicy);
        });

        $("#outputPsd,#donotOutRawIq").change(function () {
            if ($(this).prop("checked")) {
                SpectrumObservatory.Utils.StationRegistration.OutputFileConfigSettings.EnablePSD();
            }
        });

        $("#outputRawIq,#donotOutputPsd").change(function () {
            if ($(this).prop("checked")) {
                SpectrumObservatory.Utils.StationRegistration.OutputFileConfigSettings.EnableRawIQ();
            }
        });
    }

    stationRegistrationDetails.AjaxCallbacks = (function () {

        var onEditCallBegin = function (data, status, xhr) {
            SpectrumObservatory.Utils.preloader.show();
        }

        var onEditCallCompleted = function (data, status, xhr) {
            SpectrumObservatory.Utils.preloader.hide();
        }

        var onEditSpectrumObservatoryDetails = function (data, status, xhr) {
            SpectrumObservatory.Utils.StationRegistrationDetails.RegisterEventHandlers();
            SpectrumObservatory.Utils.StationRegistration.AjaxCallbacks.enableUnobtrusiveValidation();
        }

        return {
            onEditCallBegin: onEditCallBegin,
            onEditCallCompleted: onEditCallCompleted,
            onEditSpectrumObservatoryDetails: onEditSpectrumObservatoryDetails
        };
    }());

    util.StationRegistrationDetails = stationRegistrationDetails;

    return util;

}(SpectrumObservatory.Utils));

$(document).ready(function () {
    SpectrumObservatory.Utils.StationRegistrationDetails.Startups();
    SpectrumObservatory.Utils.slimScroll.enableSlimScroll('#body');

    // Enable default scroll as we already using jQuery slim scroll.
    SpectrumObservatory.Utils.defaultScroll.disableDefaultScroll();
});

$(window).resize(function () {
    SpectrumObservatory.Utils.slimScroll.resizeSlimScroll('#body');
});