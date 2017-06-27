#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

namespace Microsoft { namespace Spectrum { namespace Devices { namespace Usrp {

    public enum class TuneRequestPolicy
    {
        None = 'N',
        Auto = 'A',
        Manual = 'M'
    };

    public ref class TuneRequest
    {
        public:
            TuneRequest(double targetFreqHz)
            {
                TargetFreqHz = targetFreqHz;
                RfFreqPolicy = TuneRequestPolicy::Auto;
                RfFreqHz = 0;
                DspFreqPolicy = TuneRequestPolicy::Auto;
                DspFreqHz = 0;
            }

            TuneRequest(double targetFreqHz, double loOffset)
            {
                TargetFreqHz = targetFreqHz;
                RfFreqPolicy = TuneRequestPolicy::Manual;
                RfFreqHz = TargetFreqHz + loOffset;
                DspFreqPolicy = TuneRequestPolicy::Auto;
                DspFreqHz = 0;
            }

            double TargetFreqHz;
            TuneRequestPolicy RfFreqPolicy;
            double RfFreqHz;
            TuneRequestPolicy DspFreqPolicy;
            double DspFreqHz;

            virtual String^ ToString() override
            {
                return String::Format("TargetRfFreqHz: {0}, RfFreqPolicy: {1}, RfFreqHz: {2}, DspFreqPolicy: {3}, DspFreqHz: {4}", 
                    gcnew cli::array<Object^> {TargetFreqHz, RfFreqPolicy, RfFreqHz, DspFreqPolicy, DspFreqHz});
            }
    };
}}}}