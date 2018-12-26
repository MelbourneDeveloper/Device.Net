using System;

namespace Device.Net
{
    public class DeviceBase
    {
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public ITracer Tracer { get; set; }

        protected void RaiseConnected()
        {
            Connected?.Invoke(this, new EventArgs());
        }

        protected void RaiseDisconnected()
        {
            Disconnected?.Invoke(this, new EventArgs());
        }
    }
}
