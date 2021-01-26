using System.Runtime.InteropServices;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA1051 

namespace Device.Net.Windows
{
    /// <summary>
    /// Defines the control setting for a serial communications device.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Dcb
    {
        public int DCBlength;
        public uint BaudRate;
        public uint Flags;
        public ushort wReserved;
        public ushort XonLim;
        public ushort XoffLim;
        public byte ByteSize;
        public byte Parity;
        public byte StopBits;
        public sbyte XonChar;
        public sbyte XoffChar;
        public sbyte ErrorChar;
        public sbyte EofChar;
        public sbyte EvtChar;
        public ushort wReserved1;
        public uint fBinary;
        public uint fParity;
        public uint fOutxCtsFlow;
        public uint fOutxDsrFlow;
        public uint fDtrControl;
        public uint fDsrSensitivity;
        public uint fTXContinueOnXoff;
        public uint fOutX;
        public uint fInX;
        public uint fErrorChar;
        public uint fNull;
        public uint fRtsControl;
        public uint fAbortOnError;
    }
}