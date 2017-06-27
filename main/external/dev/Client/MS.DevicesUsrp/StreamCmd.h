#pragma once

#include "TimeSpec.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace Microsoft { namespace Spectrum { namespace Devices { namespace Usrp {

    public enum class StreamMode
    {
        StartContinuous = 'a',
        StopContinuous = 'o',
        NumSampsAndDone = 'd',
        NumSampsAndMore = 'm'
    };

    public ref class StreamCmd
    {
        public:
            StreamCmd(StreamMode mode)
            {
                Mode = mode;
            }

            StreamMode Mode;
            size_t NumSamps;
            bool StreamNow;
            TimeSpec^ TimeSpec;
    };
}}}}