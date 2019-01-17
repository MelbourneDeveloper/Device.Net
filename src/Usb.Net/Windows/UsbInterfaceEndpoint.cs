namespace Usb.Net.Windows
{
    public class UsbInterfaceEndpoint
    {
        #region Public Properties
        public byte PipeId { get; }
        public bool IsRead => (PipeId & WinUsbApiCalls.WritePipeId) != 0;
        public bool IsWrite => (PipeId & WinUsbApiCalls.WritePipeId) == 0;
        #endregion

        #region Constructor
        internal UsbInterfaceEndpoint(byte pipeId)
        {
            PipeId = pipeId;
        }
        #endregion
    }
}
