#pragma once

using namespace System;

namespace Microsoft { namespace Spectrum { namespace Devices { namespace Usrp {

    public ref class SensorValue
    {
        public:
            SensorValue(void){}

            String^ Name;
            String^ Value;
            String^ Unit;

            virtual String^ ToString() override
            {
                return String::Format("Name: {0}, Value: {1}, Unit: {2}", Name, Value, Unit);
            }
    };
}}}}