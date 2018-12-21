using System;
using System.Runtime.InteropServices;

namespace Device.Net
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SpDeviceInfoData
    {
        public uint CbSize;
        public Guid ClassGuid;
        public uint DevInst;
        public IntPtr Reserved;
    }
}