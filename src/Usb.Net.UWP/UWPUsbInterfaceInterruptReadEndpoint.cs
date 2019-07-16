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

        #region Events
        private async void UsbInterruptInPipe_DataReceived(UsbInterruptInPipe sender, UsbInterruptInEventArgs args)
        {
            try
            {
                await _ReadLock.WaitAsync();

                var bytes = args.InterruptData.ToArray();

                if(_ReadChunkTaskCompletionSource!=null)
                {
                    //In this case there should be no chunks. TODO: Put some unit tests around this.
                    //The read method wil be waiting on this

                    //NOTE: The semaphore will get released here, and then in the read method. Is this OK?

                    _ReadChunkTaskCompletionSource.SetResult(bytes);
                    return;
                }

                _Chunks.Add(bytes);

                if (bytes != null)
                {
                    Logger?.Log($"{bytes.Length} read on interrupt pipe {UsbInterruptInPipe.EndpointDescriptor.EndpointNumber}", nameof(UWPUsbInterfaceInterruptReadEndpoint), null, LogLevel.Information);
                }
            }
            finally
            {
                _ReadLock.Release();
            }
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            _ReadLock.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<byte[]> ReadAsync()
        {
            try
            {
                await _ReadLock.WaitAsync();

                byte[] retVal = null;

                if (_Chunks.Count > 0)
                {
                    retVal = _Chunks[0];
                    Tracer?.Trace(false, retVal);
                    _Chunks.RemoveAt(0);
                    return retVal;
                }

                //Wait for the event here. Once the event occurs, this should return and the semaphore should be released
                _ReadChunkTaskCompletionSource = new TaskCompletionSource<byte[]>();
                retVal = await _ReadChunkTaskCompletionSource.Task;
                _ReadChunkTaskCompletionSource = null;
                return retVal;
            }
            finally
            {
                _ReadLock.Release();
            }
        }
        #endregion


    }
}
