using System.Runtime.InteropServices;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Usb.Net.Windows
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning restore CA1815 // Override equals and operator equals on value types
#pragma warning disable CA1707 // Identifiers should not contain underscores

    public struct USB_DEVICE_DESCRIPTOR
    {
        public byte bLength;
        public byte bDescriptorType;
        public ushort bcdUSB;
        public byte bDeviceClass;
        public byte bDeviceSubClass;
        public byte bDeviceProtocol;
        public byte bMaxPacketSize0;
        public ushort idVendor;
        public ushort idProduct;
        public ushort bcdDevice;
        public byte iManufacturer;
        public byte iProduct;
        public byte iSerialNumber;
        public byte bNumConfigurations;
    }
}

#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore CA1707 // Identifiers should not contain underscores
