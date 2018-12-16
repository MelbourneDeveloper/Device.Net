using System;
using System.Runtime.InteropServices;

namespace Hid.Net
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SpDeviceInfoData
    {
        public uint CbSize;
        public Guid ClassGuid;
        public uint DevInst;
        public IntPtr Reserved;
    }
}