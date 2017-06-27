#pragma once

#include "TimeSpec.h"

using namespace System;

namespace Microsoft { namespace Spectrum { namespace Devices { namespace Usrp {

    //enum error_code_t {
    //    //! No error associated with this metadata.
    //    ERROR_CODE_NONE         = 0x0,
    //    //! No packet received, implementation timed-out.
    //    ERROR_CODE_TIMEOUT      = 0x1,
    //    //! A stream command was issued in the past.
    //    ERROR_CODE_LATE_COMMAND = 0x2,
    //    //! Expected another stream command.
    //    ERROR_CODE_BROKEN_CHAIN = 0x4,
    //    //! An internal receive buffer has filled.
    //    ERROR_CODE_OVERFLOW     = 0x8,
    //    //! Multi-channel alignment failed.
    //    ERROR_CODE_ALIGNMENT    = 0xc,
    //    //! The packet could not be parsed.
    //    ERROR_CODE_BAD_PACKET   = 0xf
    //} error_code;
    public enum class RxErrorCode
    {
        None = 0,
        Timeout = 1,
        LateCommand = 2,
        BrokenChain = 4,
        Overflow = 8,
        Alignment = 12,
        BadPacket = 15
    };

    public ref class RxMetadata
    {
        public:
            bool HasTimeSpec;
            TimeSpec^ TimeSpec;
            bool MoreFragments;
            size_t FragmentOffset;
            bool StartOfBurst;
            bool EndOfBurst;
            RxErrorCode ErrorCode;

            virtual String^ ToString() override
            {
                return String::Format("HasTimeSpec: {0}, TimeSpec: {1}, MoreFragments: {2}, " + 
                    "FragmentOffset: {3}, StartOfBurst: {4}, EndOfBurst: {5}, ErrorCode: {6}",
                    gcnew cli::array<Object^> {HasTimeSpec, TimeSpec, MoreFragments, FragmentOffset, 
                    StartOfBurst, EndOfBurst, ErrorCode});
            }
    };
}}}}