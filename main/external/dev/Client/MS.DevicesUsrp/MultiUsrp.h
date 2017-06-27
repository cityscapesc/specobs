#pragma once

#include <uhd/usrp/multi_usrp.hpp>
#include "SensorValue.h"
#include "Range.h"
#include "SubDevSpecPair.h"
#include "TuneRequest.h"
#include "TuneResult.h"
#include "DeviceAddr.h"
#include "StreamArgs.h"
#include "RxStreamer.h"
#include "StreamCmd.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace uhd;
using namespace uhd::usrp;

namespace Microsoft { namespace Spectrum { namespace Devices { namespace Usrp {

    public ref class MultiUsrp : IDisposable
	{
        private:
            // We can't store a native object in managed code due to error C4368: mixed types are not supported
            // multi_usrp::make returns a boost::shared_ptr
            // Therefore, we create a ptr to a shared_ptr in order to make everyone happy
            boost::shared_ptr<multi_usrp>* pUsrp;

            ~MultiUsrp();
            !MultiUsrp();

        public:
            MultiUsrp(DeviceAddr^ args);

            RxStreamer^ get_rx_stream(StreamArgs^ args);
            Dictionary<String^, String^>^ get_usrp_rx_info(size_t channel);

            //void set_master_clock_rate(double rate, size_t mboard);
            //double get_master_clock_rate(size_t mboard);
            String^ get_pp_string();
            String^ get_mboard_name(size_t mboard);
            //time_spec_t get_time_now(size_t mboard);
            //time_spec_t get_time_last_pps(size_t mboard);
            void set_time_now(size_t mboard);
            void set_time_now(const time_spec_t& time_spec, size_t mboard);
            //void set_time_next_pps(const time_spec_t &time_spec, size_t mboard);
            //void set_time_unknown_pps(const time_spec_t &time_spec);
            bool get_time_synchronized(void);
            //void set_command_time(const uhd::time_spec_t &time_spec, size_t mboard);
            //void clear_command_time(size_t mboard);
            void issue_stream_cmd(StreamCmd^ stream_cmd, size_t chan);
            //void set_clock_config(const clock_config_t &clock_config, size_t mboard);
            void set_time_source(String^ source, const size_t mboard);
            String^ get_time_source(const size_t mboard);
            List<String^>^ get_time_sources(const size_t mboard);
            void set_clock_source(String^ source, const size_t mboard);
            String^ get_clock_source(const size_t mboard);
            List<String^>^ get_clock_sources(const size_t mboard);
            size_t get_num_mboards(void);
            SensorValue^ get_mboard_sensor(String^ sensorName, size_t mboard);
            List<String^>^ get_mboard_sensor_names(size_t mboard);
            //void set_user_register(const uint8_t addr, const uint32_t data, size_t mboard);

            void set_rx_subdev_spec(List<SubDevSpecPair^>^ spec);
            void set_rx_subdev_spec(List<SubDevSpecPair^>^ spec, size_t mboard);
            List<SubDevSpecPair^>^ get_rx_subdev_spec(size_t mboard);
            size_t get_rx_num_channels();
            String^ get_rx_subdev_name(size_t chan);
            void set_rx_rate(double rate, size_t chan);
            double get_rx_rate(size_t chan);
            List<Range^>^ get_rx_rates(size_t chan);
            TuneResult^ set_rx_freq(double targetFreq, size_t chan);
            TuneResult^ set_rx_freq(TuneRequest^ request, size_t chan);
            double get_rx_freq(size_t chan);
            List<Range^>^ get_rx_freq_range(size_t chan);
            List<Range^>^ get_fe_rx_freq_range(size_t chan);
            void set_rx_gain(double gain, String^ name, size_t chan);
            void set_rx_gain(double gain, size_t chan);
            double get_rx_gain(String^ name, size_t chan);
            double get_rx_gain(size_t chan);
            List<Range^>^ get_rx_gain_range(String^ name, size_t chan);
            List<Range^>^ get_rx_gain_range(size_t chan);
            List<String^>^ get_rx_gain_names(size_t chan);
            void set_rx_antenna(String^ ant, size_t chan);
            String^ get_rx_antenna(size_t chan);
            List<String^>^ get_rx_antennas(size_t chan);
            void set_rx_bandwidth(double bandwidth, size_t chan);
            double get_rx_bandwidth(size_t chan);
            List<Range^>^ get_rx_bandwidth_range(size_t chan);
            //dboard_iface::sptr get_rx_dboard_iface(size_t chan);
            SensorValue^ get_rx_sensor(String^ name, size_t chan);
            List<String^>^ get_rx_sensor_names(size_t chan);
            //void set_rx_dc_offset(const bool enb, size_t chan);
            //void set_rx_dc_offset(const std::complex<double> &offset, size_t chan);
            //void set_rx_iq_balance(const std::complex<double> &correction, size_t chan);

            // TX
            // We aren't doing any transmissions, so we haven't wrapped those calls

        private:
            TuneResult^ set_rx_freq(tune_request_t tr, size_t chan);
	};
}}}}