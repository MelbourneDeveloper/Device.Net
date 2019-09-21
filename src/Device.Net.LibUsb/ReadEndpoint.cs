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

    public class WriteEndpoint : IUsbInterfaceEndpoint
    {
        public UsbEndpointWriter UsbEndpointWriter { get; }

        public WriteEndpoint(UsbEndpointWriter usbEndpointWriter, ushort? maxPacketSize)
        {
            UsbEndpointWriter = usbEndpointWriter;

            if (maxPacketSize.HasValue) MaxPacketSize = maxPacketSize.Value;
        }

        public bool IsRead => true;

        public bool IsWrite => false;

        public bool IsInterrupt => false;

        public byte PipeId => UsbEndpointWriter.EpNum;

        public ushort MaxPacketSize { get; }
    }
}
