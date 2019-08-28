using System;

namespace SerialPort.Net.Windows
{
    //Is flags correct here?
    [Flags]
    public enum StopBits
    {
        None,
        One,
        Two,
        OnePointFive,
    }
}