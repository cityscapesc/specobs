using namespace System;
using namespace System::Collections::Generic;

namespace Microsoft { namespace Spectrum { namespace Devices { namespace Usrp {

    [Serializable]
    public ref class DeviceAddr : Dictionary<String^, String^>
    {
    };
}}}}