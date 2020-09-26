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
        public byte PipeId => (byte)UsbEndpoint.Address;
        public ushort MaxPacketSize => (ushort)UsbEndpoint.MaxPacketSize;
        public int InterfaceNumber { get; }

        public AndroidUsbEndpoint(UsbEndpoint usbEndpoint, int interfaceNumber, ILogger logger)
        {
            _logger = logger ?? NullLogger.Instance;

            if (usbEndpoint == null) throw new ArgumentNullException(nameof(usbEndpoint));

            var isRead = usbEndpoint.Direction == UsbAddressing.In;
            var isWrite = usbEndpoint.Direction == UsbAddressing.Out;
            var isInterrupt = usbEndpoint.Type == UsbAddressing.XferInterrupt;

            IsRead = isRead;
            IsWrite = isWrite;
            IsInterrupt = isInterrupt;
            UsbEndpoint = usbEndpoint;
            InterfaceNumber = interfaceNumber;

            _logger.LogInformation("Endpoint found. Interface Number: {interfaceNumber} PipeId/Address {address} Direction: {direction} Type: {type} IsRead: {isRead} IsWrite: {isWrite}", interfaceNumber, usbEndpoint.Address, usbEndpoint.Direction, usbEndpoint.Type, IsRead, isWrite);
        }
    }
}