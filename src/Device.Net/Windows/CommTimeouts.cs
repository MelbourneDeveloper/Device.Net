using System.Runtime.InteropServices;

namespace Device.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CommTimeouts
    {
#pragma warning disable CA1051
        public uint ReadIntervalTimeout;
        public uint ReadTotalTimeoutMultiplier;
        public uint ReadTotalTimeoutConstant;
        public uint WriteTotalTimeoutMultiplier;
        public uint WriteTotalTimeoutConstant;
#pragma warning restore CA1051 
    }
}