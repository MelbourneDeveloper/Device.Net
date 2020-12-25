using Device.Net;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
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
            ILogger logger = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSzie = null) : base(logger, readBufferSize, writeBufferSzie)
        {
            _SafeFileHandle = handle;
            InterfaceNumber = interfaceNumber;
        }
        #endregion

        #region Public Methods
        public async Task<TransferResult> ReadAsync(uint bufferLength, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var bytes = new byte[bufferLength];
                var isSuccess = WinUsbApiCalls.WinUsb_ReadPipe(_SafeFileHandle, ReadEndpoint.PipeId, bytes, bufferLength, out var bytesRead, IntPtr.Zero);
                _ = WindowsHelpers.HandleError(isSuccess, "Couldn't read data", Logger);
                Logger.LogTrace(new Trace(false, bytes));
                return new TransferResult(bytes, bytesRead);
            }, cancellationToken).ConfigureAwait(false);
        }

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
                Logger.LogTrace(new Trace(true, data));
                return bytesWritten;
            }, cancellationToken);

        public void Dispose()
        {
            if (_IsDisposed) return;
            _IsDisposed = true;

            //This is a native resource, so the IDisposable pattern should probably be implemented...
            var isSuccess = WinUsbApiCalls.WinUsb_Free(_SafeFileHandle);
            _ = WindowsHelpers.HandleError(isSuccess, "Interface could not be disposed", Logger);

            GC.SuppressFinalize(this);
        }

        public Task<TransferResult> PerformControlTransferAsync(SetupPacket setupPacket, byte[] buffer, CancellationToken cancellationToken = default)
        {
            return setupPacket == null
                ? throw new ArgumentNullException(nameof(setupPacket)) :
            Task.Run(() =>
            {
                using var scope = Logger.BeginScope("Perfoming Control Transfer {setupPacket}", setupPacket);

                try
                {

                    var transferBuffer = new byte[setupPacket.Length];

                    uint bytesTransferred = 0;

                    if (setupPacket.Length > 0)
                    {
                        if (setupPacket.RequestType.Direction == RequestDirection.Out)
                        {
                            ////Make a copy so we don't mess with the array passed in
                            Array.Copy(buffer, transferBuffer, buffer.Length);
                        }
                    }

                    var isSuccess = WinUsbApiCalls.WinUsb_ControlTransfer(_SafeFileHandle.DangerousGetHandle(),
                        setupPacket.ToWindowsSetupPacket(),
                        transferBuffer,
                        (uint)transferBuffer.Length,
                        ref bytesTransferred,
                        IntPtr.Zero);

                    _ = WindowsHelpers.HandleError(isSuccess, "Couldn't do a control transfer", Logger);

                    Logger.LogTrace(new Trace(setupPacket.RequestType.Direction == RequestDirection.Out, transferBuffer));
                    Logger.LogInformation("Control Transfer complete {setupPacket}", setupPacket);

                    return bytesTransferred != setupPacket.Length && setupPacket.RequestType.Direction == RequestDirection.In
                        ? throw new ControlTransferException($"Requested {setupPacket.Length} bytes but received {bytesTransferred}")
                        : new TransferResult(transferBuffer, bytesTransferred);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Error on {nameof(PerformControlTransferAsync)}");

                    throw;
                }

            }, cancellationToken);
        }

        #endregion
    }
}
