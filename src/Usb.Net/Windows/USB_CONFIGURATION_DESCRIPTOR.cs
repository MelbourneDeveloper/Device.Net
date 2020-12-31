// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
using System.Runtime.InteropServices;

#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace Usb.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct USB_CONFIGURATION_DESCRIPTOR
    {
        public byte bLength;
        public byte bDescriptorType;
        public ushort wTotalLength;
        public byte bNumInterfaces;
        public byte bConfigurationValue;
        public byte iConfiguration;
        public byte bmAttributes;
        public byte MaxPower;
    }
}
#pragma warning restore CA1815 // Override equals and operator equals on value types
#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore CA1707 // Identifiers should not contain underscores
