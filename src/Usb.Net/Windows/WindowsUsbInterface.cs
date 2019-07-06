using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbInterface : UsbInterfaceBase, IDisposable, IUsbInterface
    {
        #region Private Properties
        private  bool _IsDisposed;
        private readonly SafeFileHandle _SafeFileHandle;
        /// <summary>
        /// TODO: Make private?
        /// </summary>

        #endregion

        #region Constructor
        public WindowsUsbInterface(SafeFileHandle handle, ILogger logger, ITracer tracer, ushort readBufferSize, ushort writeBufferSize) : base(logger, tracer, readBufferSize, writeBufferSize)
        {
            _SafeFileHandle = handle;
        }
        #endregion


        #region Public Methods
        public async Task<byte[]> ReadAsync(uint bufferLength)
        {
            return await Task.Run(() =>
            {
                var bytes = new byte[bufferLength];
                var isSuccess = WinUsbApiCalls.WinUsb_ReadPipe(_SafeFileHandle, ReadEndpoint.PipeId, bytes, bufferLength, out var bytesRead, IntPtr.Zero);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't read data");
                Tracer?.Trace(false, bytes);
                return bytes;
            });
        }

        public async Task<byte[]> ReadInterruptAsync(uint bufferLength, uint timeout)
        {
            return await Task.Run(() =>
            {
                if (!SetTimeout(timeout))
                    throw new ApplicationException($"Unable to Set timeout.");

                var bytes = new byte[bufferLength];
                var isSuccess = WinUsbApiCalls.WinUsb_ReadPipe(_SafeFileHandle, InterruptEndpoint.PipeId, bytes, bufferLength, out var bytesRead, IntPtr.Zero);
                //TODO: Should get Last error here and if it's Timeout, don't handle error?
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't read data from interrupt pipe"); //TODO: Error code 121 is timeout, I think we should ignore this?
                Tracer?.Trace(false, bytes);
                return bytes;
            });
        }

        public async Task WriteAsync(byte[] data)
        {
            await Task.Run(() =>
            {
                var isSuccess = WinUsbApiCalls.WinUsb_WritePipe(_SafeFileHandle, WriteEndpoint.PipeId, data, (uint)data.Length, out var bytesWritten, IntPtr.Zero);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't write data");
                Tracer?.Trace(true, data);
            });
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

        #region Private Methods
        private bool SetTimeout(uint timeout)
        {
            if (!WinUsbApiCalls.WinUsb_SetPipePolicy(_SafeFileHandle, InterruptEndpoint.PipeId, 0x03, sizeof(uint), ref timeout))
                return false;

            return true;
        }
        #endregion
    }
}
