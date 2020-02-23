using Device.Net;
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
                    Logger?.Log($"{bytes.Length} read on interrupt pipe {UsbInterruptInPipe.EndpointDescriptor.EndpointNumber}", nameof(UWPUsbInterfaceInterruptReadEndpoint), null, LogLevel.Information);
                }

                if (_ReadChunkTaskCompletionSource != null && _ReadChunkTaskCompletionSource.Task.Status!= TaskStatus.RanToCompletion)
                {
                    //In this case there should be no chunks. TODO: Put some unit tests around this.
                    //The read method wil be waiting on this
                    var result = _Chunks[0];
                    _Chunks.RemoveAt(0);
                    _ReadChunkTaskCompletionSource.SetResult(result);
                    Logger?.Log($"Completion source result set", nameof(UWPUsbInterfaceInterruptReadEndpoint), null, LogLevel.Information);
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
            try
            {
                Logger?.Log($"Read called on {nameof(UWPUsbInterfaceInterruptReadEndpoint)}", nameof(UWPUsbInterfaceInterruptReadEndpoint), null, LogLevel.Information);

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
                        Logger?.Log($"Read the first chunk {nameof(UWPUsbInterfaceInterruptReadEndpoint)}", nameof(UWPUsbInterfaceInterruptReadEndpoint), null, LogLevel.Information);
                        return retVal;
                    }
                }
                catch(Exception ex)
                {
                    Logger?.Log($"Error {nameof(ReadAsync)}", nameof(UWPUsbInterfaceInterruptReadEndpoint), ex, LogLevel.Error);
                    throw;
                }
                finally
                {
                    _DataReceivedLock.Release();                    
                }

                //Wait for the event here. Once the event occurs, this should return and the semaphore should be released
                _ReadChunkTaskCompletionSource = new TaskCompletionSource<byte[]>();

                Logger?.Log($"Data received lock released. Completion source created. Waiting for data.", nameof(UWPUsbInterfaceInterruptReadEndpoint), null, LogLevel.Information);

                await SynchronizeWithCancellationToken(async () => { return  _ReadChunkTaskCompletionSource.Task; }, cancellationToken);
              
                _ReadChunkTaskCompletionSource = null;

                Logger?.Log($"Completion source nulled", nameof(UWPUsbInterfaceInterruptReadEndpoint), null, LogLevel.Information);

                Tracer?.Trace(false, retVal);
                return retVal;
            }
            finally
            {
                _ReadLock.Release();
            }
        }
        #endregion

        public static async Task SynchronizeWithCancellationToken(Task task, CancellationToken cancellationToken = default)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            var cancelTask = Task.Run(() =>
            {
                while (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    //TODO: Soft code this
                    Task.Delay(10);

                    if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(Messages.ErrorMessageOperationCanceled);
                }
            });

            await Task.WhenAny(new Task[] { task, cancelTask });
        }
    }
}
