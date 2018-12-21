using Device.Net.UWP;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Devices.Usb;

namespace Usb.Net.UWP
{
    public class UWPUsbDevice : UWPDeviceBase<UsbDevice>
    {
        #region Event Handlers

        private byte[] InputReportToBytes(HidInputReportReceivedEventArgs args)
        {
            byte[] bytes;
            using (var stream = args.Report.Data.AsStream())
            {
                bytes = new byte[args.Report.Data.Length];
                stream.Read(bytes, 0, (int)args.Report.Data.Length);
            }

            return bytes;
        }
        #endregion

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
            _ConnectedDevice = await GetDevice(DeviceId);

            if (_ConnectedDevice != null)
            {
                var usbInterface = _ConnectedDevice.Configuration.UsbInterfaces.FirstOrDefault();
                var fsdfsdf = usbInterface.InterruptInPipes.FirstOrDefault();
                fsdfsdf.DataReceived += Fsdfsdf_DataReceived;

                RaiseConnected();
            }
            else
            {
                throw new Exception($"Could not connect to device with Device Id {DeviceId}. Check that the package manifest has been configured to allow this device.");
            }
        }

        private void Fsdfsdf_DataReceived(UsbInterruptInPipe sender, UsbInterruptInEventArgs args)
        {
            var asdasd = args.InterruptData.ToArray();
        }

        private static async Task<UsbDevice> GetDevice(string id)
        {
            var hidDeviceOperation = UsbDevice.FromIdAsync(id);
            var task = hidDeviceOperation.AsTask();
            var hidDevice = await task;
            return hidDevice;
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
