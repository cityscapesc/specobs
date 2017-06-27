#pragma once

using namespace System;

namespace Microsoft { namespace Spectrum { namespace Devices { namespace Usrp {

    public ref class TuneResult
    {
        public:
            double TargetRfFreqHz;
            double ActualRfFreqHz;
            double TargetDspFreqHz;
            double ActualDspFreqHz;

            virtual String^ ToString() override
            {
                return String::Format("TargetRfFreqHz: {0}, ActualRfFreqHz: {1}, TargetDspFreqHz: {2}, ActualDspFreqHz: {3}", 
                    TargetRfFreqHz, ActualRfFreqHz, TargetDspFreqHz, ActualDspFreqHz);
            }
    };
}}}}