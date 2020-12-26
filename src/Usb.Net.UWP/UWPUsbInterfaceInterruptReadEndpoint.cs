using Device.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Usb;

namespace Usb.Net.UWP
{
    public class UWPUsbInterfaceInterruptReadEndpoint : UWPUsbInterfaceEndpoint<UsbInterruptInPipe>, IDisposable
    {
        #region Fields
        private readonly Collection<byte[]> _Chunks = new Collection<byte[]>();
        private readonly SemaphoreSlim _ReadLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _DataReceivedLock = new SemaphoreSlim(1, 1);
        private bool disposed;
        private TaskCompletionSource<byte[]> _ReadChunkTaskCompletionSource;
        private readonly ILogger _logger;
        #endregion

        #region Constructor
        public UWPUsbInterfaceInterruptReadEndpoint(UsbInterruptInPipe pipe, ILogger logger = null) : base(pipe)
        {
            _logger = logger ?? NullLogger.Instance;
            UsbInterruptInPipe.DataReceived += UsbInterruptInPipe_DataReceived;
        }
        #endregion

        //TODO: Put unit tests around locking here somehow

        #region Events
        private async void UsbInterruptInPipe_DataReceived(UsbInterruptInPipe sender, UsbInterruptInEventArgs args)
        {
            try
            {
                await _DataReceivedLock.WaitAsync();

                var bytes = args.InterruptData.ToArray();
                _Chunks.Add(bytes);

                if (bytes != null)
                {
                    _logger.LogInformation("{bytesLength} read on interrupt pipe {endpointNumber}", bytes.Length, UsbInterruptInPipe.EndpointDescriptor.EndpointNumber);
                }

                if (_ReadChunkTaskCompletionSource == null || _ReadChunkTaskCompletionSource.Task.Status == TaskStatus.RanToCompletion) return;

                //In this case there should be no chunks. TODO: Put some unit tests around this.
                //The read method wil be waiting on this
                var result = _Chunks[0];
                _Chunks.RemoveAt(0);
                _ReadChunkTaskCompletionSource.SetResult(result);
                _logger.LogInformation("Completion source result set");
            }
            finally
            {
                _ = _DataReceivedLock.Release();
            }
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

            _ReadLock.Dispose();
            _DataReceivedLock.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<byte[]> ReadAsync(CancellationToken cancellationToken = default)
        {
            using var logScope = _logger.BeginScope("Endpoint descriptor: {endpointDescriptor} Call: {call}", UsbInterruptInPipe.EndpointDescriptor?.ToString(), nameof(ReadAsync));

            try
            {
                await _ReadLock.WaitAsync(cancellationToken);

                byte[] retVal = null;

                try
                {
                    //Don't let any datas be added to the chunks here
                    await _DataReceivedLock.WaitAsync(cancellationToken);

                    if (_Chunks.Count > 0)
                    {
                        retVal = _Chunks[0];
                        _logger.LogTrace(new Trace(false, retVal));
                        _Chunks.RemoveAt(0);
                        _logger.LogDebug(Messages.DebugMessageReadFirstChunk);
                        return retVal;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, Messages.ErrorMessageRead);
                    throw;
                }
                finally
                {
                    _ = _DataReceivedLock.Release();
                }

                //Wait for the event here. Once the event occurs, this should return and the semaphore should be released
                _ReadChunkTaskCompletionSource = new TaskCompletionSource<byte[]>();

                _logger.LogDebug(Messages.DebugMessageLockReleased);

                //Cancel the completion source if the token is canceled
                using (cancellationToken.Register(() => _ReadChunkTaskCompletionSource.TrySetCanceled()))
                {
                    _ = await _ReadChunkTaskCompletionSource.Task;
                }

                _ReadChunkTaskCompletionSource = null;

                _logger.LogDebug(Messages.DebugMessageCompletionSourceNulled);

                _logger.LogTrace(new Trace(false, retVal));
                return retVal;
            }
            finally
            {
                _ = _ReadLock.Release();
            }
        }
        #endregion
    }
}
