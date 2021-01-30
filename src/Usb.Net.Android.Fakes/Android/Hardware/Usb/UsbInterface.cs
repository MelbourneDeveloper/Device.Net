using System;

namespace Android.Hardware.Usb
{
    public interface UsbInterface : IDisposable
    {
        int Id { get; }
        int EndpointCount { get; }
        UsbClass InterfaceClass { get; }
        UsbClass InterfaceSubclass { get; }
        string Name { get; }
        UsbEndpoint GetEndpoint(int endpointNumber);
    }
}
