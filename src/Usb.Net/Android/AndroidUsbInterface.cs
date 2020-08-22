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
        public AndroidUsbInterface(UsbInterface usbInterface, UsbDeviceConnection usbDeviceConnection, ILogger logger, ushort? readBufferSize, ushort? writeBufferSize) : base(logger, readBufferSize, writeBufferSize)
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
                IDisposable logScope = null;

                try
                {
                    logScope = Logger?.BeginScope("UsbInterface: {usbInterface} Call: {call}", UsbInterface.Id, nameof(ReadAsync));

                    var byteBuffer = ByteBuffer.Allocate((int)bufferLength);
                    var request = new UsbRequest();
                    var endpoint = ((AndroidUsbEndpoint)ReadEndpoint).UsbEndpoint;
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

                    Tracer?.Trace(false, buffers);

                    return buffers;
                }
                catch (Exception ex)
                {
                    Logger?.LogError(Messages.ErrorMessageRead);
                    throw new IOException(Messages.ErrorMessageRead, ex);
                }
                finally
                {
                    logScope?.Dispose();
                }

            }, cancellationToken);
        }

        public async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            if (data == null) throw new NotImplementedException();

            await Task.Run(async () =>
            {
                IDisposable logScope = null;

                try
                {
                    logScope = Logger?.BeginScope("UsbInterface: {usbInterface} Call: {call} Data Length: {writeLength}", UsbInterface.Id, nameof(WriteAsync), data.Length);

                    //TODO: Perhaps we should implement Batch Begin/Complete so that the UsbRequest is not created again and again. This will be expensive

                    var request = new UsbRequest();
                    var endpoint = ((AndroidUsbEndpoint)WriteEndpoint).UsbEndpoint;
                    request.Initialize(_UsbDeviceConnection, endpoint);
                    var byteBuffer = ByteBuffer.Wrap(data);

                    Tracer?.Trace(true, data);

#pragma warning disable CS0618 
                    request.Queue(byteBuffer, data.Length);
#pragma warning restore CS0618 
                    await _UsbDeviceConnection.RequestWaitAsync();
                }
                catch (Exception ex)
                {
                    Logger?.LogError(Messages.WriteErrorMessage);
                    throw new IOException(Messages.WriteErrorMessage, ex);
                }
                finally
                {
                    logScope?.Dispose();
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
            if (!_UsbDeviceConnection.ClaimInterface(UsbInterface, true))
            {
                throw new DeviceException("could not claim interface");
            }

            return Task.FromResult(true);
        }
        #endregion
    }
}