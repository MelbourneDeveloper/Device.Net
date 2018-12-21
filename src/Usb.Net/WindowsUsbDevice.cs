namespace Device.Net
{
    public class WindowsUsbDevice : WindowsDeviceBase
    {
        #region Public Methods
        public override ushort WriteBufferSize { get; }
        public override ushort ReadBufferSize { get; }   
        #endregion

        #region Constructor
        public WindowsUsbDevice(string deviceId, ushort writeBufferSzie, ushort readBufferSize) : base(deviceId)
        {
            WriteBufferSize = writeBufferSzie;
            ReadBufferSize = readBufferSize;
        }
        #endregion
    }
}
