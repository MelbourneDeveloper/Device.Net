using Device.Net;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32.SafeHandles;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbInterface : UsbInterfaceBase, IUsbInterface
    {
        #region Private Properties
        private bool _IsDisposed;
        private readonly SafeFileHandle _SafeFileHandle;
        /// <summary>
        /// TODO: Make private?
        /// </summary>

        #endregion

        #region Public Properties
        public override byte InterfaceNumber { get; }
        #endregion

        #region Constructor
        public WindowsUsbInterface(
            SafeFileHandle handle,
            byte interfaceNumber,
            ILogger? logger = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSzie = null,
            Func<SafeFileHandle, SetupPacket, byte[], CancellationToken, Task<TransferResult>>? performControlTransferAsync = null) :
            base(
                  performControlTransferAsync != null ?
                  //A func was passed in
                  new PerformControlTransferAsync((sb, data, c) => performControlTransferAsync(handle, sb, data, c)) :
                  //Use the default
                  new PerformControlTransferAsync((sb, data, c) => PerformControlTransferWindowsAsync(handle, sb, data, logger ?? NullLogger.Instance, c)),
                logger,
                readBufferSize,
                writeBufferSzie)
        {
            _SafeFileHandle = handle;
            InterfaceNumber = interfaceNumber;
        }
        #endregion

        #region Public Methods
        public Task<TransferResult> ReadAsync(uint bufferLength, CancellationToken cancellationToken = default) =>
            Task.Run(() =>
            {
                var bytes = new byte[bufferLength];
                var isSuccess = WinUsbApiCalls.WinUsb_ReadPipe(_SafeFileHandle, ReadEndpoint.PipeId, bytes, bufferLength, out var bytesRead, IntPtr.Zero);
                _ = WindowsHelpers.HandleError(isSuccess, "Couldn't read data", Logger);
                var transferResult = new TransferResult(bytes, bytesRead);
                Logger.LogDataTransfer(new Trace(false, transferResult));
                return transferResult;
            }, cancellationToken);

        public Task<uint> WriteAsync(byte[] data, CancellationToken cancellationToken = default)
            => Task.Run(() =>
            {
                var isSuccess = WinUsbApiCalls.WinUsb_WritePipe(
                    _SafeFileHandle,
                    WriteEndpoint.PipeId,
                    data,
                    (uint)data.Length,
                    out var bytesWritten,
                    IntPtr.Zero);
                _ = WindowsHelpers.HandleError(isSuccess, "Couldn't write data", Logger);
                Logger.LogDataTransfer(new Trace(true, data));
                return bytesWritten;
            }, cancellationToken);

        public void Dispose()
        {
            if (_IsDisposed)
            {
                Logger.LogWarning(Messages.WarningMessageAlreadyDisposed, InterfaceNumber);
                return;
            }

            _IsDisposed = true;

            Logger.LogInformation(Messages.InformationMessageDisposingDevice, InterfaceNumber);

            //This is a native resource, so the IDisposable pattern should probably be implemented...
            var isSuccess = WinUsbApiCalls.WinUsb_Free(_SafeFileHandle);
            _ = WindowsHelpers.HandleError(isSuccess, "Interface could not be disposed", Logger);

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Private Methods
        private static Task<TransferResult> PerformControlTransferWindowsAsync(
            SafeFileHandle safeFileHandle,
            SetupPacket setupPacket,
            byte[]? buffer,
            ILogger logger,
            CancellationToken cancellationToken = default) =>
            Task.Run(() =>
            {
                uint bytesTransferred = 0;

                //This is just because the API call requires this
                buffer ??= new byte[0];

                var isSuccess = WinUsbApiCalls.WinUsb_ControlTransfer(safeFileHandle.DangerousGetHandle(),
                    setupPacket.ToWindowsSetupPacket(),
                    buffer,
                    (uint)buffer.Length,
                    ref bytesTransferred,
                    IntPtr.Zero);

                _ = WindowsHelpers.HandleError(isSuccess, "Couldn't do a control transfer", logger);

                return new TransferResult(buffer, bytesTransferred);

            }, cancellationToken);

        #endregion
    }
}
