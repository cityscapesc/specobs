// This is the main DLL file.

#include "stdafx.h"
#include <msclr\marshal_cppstd.h>
#include <uhd\utils\msg.hpp>
#include <string>
#include <iostream>
#include "MultiUsrp.h"

using namespace Microsoft::Spectrum::Devices::Usrp;
using namespace msclr::interop;
using namespace std;

static void msg_handler(uhd::msg::type_t type, const std::string& msg)
{
	if (type == uhd::msg::type_t::error)
	{
		std::cout << msg;
	}
}

MultiUsrp::MultiUsrp(DeviceAddr^ args)
{
    device_addr_t devices;

    for each (KeyValuePair<String^, String^> kvp in args)
    {
        devices[marshal_as<string>(kvp.Key)] = marshal_as<string>(kvp.Value);
    }

    pUsrp = new boost::shared_ptr<multi_usrp>();
    multi_usrp::make(devices).swap(*pUsrp);

	uhd::msg::register_handler(&msg_handler);
}

MultiUsrp::~MultiUsrp()
{
    this->!MultiUsrp();
}

MultiUsrp::!MultiUsrp()
{
    delete pUsrp;
}

RxStreamer^ MultiUsrp::get_rx_stream(StreamArgs^ mArgs)
{
    String^ cpuFormat = mArgs->CpuFormat; // The easiest work-around to get it to compile
    String^ otwFormat = mArgs->OtwFormat; // The easiest work-around to get it to compile

    stream_args_t nArgs(marshal_as<string>(cpuFormat), marshal_as<string>(otwFormat));

    for each (String^ key in mArgs->Args->Keys)
    {
        nArgs.args.set(marshal_as<string>(key), marshal_as<string>(mArgs->Args[key]));
    }
    
    return gcnew RxStreamer((*pUsrp)->get_rx_stream(nArgs));
}

Dictionary<String^, String^>^ MultiUsrp::get_usrp_rx_info(size_t chan)
{
    dict<string, string> info = (*pUsrp)->get_usrp_rx_info(chan);

    Dictionary<String^, String^>^ ret = gcnew Dictionary<String^, String^>();

    for each (string key in info.keys())
    {
        ret->Add(marshal_as<String^>(key), marshal_as<String^>(info[key]));
    }

    return ret;
}

String^ MultiUsrp::get_pp_string()
{
    return marshal_as<String^>((*pUsrp)->get_pp_string());
}

size_t MultiUsrp::get_num_mboards(void)
{
    return (*pUsrp)->get_num_mboards();
}

String^ MultiUsrp::get_mboard_name(size_t mboard)
{
    return marshal_as<String^>((*pUsrp)->get_mboard_name(mboard));
}

void MultiUsrp::set_time_now(size_t mboard)
{
    //set_time_now(time_spec_t(), mboard);
    set_time_now(time_spec_t::get_system_time(), mboard);
}

void MultiUsrp::set_time_now(const time_spec_t& time_spec, size_t mboard)
{
    (*pUsrp)->set_time_now(time_spec, mboard);
}

bool MultiUsrp::get_time_synchronized()
{
    return (*pUsrp)->get_time_synchronized();
}

void MultiUsrp::issue_stream_cmd(StreamCmd^ mCmd, size_t chan)
{
    stream_cmd_t nCmd(static_cast<stream_cmd_t::stream_mode_t>(mCmd->Mode));
    nCmd.num_samps = mCmd->NumSamps;
    nCmd.stream_now = (bool)mCmd->StreamNow;
    nCmd.time_spec = time_spec_t(mCmd->TimeSpec->FullSeconds, mCmd->TimeSpec->FractionalSeconds);

    (*pUsrp)->issue_stream_cmd(nCmd, chan);
}

void MultiUsrp::set_time_source(String^ source, const size_t mboard)
{
    (*pUsrp)->set_time_source(marshal_as<string>(source), mboard);
}

String^ MultiUsrp::get_time_source(size_t mboard)
{
    return marshal_as<String^>((*pUsrp)->get_time_source(mboard));
}

List<String^>^ MultiUsrp::get_time_sources(size_t mboard)
{
    vector<string> sources = (*pUsrp)->get_time_sources(mboard);

    List<String^>^ ret = gcnew List<String^>();

    for each (string source in sources)
    {
        ret->Add(marshal_as<String^>(source));
    }

    return ret;
}

void MultiUsrp::set_clock_source(String^ source, const size_t mboard)
{
    (*pUsrp)->set_clock_source(marshal_as<string>(source), mboard);
}

String^ MultiUsrp::get_clock_source(size_t mboard)
{
    return marshal_as<String^>((*pUsrp)->get_clock_source(mboard));
}

List<String^>^ MultiUsrp::get_clock_sources(size_t mboard)
{
    vector<string> sources = (*pUsrp)->get_clock_sources(mboard);

    List<String^>^ ret = gcnew List<String^>();

    for each (string source in sources)
    {
        ret->Add(marshal_as<String^>(source));
    }

    return ret;
}

SensorValue^ MultiUsrp::get_mboard_sensor(String^ name, size_t mboard)
{
    sensor_value_t value = (*pUsrp)->get_mboard_sensor(marshal_as<string>(name), mboard);

    SensorValue^ ret = gcnew SensorValue();

    ret->Name = marshal_as<String^>(value.name);
    ret->Unit = marshal_as<String^>(value.unit);
    ret->Value = marshal_as<String^>(value.value);

    return ret;
}

List<String^>^ MultiUsrp::get_mboard_sensor_names(size_t mboard)
{
    vector<string> names = (*pUsrp)->get_mboard_sensor_names(mboard);

    List<String^>^ ret = gcnew List<String^>();

    for each (string name in names)
    {
        ret->Add(marshal_as<String^>(name));
    }

    return ret;
}

void MultiUsrp::set_rx_subdev_spec(List<SubDevSpecPair^>^ mSpec)
{
    set_rx_subdev_spec(mSpec, multi_usrp::ALL_MBOARDS);
}

void MultiUsrp::set_rx_subdev_spec(List<SubDevSpecPair^>^ mSpec, size_t mboard)
{
    subdev_spec_t nSpec;

    for each (SubDevSpecPair^ pair in mSpec)
    {
        string dbname = marshal_as<string>(pair->DaughterboardName);
        string sdname = marshal_as<string>(pair->SubDeviceName);

        nSpec.push_back(subdev_spec_pair_t(dbname, sdname));
    }

    (*pUsrp)->set_rx_subdev_spec(nSpec, mboard);
}

List<SubDevSpecPair^>^ MultiUsrp::get_rx_subdev_spec(size_t mboard)
{
    subdev_spec_t nSpec = (*pUsrp)->get_rx_subdev_spec(mboard);

    List<SubDevSpecPair^>^ ret = gcnew List<SubDevSpecPair^>();

    for each (subdev_spec_pair_t pair in nSpec)
    {
        String^ dbName = marshal_as<String^>(pair.db_name);
        String^ sdName = marshal_as<String^>(pair.sd_name);

        ret->Add(gcnew SubDevSpecPair(dbName, sdName));
    }

    return ret;
}

size_t MultiUsrp::get_rx_num_channels()
{
    return (*pUsrp)->get_rx_num_channels();
}

String^ MultiUsrp::get_rx_subdev_name(size_t chan)
{
    return marshal_as<String^>((*pUsrp)->get_rx_subdev_name(chan));
}

void MultiUsrp::set_rx_rate(double rate, size_t chan)
{
    return (*pUsrp)->set_rx_rate(rate, chan);
}

double MultiUsrp::get_rx_rate(size_t chan)
{
    return (*pUsrp)->get_rx_rate(chan);
}

List<Range^>^ MultiUsrp::get_rx_rates(size_t chan)
{
    meta_range_t rates = (*pUsrp)->get_rx_rates(chan);

    List<Range^>^ ret = gcnew List<Range^>();

    for each (range_t range in rates)
    {
        ret->Add(gcnew Range(range.start(), range.stop(), range.step()));
    }

    return ret;
}

TuneResult^ MultiUsrp::set_rx_freq(double targetFreq, size_t chan)
{
    return set_rx_freq(tune_request_t(targetFreq), chan);
}

TuneResult^ MultiUsrp::set_rx_freq(TuneRequest^ mReq, size_t chan)
{
    tune_request_t nReq;
    nReq.dsp_freq = mReq->DspFreqHz;
    nReq.dsp_freq_policy = static_cast<tune_request_t::policy_t>(mReq->DspFreqPolicy);
    nReq.rf_freq = mReq->RfFreqHz;
    nReq.rf_freq_policy = static_cast<tune_request_t::policy_t>(mReq->RfFreqPolicy);

    return set_rx_freq(nReq, chan);
}

TuneResult^ MultiUsrp::set_rx_freq(tune_request_t tr, size_t chan)
{
    tune_result_t nResult = (*pUsrp)->set_rx_freq(tr, chan);
    
    TuneResult^ mResult = gcnew TuneResult();
    mResult->ActualDspFreqHz = nResult.actual_dsp_freq;
    mResult->ActualRfFreqHz = nResult.actual_rf_freq;
    mResult->TargetDspFreqHz = nResult.target_dsp_freq;
    mResult->TargetRfFreqHz = nResult.target_rf_freq;

    return mResult;
}

double MultiUsrp::get_rx_freq(size_t chan)
{
    return (*pUsrp)->get_rx_freq(chan);
}

List<Range^>^ MultiUsrp::get_rx_freq_range(size_t chan)
{
    meta_range_t ranges = (*pUsrp)->get_rx_freq_range(chan);

    List<Range^>^ ret = gcnew List<Range^>();

    for each (range_t range in ranges)
    {
        ret->Add(gcnew Range(range.start(), range.stop(), range.step()));
    }

    return ret;
}

List<Range^>^ MultiUsrp::get_fe_rx_freq_range(size_t chan)
{
    meta_range_t ranges = (*pUsrp)->get_fe_rx_freq_range(chan);

    List<Range^>^ ret = gcnew List<Range^>();

    for each (range_t range in ranges)
    {
        ret->Add(gcnew Range(range.start(), range.stop(), range.step()));
    }

    return ret;
}

void MultiUsrp::set_rx_gain(double gain, String^ name, size_t chan)
{
    (*pUsrp)->set_rx_gain(gain, marshal_as<string>(name), chan);
}

void MultiUsrp::set_rx_gain(double gain, size_t chan)
{
    (*pUsrp)->set_rx_gain(gain, chan);
}

double MultiUsrp::get_rx_gain(String^ name, size_t chan)
{
    return (*pUsrp)->get_rx_gain(marshal_as<string>(name), chan);
}

double MultiUsrp::get_rx_gain(size_t chan)
{
    return (*pUsrp)->get_rx_gain(multi_usrp::ALL_GAINS, chan);
}

List<Range^>^ MultiUsrp::get_rx_gain_range(String^ name, size_t chan)
{
    meta_range_t gains = (*pUsrp)->get_rx_gain_range(chan);

    List<Range^>^ ret = gcnew List<Range^>();

    for each (range_t range in gains)
    {
        ret->Add(gcnew Range(range.start(), range.stop(), range.step()));
    }

    return ret;
}

List<Range^>^ MultiUsrp::get_rx_gain_range(size_t chan)
{
    return get_rx_gain_range(marshal_as<String^>(multi_usrp::ALL_GAINS), chan);
}

List<String^>^ MultiUsrp::get_rx_gain_names(size_t chan)
{
    vector<string> names = (*pUsrp)->get_rx_gain_names(chan);

    List<String^>^ ret = gcnew List<String^>();

    for each (string name in names)
    {
        ret->Add(marshal_as<String^>(name));
    }

    return ret;
}

void MultiUsrp::set_rx_antenna(String^ ant, size_t chan)
{
    (*pUsrp)->set_rx_antenna(marshal_as<string>(ant), chan);
}

String^ MultiUsrp::get_rx_antenna(size_t chan)
{
    return marshal_as<String^>((*pUsrp)->get_rx_antenna(chan));
}

List<String^>^ MultiUsrp::get_rx_antennas(size_t chan)
{
    vector<string> names = (*pUsrp)->get_rx_antennas(chan);

    List<String^>^ ret = gcnew List<String^>();

    for each (string name in names)
    {
        ret->Add(marshal_as<String^>(name));
    }

    return ret;
}

void MultiUsrp::set_rx_bandwidth(double bandwidth, size_t chan)
{
    (*pUsrp)->set_rx_bandwidth(bandwidth, chan);
}

double MultiUsrp::get_rx_bandwidth(size_t chan)
{
    return (*pUsrp)->get_rx_bandwidth(chan);
}

List<Range^>^ MultiUsrp::get_rx_bandwidth_range(size_t chan)
{
    meta_range_t bandwidths = (*pUsrp)->get_rx_bandwidth_range(chan);

    List<Range^>^ ret = gcnew List<Range^>();

    for each (range_t range in bandwidths)
    {
        ret->Add(gcnew Range(range.start(), range.stop(), range.step()));
    }

    return ret;
}

SensorValue^ MultiUsrp::get_rx_sensor(String^ name, size_t chan)
{
    sensor_value_t value = (*pUsrp)->get_rx_sensor(marshal_as<string>(name), chan);

    SensorValue^ ret = gcnew SensorValue();

    ret->Name = marshal_as<String^>(value.name);
    ret->Unit = marshal_as<String^>(value.unit);
    ret->Value = marshal_as<String^>(value.value);

    return ret;
}

List<String^>^ MultiUsrp::get_rx_sensor_names(size_t chan)
{
    vector<string> names = (*pUsrp)->get_rx_sensor_names(chan);

    List<String^>^ ret = gcnew List<String^>();

    for each (string name in names)
    {
        ret->Add(marshal_as<String^>(name));
    }

    return ret;
}