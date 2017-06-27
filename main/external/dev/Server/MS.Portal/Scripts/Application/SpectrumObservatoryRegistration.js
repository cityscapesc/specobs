/// <reference path="Utils.js" />
/// <reference path="SpectrumObservatoryRegistrationCommon.js"/>

SpectrumObservatory.Utils = (function (util) {
    "use strict";

    util = util || {};

    var stationRegistration = util.StationRegistration || {};

    var stationRegistrationUrls = {
        StationRegistration: "/SpectrumObservatoryRegistration/Register",
        LoadRawIQPolicy: "/SpectrumObservatoryRegistration/LoadRawIQScanPolicy"
    };

    stationRegistration.Startups = function () {
        var firstRFSensorConfigIndex,
            radioTypeSelector,
            cableTypeIndex,
            cableTypeSelector;

        // To keep current left navigation bar tab active.
        $("#Radio3").attr('checked', 'checked');

        // Enable default scroll as we already using jQuery slim scroll.
        SpectrumObservatory.Utils.defaultScroll.disableDefaultScroll();

        radioTypeSelector = "#radioType";
        firstRFSensorConfigIndex = $("input[name='RFSensorConfigurationEndToEnd.Index']:first").val();
        cableTypeIndex = $("input[name='RFSensorConfigurationEndToEnd[" + firstRFSensorConfigIndex + "].Cables.Index'").val();
        cableTypeSelector = "input[name='RFSensorConfigurationEndToEnd[" + firstRFSensorConfigIndex + "].Cables[" + cableTypeIndex + "].CableType']:first";

        SpectrumObservatory.Utils.StationRegistration.AutoCompleteFields.enableAutoCompleteForRadioType(radioTypeSelector);
        SpectrumObservatory.Utils.StationRegistration.AutoCompleteFields.enableAutoCompleteForCableType(cableTypeSelector);
    }

    stationRegistration.RegisterEventHandlers = function () {
        var settings,
            wizard,
            nextStepIndex,
            rawIqConfigEnabled,
            inputsToBeValidated;

        // wizardDomSelector                : The DOM container which contains Wizard steps DOM elements.
        // stepNames                        : An array of labels given to each step in the Wizard control
        // stepNameAsTransitionButtonName   : A boolean to change step transition buttons names to its corresponding step names on each transition.
        // stepIndicatorDomSelector         : The Dom container used to indicate wizard step status i.e completed or not.
        settings = {
            wizardDomSelector: "#right-pane #body .container #registrationSteps",
            stepNames: ["Overview", "Device Setup", "Software Download And Install", "Registration"],
            stepNameAsTransitionButtonName: true,
            stepIndicatorDomSelector: ".wizard"
        };

        nextStepIndex = 0;
        wizard = SpectrumObservatory.Utils.wizard;
        wizard.initializWizard(settings);

        SpectrumObservatory.Utils.wizard.onViewUpdated = function (event) {
            var stepName = '';

            // Capture next step index for future use.
            nextStepIndex = event.nextStepIndex;

            if (event.prevStepIndex >= 0) {
                $('#transitionButtons .preview').text(event.prevStepName);
                $('#transitionButtons .preview').show();
            }
            else {
                $('#transitionButtons .preview').hide();
            }

            if (event.nextStepIndex != -1 && event.nextStepIndex <= settings.stepNames.length) {
                stepName = event.nextStepName;

                if (event.nextStepIndex === settings.stepNames.length) {
                    stepName = "Submit";
                }

                $('#transitionButtons .next').text(stepName);
                $('#transitionButtons .next').show();
            }
            else {
                $('#transitionButtons .next').hide();
            }
        };

        SpectrumObservatory.Utils.wizard.onStepCompleted = function (event) {
            if (event.Step) {
                event.Step.children('input').prop('checked', true);
            }
        }

        $('.wizard li').click(function (event) {
            var index = $(this).index(".wizard li");

            if (!wizard) {
                return;
            }

            if ($(this).attr("status") != "completed") {
                event.preventDefault();
                return;
            }

            wizard.jumpToStep(index);
        });

        $('#transitionButtons .next').click(function () {
            var registrationInputs,
                psdConfigEnabled;

            if (!wizard) {
                return;
            }

            // When last wizard step reached, submit a form
            if (nextStepIndex === settings.stepNames.length) {
                registrationInputs = $('#stationRegistration').serialize();

                $('#stationRegistration input[type=radio]').each(function () {
                    if (this.checked) {
                        registrationInputs += '&' + this.name + '=true';
                    }
                });

                psdConfigEnabled = $("input:radio[name='ClientAggregationConfiguration.OutputData']:checked").val() === "True" ? true : false;

                // enable or disable PSD configuration controls based on outputdata is selected or not.
                if (!psdConfigEnabled) {
                    inputsToBeValidated = $('#stationRegistration .mandatory input').not('#psdConfig input');

                    $("input[name='ClientAggregationConfiguration.MinutesOfDataPerScanFile']").val(60);
                    $("input[name='ClientAggregationConfiguration.SecondsOfDataPerSample']").val(60);

                } else {
                    inputsToBeValidated = $('#stationRegistration .mandatory input').not('#rawIqConfig input');;
                }

                if (inputsToBeValidated.valid()) {
                    SpectrumObservatory.Utils.ajaxCall.doPost(stationRegistrationUrls.StationRegistration, registrationInputs)
                                                      .then(function (data) {
                                                          if (data.ViewData && !data.ServerValidationErrors) {
                                                              $('#registrationSteps').empty();
                                                              $('#registrationSteps').html(data.ViewData);

                                                              $('#transitionButtons .preview').hide();
                                                              $('#transitionButtons .next').hide();

                                                              wizard.wizardComplete();
                                                              wizard = null;

                                                              $('.wizard li').unbind('click');
                                                          }
                                                          else if (data.ViewData && data.ServerValidationErrors) {
                                                              $('#registrationSteps').empty();
                                                              $('#registrationSteps').html(data.ViewData);

                                                              SpectrumObservatory.Utils.StationRegistration.AjaxCallbacks.enableUnobtrusiveValidation();
                                                              $('#stationRegistration .mandatory input').valid()
                                                          }
                                                          else if (data.ErrorMessage) {
                                                              // TODO: Have to replace this alert box with notification bar.
                                                              alert(data.ErrorMessage);
                                                          }
                                                      });
                }
            }

            wizard.next();
        });

        $('#transitionButtons .preview').click(function () {
            if (!wizard) {
                return;
            }

            wizard.prev();
        });

        $('.device select').change(SpectrumObservatory.Utils.StationRegistration.Callbacks.onDeviceTypeChanged);

        $('#loadIQPolicyBtn').click(function () {
            SpectrumObservatory.Utils.StationRegistration.Callbacks.onLoadRawIQPolicyClick(stationRegistrationUrls.LoadRawIQPolicy);
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
    };

    util.StationRegistration = stationRegistration;
    return util;

}(SpectrumObservatory.Utils));

$(document).ready(function () {
    SpectrumObservatory.Utils.StationRegistration.Startups();
    SpectrumObservatory.Utils.slimScroll.enableSlimScroll('#body');
    SpectrumObservatory.Utils.StationRegistration.RegisterEventHandlers();
});

$(window).resize(function () {
    SpectrumObservatory.Utils.slimScroll.resizeSlimScroll('#body');
});