using Windows.Devices.Usb;

namespace Usb.Net.UWP
{
    public class UWPUsbInterfaceEndpoint : IUsbInterfaceEndpoint
    {
        #region Public Properties
        public UsbInterruptOutPipe InPipe { get; }
        public UsbInterruptInPipe OutPipe { get; }
        public bool IsRead => InPipe != null;
        public bool IsWrite => OutPipe != null;
        //TODO: They don't think it be like it is, but it do
        public bool IsInterrupt => true;
#pragma warning disable CA1065 
        //Which one?
        public byte PipeId => throw new System.NotImplementedException();
#pragma warning restore CA1065
        public ushort ReadBufferSize => (ushort)InPipe.EndpointDescriptor.MaxPacketSize;
        public ushort WriteBufferSize => (ushort)OutPipe.EndpointDescriptor.MaxPacketSize;
        #endregion

        #region Constructor
        public UWPUsbInterfaceEndpoint(UsbInterruptOutPipe inpipe, UsbInterruptInPipe outpipe)
        {
            InPipe = inpipe;
            OutPipe = outpipe;
        }
        #endregion
    }
}
