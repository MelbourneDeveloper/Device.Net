using System;
using System.Runtime.InteropServices;

namespace Device.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
#pragma warning disable CA1051 
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public bool bInheritHandle;
#pragma warning restore CA1051
    }
}
