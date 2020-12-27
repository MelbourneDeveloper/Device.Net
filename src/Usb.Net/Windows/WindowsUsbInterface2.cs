using Device.Net;
using Device.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbInterface2 : IUsbInterface2
    {
        private readonly SafeFileHandle _SafeFileHandle;
        private readonly ILogger<WindowsUsbInterface2> _logger;

        public WindowsUsbInterface2(SafeFileHandle safeFileHandle, ILogger<WindowsUsbInterface2> logger)
        {
            _SafeFileHandle = safeFileHandle;
            _logger = logger;
        }

        public void Close()
        {
            //This is a native resource, so the IDisposable pattern should probably be implemented...
            var isSuccess = WinUsbApiCalls.WinUsb_Free(_SafeFileHandle);
            _ = WindowsHelpers.HandleError(isSuccess, "Interface could not be disposed", _logger);
        }

        public Task<TransferResult> ControlTransferAsync(SetupPacket setupPacket, byte[] data) => throw new NotImplementedException();

        public async Task<TransferResult> ReadFromEndpointAsync(IUsbInterfaceEndpoint endpoint, uint bufferLength, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                var bytes = new byte[bufferLength];
                var isSuccess = WinUsbApiCalls.WinUsb_ReadPipe(_SafeFileHandle, endpoint.PipeId, bytes, bufferLength, out var bytesRead, IntPtr.Zero);
                _ = WindowsHelpers.HandleError(isSuccess, "Couldn't read data", _logger);
                return new TransferResult(bytes, bytesRead);
            }, cancellationToken).ConfigureAwait(false);
        }
        public Task<uint> WriteToEndpointAsync(IUsbInterfaceEndpoint endpoint, byte[] data) => throw new NotImplementedException();
    }
}
