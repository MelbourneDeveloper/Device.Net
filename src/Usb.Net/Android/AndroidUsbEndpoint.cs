using Android.Hardware.Usb;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Usb.Net.Android
{
    public class AndroidUsbEndpoint : IUsbInterfaceEndpoint
    {
        private readonly ILogger _logger;

        public UsbEndpoint UsbEndpoint { get; }
        public bool IsRead { get; }
        public bool IsWrite { get; }
        public bool IsInterrupt { get; }
        public byte PipeId => (byte)UsbEndpoint.EndpointNumber;
        public ushort MaxPacketSize => (ushort)UsbEndpoint.MaxPacketSize;
        public int InterfaceNumber { get; }

        public AndroidUsbEndpoint(UsbEndpoint usbEndpoint, int interfaceNumber, ILogger logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            UsbEndpoint = usbEndpoint ?? throw new ArgumentNullException(nameof(usbEndpoint));

            IsRead = usbEndpoint.Direction == UsbAddressing.In;
            IsWrite = usbEndpoint.Direction == UsbAddressing.Out;
            IsInterrupt = usbEndpoint.Type == UsbAddressing.XferInterrupt;
            InterfaceNumber = interfaceNumber;

            _logger.LogInformation("Endpoint found. Interface Number: {interfaceNumber} EndpointNumber {endpointNumber} Address: {address} Attributes: {attributes} Direction: {direction} Type: {type} IsRead: {isRead} IsWrite: {isWrite} MaxPacketSize: {maxPacketSize}",
                interfaceNumber,
                usbEndpoint.EndpointNumber,
                usbEndpoint.Address,
                usbEndpoint.Attributes,
                usbEndpoint.Direction,
                usbEndpoint.Type,
                IsRead,
                IsWrite,
                UsbEndpoint.MaxPacketSize);
        }
    }
}