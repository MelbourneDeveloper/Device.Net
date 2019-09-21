using System;
using System.Threading.Tasks;
using Usb.Net;

namespace Device.Net.LibUsb
{
    public class DummyInterface : UsbInterfaceBase, IUsbInterface
    {
        public int Timeout { get; set; }

        public DummyInterface(ILogger logger, ITracer tracer, ushort? readBufferSize, ushort? writeBufferSize, int timeout) : base(logger, tracer, readBufferSize, writeBufferSize)
        {
            Timeout = timeout;
        }

        public override byte InterfaceNumber => throw new NotImplementedException();

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
               return new ReadResult(buffer, (uint)bytesRead);
            });
        }

        public Task WriteAsync(byte[] data)
        {
            return Task.Run(() =>
            {
                var writeEndpoint = (WriteEndpoint)WriteEndpoint;
                writeEndpoint.UsbEndpointWriter.Write(data, Timeout, out var bytesWritten);
            });
        }
    }
}
