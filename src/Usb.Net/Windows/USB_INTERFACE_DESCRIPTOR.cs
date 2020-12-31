using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning restore CA1815 // Override equals and operator equals on value types
#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace Usb.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct USB_INTERFACE_DESCRIPTOR
    {
        public byte bLength;
        public byte bDescriptorType;
        public byte bInterfaceNumber;
        public byte bAlternateSetting;
        public byte bNumEndpoints;
        public byte bInterfaceClass;
        public byte bInterfaceSubClass;
        public byte bInterfaceProtocol;
        public byte iInterface;
    }
}


#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore CA1707 // Identifiers should not contain underscores
