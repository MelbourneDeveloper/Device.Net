using System;

namespace Android.Hardware.Usb
{
    public interface UsbDevice : IDisposable
    {
        int DeviceId { get; }
        string DeviceName { get; }
        int InterfaceCount { get; }
        int ProductId { get; }
        int VendorId { get; }
        UsbInterface GetInterface(int interfaceNumber);
    }
}