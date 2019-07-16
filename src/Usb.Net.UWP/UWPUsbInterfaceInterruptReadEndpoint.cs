using Device.Net;
using Device.Net.Exceptions;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Usb;

namespace Usb.Net.UWP
{
    public class UWPUsbInterfaceInterruptReadEndpoint : UWPUsbInterfaceEndpoint<UsbInterruptInPipe> 
    {
        #region Fields
        private bool IsReading { get; set; }
        private Collection<byte[]> Chunks { get; } = new Collection<byte[]>();
        private TaskCompletionSource<byte[]> ReadChunkTaskCompletionSource { get; set; }
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
        private void UsbInterruptInPipe_DataReceived(UsbInterruptInPipe sender, UsbInterruptInEventArgs args)
        {
            HandleDataReceived(args.InterruptData.ToArray());
        }
        #endregion

        #region Private Methods
        private void HandleDataReceived(byte[] bytes)
        {
            if (!IsReading)
            {
                lock (Chunks)
                {
                    Chunks.Add(bytes);
                }
            }
            else
            {
                IsReading = false;
                ReadChunkTaskCompletionSource.SetResult(bytes);
            }
        }
        #endregion

        #region Public Methods
        public async Task<byte[]> ReadAsync()
        {
            if (IsReading)
            {
                throw new DeviceException("Reentry");
            }

            //TODO: this should be a semaphore not a lock
            lock (Chunks)
            {
                if (Chunks.Count > 0)
                {
                    var retVal = Chunks[0];
                    Tracer?.Trace(false, retVal);
                    Chunks.RemoveAt(0);
                    return retVal;
                }
            }

            IsReading = true;
            ReadChunkTaskCompletionSource = new TaskCompletionSource<byte[]>();
            return await ReadChunkTaskCompletionSource.Task;
        }
        #endregion


    }
}
