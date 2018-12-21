using Device.Net.UWP;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Usb;
using Windows.Foundation;

namespace Usb.Net.UWP
{
    public class UWPUsbDevice : UWPDeviceBase<UsbDevice>
    {
        #region Constructors
        public UWPUsbDevice() : base()
        {
        }

        public UWPUsbDevice(string deviceId) : base()
        {
        }
        #endregion

        #region Private Methods
        public override async Task InitializeAsync()
        {
            await GetDevice(DeviceId);

            if (_ConnectedDevice != null)
            {
                var usbInterface = _ConnectedDevice.Configuration.UsbInterfaces.FirstOrDefault();
                var interruptPipe = usbInterface.InterruptInPipes.FirstOrDefault();
                interruptPipe.DataReceived += InterruptPipe_DataReceived;

                RaiseConnected();
            }
            else
            {
                throw new Exception($"Could not connect to device with Device Id {DeviceId}. Check that the package manifest has been configured to allow this device.");
            }
        }

        protected override IAsyncOperation<UsbDevice> FromIdAsync(string id)
        {
            return UsbDevice.FromIdAsync(id);
        }

        #endregion

        #region Event Handlers
        private void InterruptPipe_DataReceived(UsbInterruptInPipe sender, UsbInterruptInEventArgs args)
        {
            HandleDataReceived(args.InterruptData.ToArray());
        }
        #endregion

        #region Public Methods
        public override async Task WriteAsync(byte[] bytes)
        {
            var bufferToSend = bytes.AsBuffer();
            var usbInterface = _ConnectedDevice.Configuration.UsbInterfaces.FirstOrDefault();
            var outPipe = usbInterface.InterruptOutPipes.FirstOrDefault();
            await outPipe.OutputStream.WriteAsync(bufferToSend);

            Tracer?.Trace(false, bytes);
        }
        #endregion
    }
}
