#pragma once

#include "RxMetadata.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Collections::Generic;

using namespace uhd;

namespace Microsoft { namespace Spectrum { namespace Devices { namespace Usrp {

    public ref class RxStreamer : IDisposable
    {
        public:
            // Made generic so that we can quickly experiment with the different CpuFormat options: sc8, sc16, fc32, fc64

			// TODO: why is complexwidth being passed in and not being used at all?

            generic <typename T>
            size_t Receive(cli::array<T>^ buff, size_t samplesPerBuffer, Int32 complexWidth, [Out] RxMetadata^% md, double timeout, bool onePacket)
            {
                // It's easiest just to pin a single array and not have to keep track of multiple pin_ptrs, so ...
                // Buff is a single [] that we will partition up into channelCount sub arrays, each of which is sizeOfT * samplesPerBuff * complexWidth in bytes.
                // We use byte* rather than the specific type T for 2 reasons: 1) UHD doesn't care about the type as we have already communicated the size in the stream args
                // 2) you can't use a managed type for np, so we have to pick something that works in both native and managed.
                pin_ptr<T> mp = &buff[0];
                byte* np = reinterpret_cast<byte*>(mp);

                int sizeOfT = sizeof(T);
                std::vector<byte*> nBuffs;
                nBuffs.push_back(np);

                rx_metadata_t nmd;
                size_t sampleCount = (*pStreamer)->recv(nBuffs, samplesPerBuffer, nmd, timeout, onePacket);

                md = gcnew RxMetadata();
                md->EndOfBurst = nmd.end_of_burst;
                md->ErrorCode = static_cast<RxErrorCode>(nmd.error_code);
                md->FragmentOffset = nmd.fragment_offset;
                md->HasTimeSpec = nmd.has_time_spec;
                md->MoreFragments = nmd.more_fragments;
                md->StartOfBurst = nmd.start_of_burst;
                md->TimeSpec = gcnew TimeSpec(nmd.time_spec.get_full_secs(), nmd.time_spec.get_frac_secs());

                return sampleCount;
            }

        internal:
            // We can't store a native object in managed code due to error C4368: mixed types are not supported
            // multi_usrp::get_rx_stream returns a boost::shared_ptr
            // Therefore, we create a ptr to a shared_ptr in order to make everyone happy
            boost::shared_ptr<rx_streamer>* pStreamer;

            RxStreamer(boost::shared_ptr<rx_streamer> pStreamer)
            {
                this->pStreamer = new  boost::shared_ptr<rx_streamer>();
                pStreamer.swap(*(this->pStreamer));
            }

            ~RxStreamer() { this->!RxStreamer(); }
            !RxStreamer() { delete pStreamer; }
    };
}}}}