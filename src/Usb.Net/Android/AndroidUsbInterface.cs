using Android.Hardware.Usb;
using Device.Net;
using Device.Net.Exceptions;
using Java.Nio;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net.Android
{
    internal class AndroidUsbInterface : UsbInterfaceBase, IUsbInterface
    {
        #region Fields
        private bool disposed;
        private readonly UsbDeviceConnection _UsbDeviceConnection;
        #endregion

        #region Constructor
        public AndroidUsbInterface(
            UsbInterface usbInterface,
            UsbDeviceConnection usbDeviceConnection,
            ILogger logger = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null) : base(logger, readBufferSize, writeBufferSize)
        {
            UsbInterface = usbInterface ?? throw new ArgumentNullException(nameof(usbInterface));
            _UsbDeviceConnection = usbDeviceConnection ?? throw new ArgumentNullException(nameof(usbDeviceConnection));
        }
        #endregion

        #region Public Properties
        public override byte InterfaceNumber => (byte)UsbInterface.Id;
        public UsbInterface UsbInterface { get; }
        #endregion

        #region Public Methods
        public async Task<ReadResult> ReadAsync(uint bufferLength, CancellationToken cancellationToken = default)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    //TODO: validate here
                    var endpoint = ((AndroidUsbEndpoint)ReadEndpoint).UsbEndpoint;

                    using var logScope = Logger.BeginScope(
                        "UsbInterface: {usbInterface} Call: {call} Endpoint Id: {endpointId}",
                        UsbInterface.Id,
                        nameof(ReadAsync),
                        endpoint.EndpointNumber);
                    var byteBuffer = ByteBuffer.Allocate((int)bufferLength);
                    var request = new UsbRequest();
                    request.Initialize(_UsbDeviceConnection, endpoint);
#pragma warning disable CS0618
                    request.Queue(byteBuffer, (int)bufferLength);
#pragma warning restore CS0618
                    await _UsbDeviceConnection.RequestWaitAsync();

                    //TODO: Get the actual length of the data read instead of just returning the length of the array

                    var buffers = new ReadResult(new byte[bufferLength], bufferLength);

                    byteBuffer.Rewind();

                    //Ouch. Super nasty
                    for (var i = 0; i < bufferLength; i++)
                    {
                        buffers.Data[i] = (byte)byteBuffer.Get();
                    }

                    //Marshal.Copy(byteBuffer.GetDirectBufferAddress(), buffers, 0, ReadBufferLength);

                    Logger.LogTrace(new Trace(false, buffers));

                    return buffers;
                }
                catch (Exception ex)
                {
                    Logger.LogError(Messages.ErrorMessageRead);
                    throw new IOException(Messages.ErrorMessageRead, ex);
                }
            }, cancellationToken);
        }

        public async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            if (data == null) throw new NotImplementedException();

            await Task.Run(async () =>
            {
                try
                {
                    //TODO: Perhaps we should implement Batch Begin/Complete so that the UsbRequest is not created again and again. This will be expensive

                    var request = new UsbRequest();
                    var endpoint = ((AndroidUsbEndpoint)WriteEndpoint).UsbEndpoint;

                    using var logScope = Logger.BeginScope("UsbInterface: {usbInterface} Endpoint: {endpoint} Call: {call} Data Length: {writeLength}", UsbInterface.Id, endpoint.Address, nameof(WriteAsync), data.Length);
                    Logger.LogInformation("Before Write UsbInterface: {usbInterface} Endpoint: {endpoint} Call: {call} Data Length: {writeLength}", UsbInterface.Id, endpoint.Address, nameof(WriteAsync), data.Length);


                    request.Initialize(_UsbDeviceConnection, endpoint);
                    var byteBuffer = ByteBuffer.Wrap(data);

#pragma warning disable CS0618
                    request.Queue(byteBuffer, data.Length);
#pragma warning restore CS0618

                    await _UsbDeviceConnection.RequestWaitAsync();

                    Logger.LogTrace(new Trace(true, data), $"Write endpoint: {endpoint.Address}");
                }
                catch (Exception ex)
                {
                    Logger.LogError(Messages.WriteErrorMessage);
                    throw new IOException(Messages.WriteErrorMessage, ex);
                }
            }, cancellationToken);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            UsbInterface.Dispose();
            _UsbDeviceConnection.Dispose();
        }

        public override Task ClaimInterface()
        {
            Logger.LogInformation("Claimed interface {interfaceId}", InterfaceNumber);

            return !_UsbDeviceConnection.ClaimInterface(UsbInterface, true)
                ? throw new DeviceException("could not claim interface")
                : Task.FromResult(true);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public Task<ReadResult> SendControlTransferAsync(SetupPacket setupPacket, byte[] buffer) => throw new NotImplementedException();
#pragma warning restore IDE0060 // Remove unused parameter
        #endregion
    }
}