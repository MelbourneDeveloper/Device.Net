#if !NET45

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Usb.Net;
using Usb.Net.Windows;
namespace Device.Net.UnitTests
{
    [TestClass]
    public class IntegrationTestControlTransfer
    {
        [TestMethod]
        public async Task Get_status_from_DFU_device_using_contol_transfer()
        {
            //Note: creating a LoggerFactory like this is easier than creating a mock
            var loggerFactory = LoggerFactory.Create((builder) =>
            {
                _ = builder.AddDebug().SetMinimumLevel(LogLevel.Trace);
            });

            var factory = new WindowsUsbDeviceFactory(loggerFactory);
            var deviceManager = new DeviceManager(loggerFactory);
            deviceManager.DeviceFactories.Add(factory);

            //Get the filtered devices
            var devices = await deviceManager.GetDevicesAsync(new List<FilterDeviceDefinition>
            {
                new FilterDeviceDefinition
                {
                    DeviceType= DeviceType.Usb,
                    VendorId= 0x0483,
                    ProductId=0xDF11,
                    //This does not affect the filtering
                    Label="USB Device in DFU Mode"
                },
            });

            //Get the first available device
            var dfuDevice = devices.FirstOrDefault();

            //Ensure that it gets picked up
            Assert.IsNotNull(dfuDevice);

            //Initialize the device
            await dfuDevice.InitializeAsync();


            ///////////////////////////////////////////////
            /// code that I am trying to verify against, can be removed later (or used as a seperate unit test directly against libUsb
            //byte[] buffer = new byte[6];
            ////int length = usb.controlTransfer(DFU_RequestType | USB_DIR_IN, DFU_GETSTATUS, 0, 0, buffer, 6, 500);
            //LibUsbDotNet.Main.UsbSetupPacket setup = new LibUsbDotNet.Main.UsbSetupPacket()
            //{
            //    RequestType = (byte)(DFU_RequestType | USB_DIR_IN),
            //    Request = (byte)DFU_GETSTATUS,
            //    Value = 0,
            //    Index = 0
            //};
            //int length = usb.ControlTransfer(setup, buffer, 0, 6);

            //if (length < 0)
            //{
            //    throw new Exception("USB Failed during getStatus");
            //}
            //status.bStatus = buffer[0]; // state during request
            //status.bState = buffer[4]; // state after request
            //status.bwPollTimeout = (buffer[3] & 0xFF) << 16;
            //status.bwPollTimeout |= (buffer[2] & 0xFF) << 8;
            //status.bwPollTimeout |= (buffer[1] & 0xFF);
            ///
            ///////////////////////////////////////////////

            const byte DFU_GETSTATUS = 0x03;
            var buffer = new byte[6];

            var setupPacket = new UsbSetupPacket
            {
                RequestType = new UsbControlRequestType
                {
                    Direction = UsbTransferDirection.In,
                    Recipient = UsbControlRecipient.Device, //probably not correct, needs verifying
                    ControlTransferType = UsbControlTransferType.Class,
                },
                Request = DFU_GETSTATUS,
                Length = (uint)buffer.Length
            };

            //var response = await dfuDevice.SendControlInTransferAsync(setupPacket, buffer); //for when async is added...
            var response = dfuDevice.SendControlOutTransfer(setupPacket, buffer);

            //Possible return states
            //const int STATE_IDLE = 0x00; //usually this one???
            //const int STATE_DETACH = 0x01;
            //const int STATE_DFU_IDLE = 0x02; //or would it be this one???
            //const int STATE_DFU_DOWNLOAD_SYNC = 0x03;
            //const int STATE_DFU_DOWNLOAD_BUSY = 0x04;
            //const int STATE_DFU_DOWNLOAD_IDLE = 0x05;
            //const int STATE_DFU_MANIFEST_SYNC = 0x06;
            //const int STATE_DFU_MANIFEST = 0x07;
            //const int STATE_DFU_MANIFEST_WAIT_RESET = 0x08;
            //const int STATE_DFU_UPLOAD_IDLE = 0x09;
            //const int STATE_DFU_ERROR = 0x0A;
            //const int STATE_DFU_UPLOAD_SYNC = 0x91;
            //const int STATE_DFU_UPLOAD_BUSY = 0x92;


            //Specify the response part of the Message Contract
            var expectedResult = new byte[] { 0x02 }; //lets try STATE_DFU_IDLE first (since we are in DFU mode)...


            //Assert that the response part meets the specification
            Assert.IsTrue(expectedResult.Equals(buffer[4]));
        }
    }
}

#endif
