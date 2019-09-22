using System;
using System.IO;
using System.Threading.Tasks;
using LibUsbDotNet.Main;
using Usb.Net;

namespace Device.Net.LibUsb
{
    public class UsbInterface : UsbInterfaceBase, IUsbInterface
    {
        private byte _interfaceId;

        public int Timeout { get; set; }

        public UsbInterface(ILogger logger, ITracer tracer, ushort? readBufferSize, ushort? writeBufferSize, int timeout, byte interfaceId) : base(logger, tracer, readBufferSize, writeBufferSize)
        {
            Timeout = timeout;
            _interfaceId = interfaceId;
        }

        public override byte InterfaceNumber => _interfaceId;

        public void Dispose()
        {
        }

        public Task<ReadResult> ReadAsync(uint bufferLength)
        {
            return Task.Run(() =>
            {
                var readEndpoint = (ReadEndpoint)ReadEndpoint;
                var buffer = new byte[bufferLength];
                readEndpoint.UsbEndpointReader.Read(buffer, Timeout, out var bytesRead);
                Tracer?.Trace(false, buffer);
                return new ReadResult(buffer, (uint)bytesRead);
            });
        }

        public Task WriteAsync(byte[] data)
        {
            return Task.Run(() =>
            {
                var writeEndpoint = (WriteEndpoint)WriteEndpoint;
                var errorCode = writeEndpoint.UsbEndpointWriter.Write(data, Timeout, out var bytesWritten);
                if (errorCode == ErrorCode.Ok || errorCode == ErrorCode.Success)
                {
                    Tracer?.Trace(true, data);
                }
                else
                {
                    var message = $"Error. Write error code: {errorCode}";
                    Logger?.Log(message, GetType().Name, null, LogLevel.Error);
                    throw new IOException(message);
                }
            });
        }
    }
}
