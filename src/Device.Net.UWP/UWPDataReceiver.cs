using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.UWP
{
    public class UWPDataReceiver : IDisposable, IDataReceiver
    {
        #region Fields
        private readonly Queue<byte[]> _readQueue = new Queue<byte[]>();
        private readonly SemaphoreSlim _readLock = new SemaphoreSlim(1, 1);
        private bool disposed;
        private TaskCompletionSource<byte[]> _readChunkTaskCompletionSource;
        private readonly ILogger _logger;
        private readonly IDisposable _dataReceivedSubscription;
        private readonly IObserver<byte[]> _dataReceived;
        #endregion

        #region Constructor
        public UWPDataReceiver(
            IObservable<byte[]> dataRecievedObservable,
            ILogger logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _dataReceived = new Observer<byte[]>(DataReceived);
            _dataReceivedSubscription = dataRecievedObservable.Subscribe(_dataReceived);
        }
        #endregion

        #region Public Methods
        public void DataReceived(byte[] bytes)
        {
            _logger.LogDebug("Received data - Length: {dataLength}. {infoMessage} {state}",
                bytes.Length,
                _readChunkTaskCompletionSource != null ? "Setting completion source..." : "Enqueuing data"
                , new Trace(false, bytes));

            if (_readChunkTaskCompletionSource != null)
            {
                _logger.LogDebug("Setting result of task completion source");
                _readChunkTaskCompletionSource.SetResult(bytes);
            }
            else
            {
                _readQueue.Enqueue(bytes);
            }
        }

        public async Task<byte[]> ReadAsync(CancellationToken cancellationToken = default)
        {
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

        public void Dispose()
        {
            if (disposed)
            {
                _logger.LogWarning(Messages.WarningMessageAlreadyDisposed, nameof(UWPDataReceiver));
                return;
            }

            disposed = true;
            _logger.LogInformation(Messages.InformationMessageDisposingDevice, nameof(UWPDataReceiver));
            _dataReceivedSubscription.Dispose();
            _readLock.Dispose();
        }
        #endregion
    }
}
