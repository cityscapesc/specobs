/// <reference path="Utils.js" />
SpectrumObservatory.Utils = (function (util) {
    "use strict";

    util = util || {};

    var stationRegistration,
        scannerRadioTypes,
        cableTypes;

    stationRegistration = util.StationRegistration || {};

    scannerRadioTypes = [
          "Ettus USRP N200 WBX & SBX",
          "CRFS RFeye Node"
    ];

    cableTypes = [
            "8215 (RG-6A)",
            "8237 (RG-8)",
            "9913 (RG-8)",
            "9258 (RG-8X)",
            "8213 (RG-11)",
            "8261 (RG-11A)",
            "8240 (RG-58)",
            "9201 (RG-58)",
            "8219 (RG-58A)",
            "8259 (RG-58C)",
            "8212 (RG-59)",
            "8263 (RG-59B)",
            "9269 (RG-62A)",
            "83241 (RG-141A)",
            "8216 (RG-174)",
            "8267 (RG-213)",
            "9913F7",
            "7810A",
            "7808A",
            "Davis RF Bury-Flex",
            "LMR-100A",
            "LMR-200",
            "LMR-240",
            "LMR-400",
            "LMR-600",
            "LMR-900",
            "CQ102 (RG-8)",
            "CQ106 (RG-8)",
            "CQ125 (RG-58)",
            "CQ127 (RG-58C)",
            "CQ110 (RG-213)",
            "Tandy Cable RG-8X",
            "Tandy Cable RG-58",
            "Tandy Cable RG-59",
            "Heliax LDF4-50A",
            "Heliax LDF5-50A",
            "Heliax LDF6-50A",
            "Wireman 551",
            "Wireman 552",
            "Wireman 553",
            "Wireman 554",
            "Wireman 551 (wet)",
            "Wireman 552 (wet)",
            "Wireman 553 (wet)",
            "Wireman 554 (wet)",
            "Generic 300 ohm Tubular",
            "Generic 450 ohm Window",
            "Generic 600 ohm Open",
            "Ideal (lossless) 50 ohm",
            "Ideal (lossless) 75 ohm",
    ];

    stationRegistration.Callbacks = (function () {

        var deviceTypes,
            usrpDefaultConfig,
            rfExplorerDefaultConfig;

        deviceTypes = {
            Usrp: "USRP",
            RFExplorer: "RFExplorer"
        };

        usrpDefaultConfig = {
            AntennaPort: "RX2",
            CommunicationChannel: "addr",
            SmaplesPerScan: "1024"
        };

        rfExplorerDefaultConfig = {
            AntennaPort: "N/A",
            CommunicationChannel: "COM3",
            SmaplesPerScan: "112"
        };

        var onDeviceTypeChanged = function () {
            var parentElelementIndex,
                antennaPortSelector,
                samplesPerScanSelector,
                communicationChannelSelector,
                getSelectorbyName;

            getSelectorbyName = function (elementName) {
                return "input[name=" + "'" + parentElelementIndex + "." + elementName + "']";
            }

            parentElelementIndex = $(this).attr('name').split('.')[0];

            antennaPortSelector = getSelectorbyName("AntennaPort");
            samplesPerScanSelector = getSelectorbyName("SamplesPerScan");
            communicationChannelSelector = getSelectorbyName("CommunicationsChannel");

            if ($(this).val().toLowerCase() === deviceTypes.Usrp.toLowerCase()) {

                $(antennaPortSelector).val(usrpDefaultConfig.AntennaPort);
                $(samplesPerScanSelector).val(usrpDefaultConfig.SmaplesPerScan);
                $(communicationChannelSelector).val(usrpDefaultConfig.CommunicationChannel);
            }
            else if ($(this).val().toLowerCase() === deviceTypes.RFExplorer.toLowerCase()) {

                $(antennaPortSelector).val(rfExplorerDefaultConfig.AntennaPort);
                $(samplesPerScanSelector).val(rfExplorerDefaultConfig.SmaplesPerScan);
                $(communicationChannelSelector).val(rfExplorerDefaultConfig.CommunicationChannel);
            }
        };

        var loadRawIQPolicy = function (postUrl) {
            var rawIqInputs,
                rawIQConfigEnabled,
                inputFiledToBeValidated = $('#rawIqConfig .mandatory input');

            rawIQConfigEnabled = $("input:radio[name='RawIqDataConfiguration.OutputData']:checked").val() === "True" ? true : false;

            if (rawIQConfigEnabled
                && inputFiledToBeValidated.valid()) {

                rawIqInputs = $('#rawIqConfig input').serialize();

                $('#rawIqConfig input[type=radio]').each(function () {
                    if (this.checked) {
                        rawIqInputs = rawIqInputs + '&' + this.name + '=true';
                    }
                });

                SpectrumObservatory.Utils.ajaxCall.doPost(postUrl, rawIqInputs)
                                                     .then(function (data) {
                                                         if (data.ViewData && !data.ServerValidationErrors) {
                                                             var policyBtnParent = $("#loadIQPolicyBtn").parent("label").parent("li");

                                                             policyBtnParent.nextAll().remove();
                                                             policyBtnParent.after(data.ViewData);

                                                             SpectrumObservatory.Utils.StationRegistration.AjaxCallbacks.enableUnobtrusiveValidation();
                                                         }
                                                         else if (data.ViewData && data.ServerValidationErrors) {
                                                             var policyBtnParent = $("#loadIQPolicyBtn").parent("li");

                                                             policyBtnParent.next().remove();
                                                             policyBtnParent.after(data.ViewData);

                                                             SpectrumObservatory.Utils.StationRegistration.AjaxCallbacks.enableUnobtrusiveValidation();
                                                             $('#rawIqConfig .mandatory input').valid();
                                                         }
                                                         else {
                                                             alert(data.ErrorMessage);
                                                         }
                                                     });
            }
        }

        return {
            onDeviceTypeChanged: onDeviceTypeChanged,
            onLoadRawIQPolicyClick: loadRawIQPolicy
        };
    }());

    stationRegistration.OutputFileConfigSettings = (function () {
        var enablePsdFileOutput,
            enableRawIqFileOutput;

        enablePsdFileOutput = function () {
            $("#outputPsd").prop("checked", true);
            $("#donotOutRawIq").prop("checked", true);
        }

        enableRawIqFileOutput = function () {
            $("#outputRawIq").prop("checked", true);
            $("#donotOutputPsd").prop("checked", true);
        }

        return {
            EnablePSD: enablePsdFileOutput,
            EnableRawIQ: enableRawIqFileOutput
        };
    }());

    stationRegistration.AjaxCallbacks = (function () {
        var enableUnobtrusiveValidation = function () {
            var selector = '#registrationForm form';

            // It is very important to clear validator and unobstrusivValidation form data before calling resetting client validation, 
            // otherwise validations will not be performed for newly added HTML contents on Ajax updates.
            $(selector).removeData('validator');
            $(selector).removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse(selector);
        }

        var onCableAdded = function (data, status, xhr) {
            var cableElementName,
                cableElementIndex,
                cableSelector;

            cableElementName = $(data).attr('name')
            cableElementIndex = cableElementName.replace(".Index", "") + "[" + $("input[name='" + cableElementName + "']:last").val() + "].CableType";
            cableSelector = "input[name='" + cableElementIndex + "']";

            enableUnobtrusiveValidation();
            SpectrumObservatory.Utils.StationRegistration.AutoCompleteFields.enableAutoCompleteForCableType(cableSelector);
        }

        var onConnectorAdded = function (data, status, xhr) {
            enableUnobtrusiveValidation();
        }

        var onAntennaAdded = function (data, status, xhr) {
            var antennaElementName,
                antennaElementIndex,
                antennaSelector;

            antennaElementName = $(data).attr('name');
            antennaElementIndex = antennaElementName.replace(".Index", "") + "[" + $("input[name='" + antennaElementName + "']:last").val() + "].AntennaType";
            antennaSelector = "input[name'" + antennaElementIndex + "']";

            enableUnobtrusiveValidation();
        }

        var onSensorConfigurationAdded = function (data, status, xhr) {

            var rfSensorConfigIndex,
                deviceSelector,
                cableElementName,
                cableElementIndex,
                cableSelector;

            // Find an index of the recent RFSensorEndToEndConfiguration.
            rfSensorConfigIndex = $("input[name='RFSensorConfigurationEndToEnd.Index']:last").val();
            deviceSelector = "select[name='RFSensorConfigurationEndToEnd[" + rfSensorConfigIndex + "].DeviceType']";

            // Find an index of the first CableType within the recent RFSensorEndToEndConfiguration element.
            cableElementName = "RFSensorConfigurationEndToEnd[" + rfSensorConfigIndex + "].Cables.Index";
            cableElementIndex = cableElementName.replace(".Index", "") + "[" + $("input[name='" + cableElementName + "']:first").val() + "].CableType";
            cableSelector = "input[name='" + cableElementIndex + "']";

            enableUnobtrusiveValidation();
            $(deviceSelector).change(SpectrumObservatory.Utils.StationRegistration.Callbacks.onDeviceTypeChanged);
            SpectrumObservatory.Utils.StationRegistration.AutoCompleteFields.enableAutoCompleteForCableType(cableSelector);
        }

        return {
            onCableAdded: onCableAdded,
            onConnectorAdded: onConnectorAdded,
            onAntennaAdded: onAntennaAdded,
            onSensorConfigurationAdded: onSensorConfigurationAdded,
            enableUnobtrusiveValidation: enableUnobtrusiveValidation
        };
    }());

    stationRegistration.AutoCompleteFields = (function () {

        var enableAutoCompleteForRadioType,
            enableAutoCompleteForCableType;

        enableAutoCompleteForRadioType = function (selector) {
            $(selector).autocomplete({
                source: scannerRadioTypes
            });
        }

        enableAutoCompleteForCableType = function (selector) {
            $(selector).autocomplete({
                source: cableTypes
            })
        }

        return {
            enableAutoCompleteForCableType: enableAutoCompleteForCableType,
            enableAutoCompleteForRadioType: enableAutoCompleteForRadioType
        };

    }());

    util.StationRegistration = stationRegistration;
    return util;
}(SpectrumObservatory.Utils));