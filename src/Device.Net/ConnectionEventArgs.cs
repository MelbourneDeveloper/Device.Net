using System;

namespace Device.Net
{
    public class ConnectionEventArgs : EventArgs
    {
        public IDevice Device { get; set; }
        public bool IsDisconnection { get; set; }
    }
}
