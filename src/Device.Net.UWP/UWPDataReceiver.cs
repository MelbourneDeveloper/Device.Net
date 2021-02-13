using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.UWP
{
    public class UwpDataReceiver : IDisposable, IDataReceiver
    {
        #region Fields
        private readonly Queue<TransferResult> _readQueue = new Queue<TransferResult>();
        private readonly SemaphoreSlim _readLock = new SemaphoreSlim(1, 1);
        private bool disposed;
        private TaskCompletionSource<TransferResult>? _readChunkTaskCompletionSource;
        private readonly ILogger _logger;
        private readonly IDisposable _dataReceivedSubscription;
        private readonly IObserver<TransferResult> _dataReceived;
        #endregion

        #region Public Properties
        public bool HasData => _readQueue.Count > 0;
        #endregion

        #region Constructor
        public UwpDataReceiver(
            IObservable<TransferResult> dataRecievedObservable,
            ILogger? logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            _dataReceived = new Observer<TransferResult>(DataReceived);
            _dataReceivedSubscription = dataRecievedObservable.Subscribe(_dataReceived);
        }
        #endregion

        #region Public Methods
        public void DataReceived(TransferResult bytes)
        {
            _logger.LogTrace("{infoMessage}{trace}",
                _readChunkTaskCompletionSource != null ? "Setting completion source..." : "Enqueuing data..."
                , new Trace(false, bytes));

            if (_readChunkTaskCompletionSource != null)
            {
                _readChunkTaskCompletionSource.SetResult(bytes);
            }
            else
            {
                _readQueue.Enqueue(bytes);
            }
        }

        public async Task<TransferResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"Calling {nameof(ReadAsync)}");

            try
            {
                await _readLock.WaitAsync(cancellationToken);

                TransferResult bytes = default;

                if (_readQueue.Count == 0)
                {
                    _logger.LogDebug("Creating a completion source...");
                    _readChunkTaskCompletionSource = new TaskCompletionSource<TransferResult>();

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
                _logger.LogWarning(Messages.WarningMessageAlreadyDisposed, nameof(UwpDataReceiver));
                return;
            }

            disposed = true;
            _logger.LogInformation(Messages.InformationMessageDisposingDevice, nameof(UwpDataReceiver));
            _dataReceivedSubscription.Dispose();
            _readLock.Dispose();
        }
        #endregion
    }
}
