using LibUsbDotNet;
using Usb.Net;

namespace Device.Net.LibUsb
{
    public class WriteEndpoint : IUsbInterfaceEndpoint
    {
        public UsbEndpointWriter UsbEndpointWriter { get; }

        public WriteEndpoint(UsbEndpointWriter usbEndpointWriter, ushort? maxPacketSize)
        {
            UsbEndpointWriter = usbEndpointWriter;

            if (maxPacketSize.HasValue) MaxPacketSize = maxPacketSize.Value;
        }

        public bool IsRead => false;

        public bool IsWrite => true;

        public bool IsInterrupt => false;

        public byte PipeId => UsbEndpointWriter.EpNum;

        public ushort MaxPacketSize { get; }
    }
}
