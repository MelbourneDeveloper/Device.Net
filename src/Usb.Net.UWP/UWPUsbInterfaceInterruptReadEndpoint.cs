using Device.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Usb;

namespace Usb.Net.UWP
{
    public class UWPUsbInterfaceInterruptReadEndpoint : UWPUsbInterfaceEndpoint<UsbInterruptInPipe>, IDisposable
    {
        #region Fields
        private bool disposed;
        private readonly ILogger _logger;
        private readonly IDataReceiver _dataReceiver;
        #endregion

        #region Constructor
        public UWPUsbInterfaceInterruptReadEndpoint(
            UsbInterruptInPipe pipe,
            IDataReceiver dataReceiver,
            ILogger<UWPUsbInterfaceInterruptReadEndpoint> logger = null) : base(pipe, logger)
        {
            _logger = logger ?? (ILogger)NullLogger.Instance;
            UsbInterruptInPipe.DataReceived += UsbInterruptInPipe_DataReceived;
            _dataReceiver = dataReceiver;
        }
        #endregion

        //TODO: Put unit tests around locking here somehow

        #region Events
        private void UsbInterruptInPipe_DataReceived(
            UsbInterruptInPipe sender,
            UsbInterruptInEventArgs args)
        {
            var bytes = args?.InterruptData?.ToArray();
            if (bytes != null) _dataReceiver.DataReceived(new TransferResult(bytes, args.InterruptData.Length));
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            if (disposed)
            {
                _logger.LogWarning(Messages.WarningMessageAlreadyDisposed, Pipe?.ToString());
                return;
            }

            disposed = true;

            _logger.LogInformation(Messages.InformationMessageDisposingDevice, Pipe?.ToString());

            UsbInterruptInPipe.DataReceived -= UsbInterruptInPipe_DataReceived;

            GC.SuppressFinalize(this);
        }

        public async Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            var transferResult = await _dataReceiver.ReadAsync(cancellationToken);
            _logger.LogDataTransfer(new Trace(false, transferResult));
            return transferResult;
        }
        #endregion
    }
}
