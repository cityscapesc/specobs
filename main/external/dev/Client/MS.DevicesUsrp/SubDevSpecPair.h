#pragma once

using namespace System;

namespace Microsoft { namespace Spectrum { namespace Devices { namespace Usrp {

    public ref class SubDevSpecPair
    {
        public:
            SubDevSpecPair(String^ dbName, String^ sdName)
            {
                DaughterboardName = dbName;
                SubDeviceName = sdName;
            }

            property String^ DaughterboardName;
            property String^ SubDeviceName;
    };
}}}}