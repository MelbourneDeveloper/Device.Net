using Device.Net;
using Microsoft.Extensions.Logging;
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
        #endregion

        #region Public Properties
        public ILogger Logger { get; }
        public ITracer Tracer { get; }
        #endregion

        #region Constructor
        public UWPUsbInterfaceInterruptReadEndpoint(UsbInterruptInPipe pipe, ILogger logger, ITracer tracer) : base(pipe)
        {
            Logger = logger;
            Tracer = tracer;
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
                    Logger?.LogInformation("{bytesLength} read on interrupt pipe {endpointNumber}", bytes.Length, UsbInterruptInPipe.EndpointDescriptor.EndpointNumber);
                }

                if (_ReadChunkTaskCompletionSource != null && _ReadChunkTaskCompletionSource.Task.Status != TaskStatus.RanToCompletion)
                {
                    //In this case there should be no chunks. TODO: Put some unit tests around this.
                    //The read method wil be waiting on this
                    var result = _Chunks[0];
                    _Chunks.RemoveAt(0);
                    _ReadChunkTaskCompletionSource.SetResult(result);
                    Logger?.LogInformation($"Completion source result set");
                    return;
                }
            }
            finally
            {
                _DataReceivedLock.Release();
            }
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            _ReadLock.Dispose();
            _DataReceivedLock.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<byte[]> ReadAsync(CancellationToken cancellationToken = default)
        {
            IDisposable logScope = null;

            try
            {
                logScope = Logger?.BeginScope("Endpoint descriptor: {endpointDescriptor} Call: {call}", UsbInterruptInPipe.EndpointDescriptor?.ToString(), nameof(ReadAsync));

                await _ReadLock.WaitAsync();

                byte[] retVal = null;

                try
                {
                    //Don't let any datas be added to the chunks here
                    await _DataReceivedLock.WaitAsync();

                    if (_Chunks.Count > 0)
                    {
                        retVal = _Chunks[0];
                        Tracer?.Trace(false, retVal);
                        _Chunks.RemoveAt(0);
                        Logger?.LogDebug(Messages.DebugMessageReadFirstChunk);
                        return retVal;
                    }
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, Messages.ErrorMessageRead);
                    throw;
                }
                finally
                {
                    _DataReceivedLock.Release();
                }

                //Wait for the event here. Once the event occurs, this should return and the semaphore should be released
                _ReadChunkTaskCompletionSource = new TaskCompletionSource<byte[]>();

                Logger?.LogDebug(Messages.DebugMessageLockReleased);

                //Cancel the completion source if the token is canceled
                using (cancellationToken.Register(() => { _ReadChunkTaskCompletionSource.TrySetCanceled(); }))
                {
                    await _ReadChunkTaskCompletionSource.Task;
                }

                _ReadChunkTaskCompletionSource = null;

                Logger?.LogDebug(Messages.DebugMessageCompletionSourceNulled);

                Tracer?.Trace(false, retVal);
                return retVal;
            }
            finally
            {
                logScope?.Dispose();
                _ReadLock.Release();
            }
        }
        #endregion
    }
}
