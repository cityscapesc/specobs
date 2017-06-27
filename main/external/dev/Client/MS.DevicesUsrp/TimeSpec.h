#pragma once

using namespace System;

namespace Microsoft { namespace Spectrum { namespace Devices { namespace Usrp {

    public ref class TimeSpec
    {
        public:

            TimeSpec()
            {
                FullSeconds = 0;
                FractionalSeconds = 0;
            }

            TimeSpec(UInt64 fullSeconds, double fractionalSeconds)
            {
                FullSeconds = fullSeconds;
                FractionalSeconds = fractionalSeconds;
            }

            UInt64 FullSeconds;
            double FractionalSeconds;

            virtual String^ ToString() override
            {
                return String::Format("FullSeconds: {0}, FractionalSeconds: {1}", 
                    FullSeconds, FractionalSeconds);
            }
    };
}}}}