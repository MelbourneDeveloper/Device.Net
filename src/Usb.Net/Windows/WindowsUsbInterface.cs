﻿using Device.Net;
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
        public async Task<ReadResult> ReadAsync(uint bufferLength, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var bytes = new byte[bufferLength];
                var isSuccess = WinUsbApiCalls.WinUsb_ReadPipe(_SafeFileHandle, ReadEndpoint.PipeId, bytes, bufferLength, out var bytesRead, IntPtr.Zero);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't read data");
                Logger.LogTrace(new Trace(false, bytes));
                return new ReadResult(bytes, bytesRead);
            }, cancellationToken);
        }

        public async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                var isSuccess = WinUsbApiCalls.WinUsb_WritePipe(_SafeFileHandle, WriteEndpoint.PipeId, data, (uint)data.Length, out var bytesWritten, IntPtr.Zero);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't write data");
                Logger.LogTrace(new Trace(true, data));
            }, cancellationToken);
        }

        //TODO: make async?
        //TODO: WINUSB_SETUP_PACKET not exposed
        public uint SendControlOutTransfer(ISetupPacket setupPacket, byte[] buffer)
        {
            //TODO: convert from ISetupPacket to WINUSB_SETUP_PACKET
            var winSetupPacket = new WINUSB_SETUP_PACKET();

            uint bytesWritten = 0;

            if (buffer != null && buffer.Length > 0)
            {
                WinUsbApiCalls.WinUsb_ControlTransfer(_SafeFileHandle.DangerousGetHandle(), winSetupPacket, buffer, (uint)buffer.Length, ref bytesWritten, IntPtr.Zero); //last pointer is overlapped structure for async operations
            }
            else
            {
                throw new Exception("Buffer must not be empty");
            }
            return bytesWritten;
        }

        //TODO: make async?
        //TODO: WINUSB_SETUP_PACKET not exposed
        public uint SendControlInTransfer(ISetupPacket setupPacket)
        {
            //TODO: convert from ISetupPacket to WINUSB_SETUP_PACKET
            var winSetupPacket = new WINUSB_SETUP_PACKET();
            uint bytesWritten = 0;
            var buffer = Array.Empty<byte>();

            WinUsbApiCalls.WinUsb_ControlTransfer(_SafeFileHandle.DangerousGetHandle(), winSetupPacket, buffer, (uint)buffer.Length, ref bytesWritten, IntPtr.Zero); //last pointer is overlapped structure for async operations
            return bytesWritten;
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
