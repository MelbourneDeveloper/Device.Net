using Microsoft.Extensions.Logging;
using Windows.Devices.Usb;

namespace Usb.Net.UWP
{
    public class UWPUsbInterfaceEndpoint<T> : IUsbInterfaceEndpoint
    {
        private readonly ILogger _logger;

        #region Public Properties
        public UsbBulkOutPipe UsbBulkOutPipe => Pipe as UsbBulkOutPipe;
        public UsbInterruptOutPipe UsbInterruptOutPipe => Pipe as UsbInterruptOutPipe;
        public UsbInterruptInPipe UsbInterruptInPipe => Pipe as UsbInterruptInPipe;
        public UsbBulkInPipe UsbBulkInPipe => Pipe as UsbBulkInPipe;

        public T Pipe { get; }
        public bool IsRead => UsbInterruptInPipe != null || UsbBulkInPipe != null;
        public bool IsWrite => UsbInterruptOutPipe != null || UsbBulkOutPipe != null;
        public bool IsInterrupt => UsbInterruptOutPipe != null || UsbInterruptInPipe != null;
        //Which one?
        public byte PipeId { get; }
        public ushort MaxPacketSize { get; }
        #endregion

        #region Constructor
        public UWPUsbInterfaceEndpoint(T pipe, ILogger<UWPUsbInterfaceEndpoint<T>> logger)
        {
            _logger = logger;
            Pipe = pipe;
            var endpointDescriptorProperty = pipe.GetType().GetProperty(nameof(UsbBulkOutPipe.EndpointDescriptor));
            var endpointDescriptor = endpointDescriptorProperty.GetValue(pipe);
            var endpointNumberProperty = endpointDescriptor.GetType().GetProperty(nameof(UsbBulkOutEndpointDescriptor.EndpointNumber));
            PipeId = (byte)endpointNumberProperty.GetValue(endpointDescriptor);
            var maxPacketSizeProperty = endpointDescriptor.GetType().GetProperty(nameof(UsbBulkOutEndpointDescriptor.MaxPacketSize));
            var maxPacketSize = (uint)maxPacketSizeProperty.GetValue(endpointDescriptor);
            MaxPacketSize = (ushort)maxPacketSize;
            _logger.LogInformation("Found pipe: {pipeId} MaxPacketSize: {maxPacketSize}", PipeId, MaxPacketSize);
        }
        #endregion

        #region Public Overrides
        public override string ToString() => $"{typeof(T).Name} Endpoint: {PipeId}";
        #endregion
    }
}
