SpectrumObservatory.Utils = (function (util) {
    "use strict";

    util = util || {};

    util.ApplicationStartupEventHandlers = function () {
        var navigationBarLinks = {
            Home: "/Home/Index",
            Profile: "/Profile/Index",
            StationRegistration: "/SpectrumObservatoryRegistration/BeginRegistration",
            FAQ: "/Common/Index",
            Feedback: "/Common/FeedbackView",
            Contribution: "/Common/Contribute",
            Manage: "/Manage/Index",
            StationSearch: "/Home/Index?stationSearch"
        }

        $("#Radio7").click(function () {
            window.location.href = navigationBarLinks.Profile;
        });
        $("#Radio5").click(function () {
            window.location.href = navigationBarLinks.FAQ;
        });
        $("#Radio6").click(function () {
            window.location.href = navigationBarLinks.Feedback;
        });
        $("#Radio1").click(function () {
            window.location.href = navigationBarLinks.Home;
        });
        $("#Radio4").click(function () {
            window.location.href = navigationBarLinks.Manage;
        });
        $("#Radio8").click(function () {
            window.location.href = navigationBarLinks.Contribution;
        });
        $("#Radio20").click(function () {
            window.location.href = navigationBarLinks.StationSearch;
        });
        $("#Radio3").click(function () {
            SpectrumObservatory.Utils.ajaxCall.doGet(navigationBarLinks.StationRegistration, null, true)
            .then(function (responseHtml) {
                $('#left-pane .container').empty();
                $('#left-pane .container').html(responseHtml);
            });
        });
    };

    var spectrumObservatoryUtils = (function (util) {

        var wizardStepIndicator = (function () {
            var completionStatus = {
                Completed: "completed",
                Pending: "pending"
            };

            var steps = [];
            var stepIndicatorContainerClass = "jQueryWizardStepIndicators";
            var stepStatusAttribute = "status";
            var currentStepIndex = 0;

            var EventArgs = function () {

            };

            var initialize = function (stepIndicatorDomSelector) {
                $(stepIndicatorDomSelector).addClass(stepIndicatorContainerClass);
                var stepIndicators = $('.' + stepIndicatorContainerClass).children('li');

                stepIndicators.each(function (index) {
                    $(this).attr(stepStatusAttribute, completionStatus.Pending);

                    steps.push($(this));
                })
            }

            var markStepAsCompleted = function (stepIndex, context) {
                if (stepIndex >= 0 && stepIndex < steps.length) {
                    steps[stepIndex].attr(stepStatusAttribute, completionStatus.Completed);

                    if (context && typeof (context.onStepCompleted) === "function") {
                        var eventArgs = new EventArgs();
                        eventArgs.Step = steps[stepIndex];

                        context.onStepCompleted(eventArgs);
                    }
                }
            }

            var isCompleted = function (stepIndex) {
                return steps[stepIndex].attr(stepStatusAttribute) === completionStatus.Completed;
            }

            return {
                initialize: initialize,
                markStepAsCompleted: function (stepIndex) { markStepAsCompleted(stepIndex, this); },
                isStepCompleted: isCompleted,
                onStepCompleted: null,
            };
        }());

        // TODO: We should be able to have multiple instance of this wizard in a same page.
        util.wizard = (function () {

            // default settings
            var defaultSettings = {
                wizardDomSelector: null,
                stepNames: [],
                stepNameAsTransitionButtonName: false,
                stepIndicatorDomSelector: null
            };

            var stepIndicator;

            var EventArgs = function () {
            };

            var parentDomSelectorClass = 'jQueryWizard';
            var wizardStepSelectorClass = 'step';

            var wizardSteps = [];
            var stepIndex = 0;
            var maxStepIndexReached = 0;

            var initialize = function (settings) {
                $.extend(defaultSettings, settings);

                if (defaultSettings.stepIndicatorDomSelector) {
                    stepIndicator = wizardStepIndicator;
                    stepIndicator.initialize(defaultSettings.stepIndicatorDomSelector);
                }

                $(defaultSettings.wizardDomSelector).addClass(parentDomSelectorClass);
                var steps = $('.' + parentDomSelectorClass).children('div');

                steps.addClass(wizardStepSelectorClass);

                steps.each(function (index) {
                    if (index == 0) {
                        $(this).show();
                    }
                    else {
                        $(this).hide();
                    }

                    wizardSteps.push($(this));
                });
            }

            var updateView = function (context) {

                var eventArg = new EventArgs();

                eventArg.prevStepIndex = -1;
                eventArg.prevStepName = null;
                eventArg.nextStepIndex = -1;
                eventArg.nextStepName = null;

                var prevIndex = (stepIndex - 1);
                var nextIndex = (stepIndex + 1);

                if (prevIndex >= 0) {
                    eventArg.prevStepIndex = prevIndex;

                    if (defaultSettings.stepNameAsTransitionButtonName) {
                        eventArg.prevStepName = defaultSettings.stepNames[prevIndex];
                    }
                }

                if (nextIndex <= wizardSteps.length) {
                    eventArg.nextStepIndex = nextIndex;

                    if (defaultSettings.stepNameAsTransitionButtonName) {
                        eventArg.nextStepName = defaultSettings.stepNames[nextIndex];
                    }
                }

                if (context.onViewUpdating && typeof (context.onViewUpdating) === "function") {
                    context.onViewUpdating(eventArg);
                }

                jQuery.each(wizardSteps, function (index, value) {
                    if (stepIndex == index) {
                        wizardSteps[stepIndex].show();
                    }
                    else {
                        wizardSteps[index].hide();
                    }
                });

                if (context.onViewUpdated && typeof (context.onViewUpdated) === "function") {
                    context.onViewUpdated(eventArg);
                }
            }

            var hasNext = function () {
                return (stepIndex + 1) < wizardSteps.length;
            }

            var next = function (context) {
                if (hasNext()) {
                    stepIndex++;
                    updateView(context);

                    var prevStepIndex = stepIndex - 1;

                    if (stepIndicator && !stepIndicator.isStepCompleted(prevStepIndex)) {

                        if (context.onStepCompleted && typeof (context.onStepCompleted) === "function") {
                            stepIndicator.onStepCompleted = context.onStepCompleted;
                        }

                        stepIndicator.markStepAsCompleted(prevStepIndex);
                    }
                }
            }

            var prev = function (context) {
                if (stepIndex > 0) {
                    stepIndex--;
                    updateView(context);
                }
            }

            var jumpToIndex = function (index, context) {
                if (index < 0 || index > wizardSteps.length) {
                    console.log("Index out of range");
                    return;
                }

                // Do not allow to make a transition to a new step by skipping any of the previous step.
                if (stepIndicator && stepIndicator.isStepCompleted(index)) {
                    if (maxStepIndexReached <= index) {
                        stepIndex = index;
                        updateView(context);
                    }
                }
            }

            var addNewStep = function (newStepHtml) {
                if (newStepHtml) {
                    // Appending new step Html as a last child of wizard container.
                    $("." + parentDomSelectorClass).append($(newStepHtml));

                    var newStep = $("." + parentDomSelectorClass).children("div").last();

                    if (newStep.hasClass(wizardStepSelectorClass)) {
                        newStep.addClass(wizardStepSelectorClass);
                    }

                    wizardSteps.push(newStep);
                }
            };

            var reset = function (context) {
                stepIndex = 0;
            }

            var wizardComplete = function (context) {
                wizardStepIndicator.markStepAsCompleted(stepIndex);
                reset();
            }

            return {
                initializWizard: function (settings) { initialize(settings, this); },
                next: function () { next(this); },
                prev: function () { prev(this); },
                hasNext: hasNext,
                jumpToStep: function (stepIndex) { jumpToIndex(stepIndex, this); },
                addNewStep: addNewStep,
                onViewUpdating: null,
                onViewUpdated: null,
                OnStepCompleted: null,
                wizardComplete: function () { wizardComplete(this); },
                reset: function () { reset(this); }
            };
        })();

        util.preloader = (function () {

            var show = function () {
                $('#preloader').removeClass('hidden');
            }

            var hide = function () {
                $('#preloader').removeClass('hidden').addClass('hidden');
            }

            return {
                show: show,
                hide: hide
            };
        }());
    })(util);

    var commonUtils = (function (util) {

        var ajaxCall = (function () {

            function doAjax(type, contentType, url, data, sppressDefaultErrorHandler) {
                var deferred = $.Deferred();

                $.ajax({
                    url: url,
                    data: data,
                    type: type,
                    contentType: contentType,
                    beforeSend: function () {
                        SpectrumObservatory.Utils.preloader.show();
                    },
                    success: deferred.resolve,
                    error: [function (jqXHR, textStatus, errorThrown) {
                        if (!sppressDefaultErrorHandler) {
                            if (jqXHR.status == "403") { // Authentication or Autherization failure
                                window.location = "/?ReturnUrl=" + window.location.pathname;
                                return;
                            }

                            // Log technical details and show friendly message to the user.
                            console.error('errorThrown: ' + errorThrown);
                            console.error('textStatus:' + textStatus);
                        }
                    },
                    deferred.reject
                    ],
                    complete: [function (jqXHR, textStatus) {
                        SpectrumObservatory.Utils.preloader.hide();
                    },
                    deferred.always
                    ]
                });

                return deferred.promise();
            }

            return {

                doGet: function (url, data, sppressDefaultErrorHandler) {
                    return doAjax('GET', 'application/x-www-form-urlencoded; charset=UTF-8', url, data, sppressDefaultErrorHandler);
                },

                doPost: function (url, data, sppressDefaultErrorHandler) {
                    return doAjax('POST', 'application/x-www-form-urlencoded; charset=UTF-8', url, data, sppressDefaultErrorHandler);
                },

                doJsonPost: function (url, data, sppressDefaultErrorHandler) {
                    return doAjax('POST', 'application/json; charset=utf-8', url, data, sppressDefaultErrorHandler);
                }
            };

        }());

        var slimScroll = (function () {

            var enableSlimScroll = function (element, options) {

                var settings = {
                    height: $(window).height(),
                    alwaysVisible: false,
                    disableFadeOut: true,
                    railVisible: true,
                };

                $.extend(settings, options);

                $(element).slimScroll(settings);
            };

            var resizeSlimScroll = function (element) {
                var me = $(element);
                me.css("height", $(window).height() + 'px');
                $(".slimScrollDiv").css("height", $(window).height() + 'px');
                var height = Math.max((me.outerHeight() / me[0].scrollHeight) * me.outerHeight(), 30);
                $(".slimScrollBar").css({ height: height + 'px' });
            };

            return {
                enableSlimScroll: enableSlimScroll,
                resizeSlimScroll: resizeSlimScroll
            };

        }());

        var defaultScroll = (function () {

            var enableDefaultScroll = function () {
                $('body').removeClass('noScroll');
            }

            var disableDefaultScroll = function () {
                $('body').removeClass('noScroll').addClass('noScroll');
            }

            return {
                enableDefaultScroll: enableDefaultScroll,
                disableDefaultScroll: disableDefaultScroll
            };

        }());

        return {
            ajaxCall: ajaxCall,
            slimScroll: slimScroll,
            defaultScroll: defaultScroll
        };

    }(util));

    $.extend(util, commonUtils)

    return util;
}(SpectrumObservatory.Utils));

$(document).ready(function () {
    SpectrumObservatory.Utils.ApplicationStartupEventHandlers();
});


