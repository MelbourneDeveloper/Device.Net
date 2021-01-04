using Device.Net;
using Device.Net.UWP;
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
        private readonly IDataReceiver _UWPDataReceiver;
        #endregion

        #region Constructor
        public UWPUsbInterfaceInterruptReadEndpoint(
            UsbInterruptInPipe pipe,
            IDataReceiver uwpDataReceiver,
            ILogger logger = null) : base(pipe)
        {
            _logger = logger ?? NullLogger.Instance;
            UsbInterruptInPipe.DataReceived += UsbInterruptInPipe_DataReceived;
            _UWPDataReceiver = uwpDataReceiver;
        }
        #endregion

        //TODO: Put unit tests around locking here somehow

        #region Events
        private void UsbInterruptInPipe_DataReceived(
            UsbInterruptInPipe sender,
            UsbInterruptInEventArgs args)
        {
            var bytes = args?.InterruptData?.ToArray();
            if (bytes != null) _UWPDataReceiver.DataReceived(bytes);
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

        public Task<byte[]> ReadAsync(CancellationToken cancellationToken = default) => _UWPDataReceiver.ReadAsync(cancellationToken);
        #endregion
    }
}
