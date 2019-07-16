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
        public ILogger Logger { get; }
        public ITracer Tracer { get; }

        protected bool IsReading { get; set; }
        protected Collection<byte[]> Chunks { get; } = new Collection<byte[]>();
        protected TaskCompletionSource<byte[]> ReadChunkTaskCompletionSource { get; set; }

        #region Constructor
        public UWPUsbInterfaceInterruptReadEndpoint(UsbInterruptInPipe pipe, ILogger logger, ITracer tracer) : base(pipe)
        {
            Logger = logger;
            Tracer = tracer;
            UsbInterruptInPipe.DataReceived += UsbInterruptInPipe_DataReceived;
        }

        private void UsbInterruptInPipe_DataReceived(UsbInterruptInPipe sender, UsbInterruptInEventArgs args)
        {
            HandleDataReceived(args.InterruptData.ToArray());
        }

        protected void HandleDataReceived(byte[] bytes)
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

        public  async Task<byte[]> ReadAsync()
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
