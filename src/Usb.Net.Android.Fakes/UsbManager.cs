

using System.Collections.Generic;

namespace Android.Hardware.Usb
{
    public interface UsbManager
    {
        IDictionary<string, UsbDevice>? DeviceList { get; }
    }
}
