namespace Usb.Net.Windows
{
    public class WindowsUsbInterfaceEndpoint : IUsbInterfaceEndpoint
    {
        #region Public Properties
        public byte PipeId { get; }
        public bool IsRead => (PipeId & WinUsbApiCalls.WritePipeId) != 0;
        public bool IsWrite => (PipeId & WinUsbApiCalls.WritePipeId) == 0;

        //Do we need deed to call WinUsb_GetPipePolicy. https://github.com/MelbourneDeveloper/Device.Net/issues/72?

        public ushort MaxPacketSize { get; }

        public bool IsInterrupt { get; }
        #endregion

        #region Constructor
        internal WindowsUsbInterfaceEndpoint(byte pipeId, WinUsbApiCalls.USBD_PIPE_TYPE usbPipeType, ushort maxPacketSize)
        {
            PipeId = pipeId;
            IsInterrupt = usbPipeType == WinUsbApiCalls.USBD_PIPE_TYPE.UsbdPipeTypeInterrupt;
            MaxPacketSize = maxPacketSize;
        }
        #endregion

#pragma warning disable CA1305 
        public override string ToString() => PipeId.ToString();
#pragma warning restore CA1305 
    }
}
