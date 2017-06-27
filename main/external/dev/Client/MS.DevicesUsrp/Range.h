#pragma once

using namespace System;

namespace Microsoft { namespace Spectrum { namespace Devices { namespace Usrp {

    public ref class Range
    {
        public:
            Range(double start, double stop, double step)
            {
                Start = start;
                Stop = stop;
                Step = step;
            }

            double Start;
            double Stop;
            double Step;

            virtual String^ ToString() override
            {
                return String::Format("Start: {0}, Stop: {1}, Step: {2}", Start, Stop, Step);
            }
    };
}}}}