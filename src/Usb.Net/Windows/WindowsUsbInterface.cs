using Device.Net;
using Device.Net.Windows;
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
        public WindowsUsbInterface(SafeFileHandle handle, ILogger logger, ITracer tracer, byte interfaceNumber, ushort? readBufferSize, ushort? writeBufferSzie) : base(logger, tracer, readBufferSize, writeBufferSzie)
        {
            _SafeFileHandle = handle;
            InterfaceNumber = interfaceNumber;
        }
        #endregion

        #region Public Methods
        public async Task<ReadResult> ReadAsync(uint bufferLength, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var bytes = new byte[bufferLength];
                var isSuccess = WinUsbApiCalls.WinUsb_ReadPipe(_SafeFileHandle, ReadEndpoint.PipeId, bytes, bufferLength, out var bytesRead, IntPtr.Zero);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't read data");
                Tracer?.Trace(false, bytes);
                return new ReadResult(bytes, bytesRead);
            }, cancellationToken);
        }

        public async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                var isSuccess = WinUsbApiCalls.WinUsb_WritePipe(_SafeFileHandle, WriteEndpoint.PipeId, data, (uint)data.Length, out var bytesWritten, IntPtr.Zero);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't write data");
                Tracer?.Trace(true, data);
            }, cancellationToken);
        }

        public void Dispose()
        {
            if (_IsDisposed) return;
            _IsDisposed = true;

            //This is a native resource, so the IDisposable pattern should probably be implemented...
            var isSuccess = WinUsbApiCalls.WinUsb_Free(_SafeFileHandle);
            WindowsDeviceBase.HandleError(isSuccess, "Interface could not be disposed");

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
