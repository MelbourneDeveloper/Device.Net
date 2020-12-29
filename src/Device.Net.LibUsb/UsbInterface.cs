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
        #region Fields
        private readonly byte _interfaceId;
        private readonly LibUsbDotNet.UsbDevice _usbDevice;
        private bool disposed;
        #endregion

        #region Public Properties
        public int Timeout { get; set; }
        public override byte InterfaceNumber => _interfaceId;
        #endregion

        #region Constructor
        public UsbInterface(
            LibUsbDotNet.UsbDevice usbDevice,
            byte interfaceId,
            ushort? readBufferSize = null,
            ushort? writeBufferSize = null,
            ILogger logger = null,
            int timeout = 1000,
            Func<LibUsbDotNet.UsbDevice, SetupPacket, byte[], int?, Task<TransferResult>> performControlTransferAsync = null)
            : base(
                  performControlTransferAsync != null ?
                  //A func was passed in
                  new PerformControlTransferAsync((sb, data, c) => performControlTransferAsync(usbDevice, sb, data, timeout)) :
                  //Use the default
                  new PerformControlTransferAsync((sb, data, c) => PerformControlTransferLibUsbAsync(usbDevice, sb, data, c)),

                   logger, readBufferSize, writeBufferSize)
        {
            _usbDevice = usbDevice ?? throw new ArgumentNullException(nameof(usbDevice));
            Timeout = timeout;
            _interfaceId = interfaceId;
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            if (disposed)
            {
                Logger.LogWarning(Messages.WarningMessageAlreadyDisposed, _usbDevice?.DevicePath);
                return;
            }

            disposed = true;

            Logger.LogInformation(Messages.InformationMessageDisposingDevice, _usbDevice?.DevicePath);

            _ = _usbDevice.Close();
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
                Logger.LogError(new IOException(message), message, errorCode);
                throw new IOException(message);
            }, cancellationToken);
        }
        #endregion

        #region Private Methods
        private static Task<TransferResult> PerformControlTransferLibUsbAsync(LibUsbDotNet.UsbDevice usbDevice, SetupPacket setupPacket, byte[] buffer = null, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                if (setupPacket == null) throw new ArgumentNullException(nameof(setupPacket));
                buffer ??= new byte[setupPacket.Length];

                var sp = new UsbSetupPacket(
                    (byte)setupPacket.RequestType.Type,
                    setupPacket.Request,
                    setupPacket.Value,
                    setupPacket.Index,
                    setupPacket.Length);

                var isSuccess = usbDevice.ControlTransfer(ref sp, buffer, buffer.Length, out var length);

                return !isSuccess ? throw new ControlTransferException("LibUsb says no") : Task.FromResult(new TransferResult(buffer, (uint)length));
            }, cancellationToken);
        }
        #endregion
    }
}
