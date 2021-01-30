using System;

namespace SerialPort.Net
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