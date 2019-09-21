using System;
using LibUsbDotNet;
using Usb.Net;

namespace Device.Net.LibUsb
{
    public class ReadEndpoint : IUsbInterfaceEndpoint
    {
        public UsbEndpointReader UsbEndpointReader { get; }

        public ReadEndpoint(UsbEndpointReader usbEndpointReader, ushort? maxPacketSize)
        {
            UsbEndpointReader = usbEndpointReader;

            if (maxPacketSize.HasValue) MaxPacketSize = maxPacketSize.Value;
        }

        public bool IsRead => true;

        public bool IsWrite => false;

        public bool IsInterrupt => false;

        public byte PipeId => UsbEndpointReader.EpNum;

        public ushort MaxPacketSize { get; }
    }
}
