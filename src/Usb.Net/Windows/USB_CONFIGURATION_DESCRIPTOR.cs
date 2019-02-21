using System.Runtime.InteropServices;

namespace Usb.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct USB_CONFIGURATION_DESCRIPTOR
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public byte bLength;
        public byte bDescriptorType;
        public ushort wTotalLength;
        public byte bNumInterfaces;
        public byte bConfigurationValue;
        public byte iConfiguration;
        public byte bmAttributes;
        public byte MaxPower;
#pragma warning restore CA1051 // Do not declare visible instance fields
    }
}
