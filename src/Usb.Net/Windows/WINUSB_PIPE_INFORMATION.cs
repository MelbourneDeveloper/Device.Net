using System.Runtime.InteropServices;
using static Usb.Net.Windows.WinUsbApiCalls;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace Usb.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WINUSB_PIPE_INFORMATION
    {
        public USBD_PIPE_TYPE PipeType;
        public byte PipeId;
        public ushort MaximumPacketSize;
        public byte Interval;
    }
}

#pragma warning restore CA1815 // Override equals and operator equals on value types
#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore CA1707 // Identifiers should not contain underscores
