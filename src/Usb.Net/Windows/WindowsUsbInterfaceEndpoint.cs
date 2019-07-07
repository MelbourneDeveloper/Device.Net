namespace Usb.Net.Windows
{
    public class WindowsUsbInterfaceEndpoint : IUsbInterfaceEndpoint
    {
        #region Public Properties
        public byte PipeId { get; }
        public bool IsRead => (PipeId & WinUsbApiCalls.WritePipeId) != 0;
        public bool IsWrite => (PipeId & WinUsbApiCalls.WritePipeId) == 0;
        public ushort ReadBufferSize { get; }
        public ushort WriteBufferSize { get; }
        #endregion

        #region Constructor
        internal WindowsUsbInterfaceEndpoint(byte pipeId, ushort readBufferSize, ushort writeBufferSize)
        {
            PipeId = pipeId;
            ReadBufferSize = readBufferSize;
            WriteBufferSize = writeBufferSize;
        }
        #endregion

#pragma warning disable CA1305 
        public override string ToString() => PipeId.ToString();
#pragma warning restore CA1305 
    }
}
