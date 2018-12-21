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
        public UWPUsbDevice()
        {
        }

        public UWPUsbDevice(string deviceId)
        {
            DeviceId = deviceId;
        }
        #endregion

        #region Private Methods
        public async Task InitializeAsync()
        {
            _ConnectedDevice = await GetDevice(DeviceId);

            if (_ConnectedDevice != null)
            {
                var usbInterface = _ConnectedDevice.Configuration.UsbInterfaces.FirstOrDefault();
                var fsdfsdf = usbInterface.InterruptInPipes.FirstOrDefault();
                //var usbBulkOutPipe = usbInterface.BulkOutPipes.FirstOrDefault();
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

        public void Dispose()
        {
            _ConnectedDevice.Dispose();
        }

        public async Task<byte[]> ReadAsync()
        {

            var dataPacketLength = (uint)64;

            var buffer = new Windows.Storage.Streams.Buffer(dataPacketLength);

            //var setupPacket = new UsbSetupPacket()
            //{
            //    RequestType = new UsbControlRequestType()
            //    {
            //        Recipient = UsbControlRecipient.Endpoint,
            //        //Direction = UsbTransferDirection.In
            //    },
            //    Length = dataPacketLength
            //};

            //var buffer2 = await _ConnectedDevice.SendControlInTransferAsync(setupPacket, buffer);

            //var bytes = new byte[64];
            //var stream = buffer2.AsStream();
            //stream.Read(bytes, 0, 64);

            //var returnValue = buffer2.ToArray();

            //var usbInterface = _ConnectedDevice.Configuration.UsbInterfaces.FirstOrDefault();
            //var fsdfsdf = usbInterface.InterruptInPipes.FirstOrDefault();
            ////var usbBulkOutPipe = usbInterface.BulkOutPipes.FirstOrDefault();
            //await fsdfsdf.DataReceived


            //return returnValue;

            return null;
        }

        public async Task WriteAsync(byte[] bytes)
        {

            //var writer = new DataWriter();

            //writer.WriteBytes(bytes);

            //// The buffer with the data
            //var bufferToSend = writer.DetachBuffer();
            var bufferToSend = bytes.AsBuffer();
            try
            {

                //var setupPacket = new UsbSetupPacket()
                //{
                //    RequestType = new UsbControlRequestType()
                //    {
                //        Direction = UsbTransferDirection.Out,
                //        Recipient = UsbControlRecipient.Endpoint,
                         
                //        //Direction = UsbTransferDirection.Out,
                //    },
                //    Value = 9,
                //    Length = bufferToSend.Length
                //};
                var usbInterface = _ConnectedDevice.Configuration.UsbInterfaces.FirstOrDefault();
                var fsdfsdf = usbInterface.InterruptOutPipes.FirstOrDefault();
                //var usbBulkOutPipe = usbInterface.BulkOutPipes.FirstOrDefault();
                await fsdfsdf.OutputStream.WriteAsync(bufferToSend);
                //await _ConnectedDevice.SendControlOutTransferAsync(setupPacket, bufferToSend);
            }
            catch (ArgumentException ex)
            {
                //TODO: Check the string is nasty. Validation on the size of the array being sent should be done earlier anyway
                if (ex.Message == "Value does not fall within the expected range.")
                {
                    throw new Exception("It seems that the data being sent to the device does not match the accepted size. Have you checked DataHasExtraByte?", ex);
                }
            }

            Tracer?.Trace(false, bytes);
        }
        #endregion
    }
}
