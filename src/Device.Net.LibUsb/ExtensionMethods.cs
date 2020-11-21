using LibUsbDotNet.Main;
using System;

namespace Device.Net.LibUsb
{
    public static class ExtensionMethods
    {
        public static ConnectedDeviceDefinition ToConnectedDevice(this UsbRegistry usbRegistry)
            => usbRegistry == null ? throw new ArgumentNullException(nameof(usbRegistry)) :
             new ConnectedDeviceDefinition(
                usbRegistry.DevicePath,
                vendorId: (uint)usbRegistry.Vid,
                productId: (uint)usbRegistry.Pid,
               deviceType: DeviceType.Usb);
    }
}
