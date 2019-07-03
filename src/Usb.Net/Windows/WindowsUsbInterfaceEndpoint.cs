namespace Usb.Net.Windows
{
    public class WindowsUsbInterfaceEndpoint : IUsbInterfaceEndpoint
    {
        #region Public Properties
        public byte PipeId { get; }
        public bool IsRead => (PipeId & WinUsbApiCalls.WritePipeId) != 0;
        public bool IsWrite => (PipeId & WinUsbApiCalls.WritePipeId) == 0;
        #endregion

        #region Constructor
        internal WindowsUsbInterfaceEndpoint(byte pipeId)
        {
            PipeId = pipeId;
        }
        #endregion

#pragma warning disable CA1305 // Specify IFormatProvider
        public override string ToString() => PipeId.ToString();
#pragma warning restore CA1305 // Specify IFormatProvider
    }
}
