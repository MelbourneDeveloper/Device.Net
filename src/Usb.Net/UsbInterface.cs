using Device.Net;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Usb.Net
{
    public class UsbInterface : UsbInterfaceBase, IUsbInterface
    {
        #region Private Properties
        private bool _IsDisposed;
        private readonly Func<IUsbInterfaceEndpoint, Task<TransferResult>> _readFromEndpoint;
        private readonly Func<IUsbInterfaceEndpoint, byte[], Task<uint>> _writeToEndpoint;
        private readonly Func<SetupPacket, byte[], Task<TransferResult>> _controlTransfer;
        private readonly Action _freeInterface;
        #endregion

        #region Public Properties
        public override byte InterfaceNumber { get; }
        #endregion

        #region Constructor
        public UsbInterface(
            byte interfaceNumber,
            ILogger logger = null,
            ushort? readBufferSize = null,
            ushort? writeBufferSzie = null) : base(
                logger,
                readBufferSize,
                writeBufferSzie) => InterfaceNumber = interfaceNumber;
        #endregion

        #region Public Methods
        public async Task<TransferResult> ReadAsync(uint bufferLength, CancellationToken cancellationToken = default)
        {
            return await Task.Run(async () =>
            {
                var transferResult = await _readFromEndpoint(ReadEndpoint).ConfigureAwait(false);
                Logger.LogTrace(new Trace(false, transferResult.Data));
                return transferResult;
            }, cancellationToken).ConfigureAwait(false);
        }

        public Task<uint> WriteAsync(byte[] data, CancellationToken cancellationToken = default)
            => Task.Run(async () =>
            {
                var bytesWritten = await _writeToEndpoint(WriteEndpoint, data);
                Logger.LogTrace(new Trace(true, data));
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

            _freeInterface();

            GC.SuppressFinalize(this);
        }

        public Task<TransferResult> PerformControlTransferAsync(SetupPacket setupPacket, byte[] buffer, CancellationToken cancellationToken = default)
        {
            return setupPacket == null
                ? throw new ArgumentNullException(nameof(setupPacket)) :
            Task.Run(async () =>
            {
                using var scope = Logger.BeginScope("Perfoming Control Transfer {setupPacket}", setupPacket);

                try
                {
                    var transferBuffer = new byte[setupPacket.Length];

                    if (setupPacket.Length > 0)
                    {
                        if (setupPacket.RequestType.Direction == RequestDirection.Out)
                        {
                            ////Make a copy so we don't mess with the array passed in
                            Array.Copy(buffer, transferBuffer, buffer.Length);
                        }
                    }

                    var transferResult = await _controlTransfer(setupPacket, transferBuffer);

                    Logger.LogTrace(new Trace(setupPacket.RequestType.Direction == RequestDirection.Out, transferBuffer));
                    Logger.LogInformation("Control Transfer complete {setupPacket}", setupPacket);

                    return transferResult.BytesTransferred != setupPacket.Length && setupPacket.RequestType.Direction == RequestDirection.In
                        ? throw new ControlTransferException($"Requested {setupPacket.Length} bytes but received {transferResult.BytesTransferred}")
                        : transferResult;
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
