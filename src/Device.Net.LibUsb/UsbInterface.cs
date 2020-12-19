using LibUsbDotNet.Main;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Usb.Net;

namespace Device.Net.LibUsb
{
    public class UsbInterface : UsbInterfaceBase, IUsbInterface
    {
        private readonly byte _interfaceId;
        private readonly LibUsbDotNet.UsbDevice _usbDevice;

        public int Timeout { get; set; }

        public UsbInterface(
            LibUsbDotNet.UsbDevice usbDevice,
            byte interfaceId,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null,
            ILogger logger = null,
            int timeout = 1000) : base(logger, readBufferSize, writeBufferSize)
        {
            _usbDevice = usbDevice ?? throw new ArgumentNullException(nameof(usbDevice));
            Timeout = timeout;
            _interfaceId = interfaceId;
        }

        public override byte InterfaceNumber => _interfaceId;

        public void Dispose()
        {
        }

        public Task<TransferResult> ReadAsync(uint bufferLength, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var readEndpoint = (ReadEndpoint)ReadEndpoint;
                var buffer = new byte[bufferLength];
                _ = readEndpoint.UsbEndpointReader.Read(buffer, Timeout, out var bytesRead);
                Logger.LogTrace(new Trace(false, buffer));
                return new TransferResult(buffer, (uint)bytesRead);
            });
        }

        public Task<uint> WriteAsync(byte[] data, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var writeEndpoint = (WriteEndpoint)WriteEndpoint;
                var errorCode = writeEndpoint.UsbEndpointWriter.Write(data, Timeout, out var bytesWritten);
                if (errorCode == ErrorCode.Ok || errorCode == ErrorCode.Success)
                {
                    Logger.LogTrace(new Trace(true, data));
                    return (uint)bytesWritten;
                }

                var message = "Error. Write error code: {errorCode}";
                Logger.LogError(new Exception(message), message, errorCode);
                throw new IOException(message);
            }, cancellationToken);
        }

        public Task<TransferResult> PerformControlTransferAsync(SetupPacket setupPacket, byte[] buffer = null, CancellationToken cancellationToken = default)
        {
            if (setupPacket == null) throw new ArgumentNullException(nameof(setupPacket));
            buffer ??= new byte[setupPacket.Length];

            var sp = new UsbSetupPacket(
                (byte)setupPacket.RequestType.Type,
                setupPacket.Request,
                setupPacket.Value,
                setupPacket.Index,
                setupPacket.Length);

            var isSuccess = _usbDevice.ControlTransfer(ref sp, buffer, buffer.Length, out var length);

            return !isSuccess ? throw new ControlTransferException("LibUsb says no") : Task.FromResult(new TransferResult(buffer, (uint)length));
        }
    }
}
