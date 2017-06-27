#pragma once

using namespace System;
using namespace System::Collections::Generic;

namespace Microsoft { namespace Spectrum { namespace Devices { namespace Usrp {

    public ref class StreamArgs
    {
        public:
            StreamArgs(String^ cpuFormat, String^ otwFormat)
            {
                CpuFormat = cpuFormat;
                OtwFormat = otwFormat;
            }

            String^ CpuFormat;
            String^ OtwFormat;
            DeviceAddr^ Args;
            List<size_t>^ Channels;

            virtual String^ ToString() override
            {
                return String::Format("CPU format: {0}, OTW format: {1}", CpuFormat, OtwFormat);
            }
    };
}}}}