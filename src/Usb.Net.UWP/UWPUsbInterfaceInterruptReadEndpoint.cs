using Device.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Usb;

namespace Usb.Net.UWP
{
    public class UWPUsbInterfaceInterruptReadEndpoint : UWPUsbInterfaceEndpoint<UsbInterruptInPipe>, IDisposable
    {
        #region Fields
        private readonly Queue<byte[]> _readQueue = new Queue<byte[]>();
        private readonly SemaphoreSlim _readLock = new SemaphoreSlim(1, 1);
        private bool disposed;
        private TaskCompletionSource<byte[]> _readChunkTaskCompletionSource;
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
        private void UsbInterruptInPipe_DataReceived(UsbInterruptInPipe sender, UsbInterruptInEventArgs args)
        {
            var bytes = args?.InterruptData?.ToArray();

            _logger.LogInformation("{bytesLength} read on interrupt pipe {endpointNumber}", bytes?.Length, UsbInterruptInPipe.EndpointDescriptor.EndpointNumber);

            if (_readChunkTaskCompletionSource != null)
            {
                _logger.LogDebug("Setting result of task completion source");
                _readChunkTaskCompletionSource.SetResult(bytes);
            }
            else
            {
                var state = new Trace(false, bytes);

                _logger.Log(LogLevel.Debug, default, state, null, (s, e) => $"Enqueing data... {state}");

                _readQueue.Enqueue(bytes);
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

            _readLock.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<byte[]> ReadAsync(CancellationToken cancellationToken = default)
        {
            using var logScope = _logger.BeginScope("Endpoint descriptor: {endpointDescriptor} Call: {call}", UsbInterruptInPipe.EndpointDescriptor?.ToString(), nameof(ReadAsync));

            _logger.LogInformation($"Calling {nameof(ReadAsync)}");

            try
            {
                await _readLock.WaitAsync(cancellationToken);

                byte[] bytes = null;

                if (_readQueue.Count == 0)
                {
                    _logger.LogDebug("Creating a completion source...");
                    _readChunkTaskCompletionSource = new TaskCompletionSource<byte[]>();

                    //Cancel the completion source if the token is canceled
                    using (cancellationToken.Register(() => _readChunkTaskCompletionSource.TrySetCanceled()))
                    {
                        _logger.LogDebug("Awaiting completion source...");
                        bytes = await _readChunkTaskCompletionSource.Task;
                        _readChunkTaskCompletionSource = null;
                        _logger.LogDebug(Messages.DebugMessageCompletionSourceNulled);
                    }
                }
                else
                {
                    //We already have the data
                    bytes = _readQueue.Dequeue();
                    _logger.LogDebug("Dequeued data");
                }

                _logger.LogTrace(new Trace(false, bytes));
                _logger.LogDebug(Messages.DebugMessageReadFirstChunk);

                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading");
                throw;
            }
            finally
            {
                //Just in case
                _readChunkTaskCompletionSource = null;
                _ = _readLock.Release();
            }
        }
        #endregion
    }
}
