namespace Device.Net
{
    public class ConnectionEvent
    {
        public IDevice Device { get; set; }
        public bool IsDisconnection { get; set; }
    }
}
