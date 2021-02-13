using Android.Hardware.Usb;
using Device.Net;
using Device.Net.Exceptions;
using Java.Nio;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Usb.Net.Android
{
    internal class AndroidUsbInterface : UsbInterfaceBase, IUsbInterface
    {
        #region Fields
        private bool disposed;
        private readonly UsbDeviceConnection _UsbDeviceConnection;
        private readonly IAndroidFactory _androidFactory;
        private readonly int? _timeout;
        #endregion

        #region Constructor
        public AndroidUsbInterface(
            UsbInterface usbInterface,
            UsbDeviceConnection usbDeviceConnection,
            IAndroidFactory androidFactory,
            ILogger? logger = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null,
            int? timeout = null,
            Func<UsbDeviceConnection, SetupPacket, byte[]?, int?, Task<TransferResult>>? performControlTransferAsync = null)
            : base(
                  performControlTransferAsync != null ?
                  //A func was passed in
                  new PerformControlTransferAsync((sb, data, c) => performControlTransferAsync(usbDeviceConnection, sb, data, timeout)) :
                  //Use the default
                  new PerformControlTransferAsync((sb, data, c) => PerformControlTransferAndroid(usbDeviceConnection, sb, data, timeout, c)),
                logger,
                readBufferSize,
                writeBufferSize)
        {
            UsbInterface = usbInterface ?? throw new ArgumentNullException(nameof(usbInterface));
            _UsbDeviceConnection = usbDeviceConnection ?? throw new ArgumentNullException(nameof(usbDeviceConnection));
            _androidFactory = androidFactory;
            _timeout = timeout;
        }
        #endregion

        #region Public Properties
        public override byte InterfaceNumber => (byte)UsbInterface.Id;
        public UsbInterface UsbInterface { get; }
        #endregion

        #region Public Methods
        public Task<TransferResult> ReadAsync(uint bufferLength, CancellationToken cancellationToken = default)
        {
            return Task.Run(async () =>
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
                    var request = _androidFactory.CreateUsbRequest();
                    _ = request.Initialize(_UsbDeviceConnection, endpoint);
#pragma warning disable CS0618
                    _ = request.Queue(byteBuffer, (int)bufferLength);
#pragma warning restore CS0618

                    _ = _timeout.HasValue
                        //Note: two versions here in case they have different functionality. When both code paths are tested it's probably possible to remove one
                        ? await _UsbDeviceConnection.RequestWaitAsync(_timeout.Value).ConfigureAwait(false)
                        : await _UsbDeviceConnection.RequestWaitAsync().ConfigureAwait(false);

                    //TODO: Get the actual length of the data read instead of just returning the length of the array

                    var buffers = new TransferResult(new byte[bufferLength], bufferLength);

                    _ = byteBuffer.Rewind();

                    //Ouch. Super nasty
                    for (var i = 0; i < bufferLength; i++)
                    {
                        buffers.Data[i] = (byte)byteBuffer.Get();
                    }

                    //Marshal.Copy(byteBuffer.GetDirectBufferAddress(), buffers, 0, ReadBufferLength);

                    Logger.LogDataTransfer(new Trace(false, buffers));

                    return buffers;
                }
                catch (Exception ex)
                {
                    Logger.LogError(Messages.ErrorMessageRead);
                    throw new IOException(Messages.ErrorMessageRead, ex);
                }
            }, cancellationToken);
        }

        public Task<uint> WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            return Task.Run(async () =>
            {
                try
                {
                    //TODO: Perhaps we should implement Batch Begin/Complete so that the UsbRequest is not created again and again. This will be expensive

                    var request = _androidFactory.CreateUsbRequest();
                    var endpoint = ((AndroidUsbEndpoint)WriteEndpoint).UsbEndpoint;

                    using var logScope = Logger.BeginScope("UsbInterface: {usbInterface} Endpoint: {endpoint} Call: {call} Data Length: {writeLength}", UsbInterface.Id, endpoint.Address, nameof(WriteAsync), data.Length);
                    Logger.LogInformation("Before Write UsbInterface: {usbInterface} Endpoint: {endpoint} Call: {call} Data Length: {writeLength}", UsbInterface.Id, endpoint.Address, nameof(WriteAsync), data.Length);

                    _ = request.Initialize(_UsbDeviceConnection, endpoint);
                    var byteBuffer = ByteBuffer.Wrap(data);

#pragma warning disable CS0618
                    _ = request.Queue(byteBuffer, data.Length);
#pragma warning restore CS0618

                    _ = await _UsbDeviceConnection.RequestWaitAsync().ConfigureAwait(false);

                    //TODO: It's not clear if there is a way to count the number of bytes transferred here. This is a bug in a sense...

                    Logger.LogDataTransfer(new Trace(true, data));

                    return (uint)data.Length;
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
            if (disposed)
            {
                Logger.LogWarning(Messages.WarningMessageAlreadyDisposed, UsbInterface?.ToString());
                return;
            }

            disposed = true;

            Logger.LogInformation(Messages.InformationMessageDisposingDevice, UsbInterface?.ToString());

            UsbInterface?.Dispose();
            _UsbDeviceConnection.Dispose();
        }

        public override Task ClaimInterface()
        {
            Logger.LogInformation("Claimed interface {interfaceId}", InterfaceNumber);

            return !_UsbDeviceConnection.ClaimInterface(UsbInterface, true)
                ? throw new DeviceException("could not claim interface")
                : Task.FromResult(true);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This is the low level call to do a control transfer at the Android level. This can be overriden in the contructor
        /// </summary>
        private static Task<TransferResult> PerformControlTransferAndroid(
            UsbDeviceConnection usbDeviceConnection,
            SetupPacket setupPacket,
            byte[]? buffer = null,
            int? timeout = null,
            CancellationToken cancellationToken = default)
        =>
            //Use Task.Run so we can pass the cancellation token in instead of using the async control transfer method which doesn't have a cancellation token
            Task.Run(() =>
            {
                var bytesTransferred = usbDeviceConnection.ControlTransfer(
                    setupPacket.RequestType.Direction == RequestDirection.In ? UsbAddressing.In : UsbAddressing.Out,
                    setupPacket.Request,
                    setupPacket.Value,
                    setupPacket.Index,
                    buffer,
                    setupPacket.Length,
                    timeout ?? 0
                    );

                return new TransferResult(buffer, (uint)bytesTransferred);
            }, cancellationToken);

        #endregion
    }
}