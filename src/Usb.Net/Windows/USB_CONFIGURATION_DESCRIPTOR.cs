using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Usb.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public struct USB_CONFIGURATION_DESCRIPTOR
#pragma warning restore CA1815 // Override equals and operator equals on value types
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
