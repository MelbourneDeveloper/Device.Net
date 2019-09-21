using LibUsbDotNet.Main;

namespace Device.Net.LibUsb
{
    public static class ExtensionMethods
    {
        public static ConnectedDeviceDefinition ToConnectedDevice(this UsbRegistry usbRegistry)
        {
            return new ConnectedDeviceDefinition(usbRegistry.DevicePath)
            {
                VendorId = (uint)usbRegistry.Vid,
                ProductId = (uint)usbRegistry.Pid,
                DeviceType = DeviceType.Usb
            };
        }
    }
}
