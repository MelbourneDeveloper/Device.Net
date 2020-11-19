using LibUsbDotNet.Main;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Usb.Net;

namespace Device.Net.LibUsb
{
    public class UsbInterface : UsbInterfaceBase, IUsbInterface
    {
        private readonly byte _interfaceId;

        public int Timeout { get; set; }

        public UsbInterface(ILogger logger, ushort? readBufferSize, ushort? writeBufferSize, int timeout, byte interfaceId) : base(logger, readBufferSize, writeBufferSize)
        {
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
                readEndpoint.UsbEndpointReader.Read(buffer, Timeout, out var bytesRead);
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
                else
                {
                    var message = $"Error. Write error code: {errorCode}";
                    Logger.LogError(message, GetType().Name, null, LogLevel.Error);
                    throw new IOException(message);
                }
            }, cancellationToken);
        }
    }
}
