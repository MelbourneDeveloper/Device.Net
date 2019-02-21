using System.Runtime.InteropServices;

namespace Usb.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct USB_INTERFACE_DESCRIPTOR
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public byte bLength;
        public byte bDescriptorType;
        public byte bInterfaceNumber;
        public byte bAlternateSetting;
        public byte bNumEndpoints;
        public byte bInterfaceClass;
        public byte bInterfaceSubClass;
        public byte bInterfaceProtocol;
        public byte iInterface;
#pragma warning restore CA1051 // Do not declare visible instance fields
    }
}
