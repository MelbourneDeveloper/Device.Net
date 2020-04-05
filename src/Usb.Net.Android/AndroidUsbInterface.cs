using Android.Hardware.Usb;
using Device.Net;
using Device.Net.Exceptions;
using Java.Nio;
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
        public AndroidUsbInterface(UsbInterface usbInterface, UsbDeviceConnection usbDeviceConnection, ILogger logger, ITracer tracer, ushort? readBufferSize, ushort? writeBufferSize) : base(logger, tracer, readBufferSize, writeBufferSize)
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
                    Logger?.Log(Messages.ErrorMessageRead, nameof(AndroidUsbInterfaceManager), ex, LogLevel.Error);
                    throw new IOException(Messages.ErrorMessageRead, ex);
                }
            }, cancellationToken);
        }

        public async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await Task.Run(async () =>
            {
                try
                {
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
                    Logger?.Log(Messages.WriteErrorMessage, nameof(AndroidUsbInterface), ex, LogLevel.Error);
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
            if (!_UsbDeviceConnection.ClaimInterface(UsbInterface, true))
            {
                throw new DeviceException("could not claim interface");
            }

            return Task.FromResult(true);
        }
        #endregion
    }
}