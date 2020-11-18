#if !NET45

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

#if !WINDOWS_UWP
using Hid.Net.Windows;
using Usb.Net.Windows;
using Usb.Net;
#else
using Usb.Net.UWP;
using Hid.Net.UWP;
#endif

namespace Device.Net.UnitTests
{
    [TestClass]
    public class IntegrationTests

    {
        private ILoggerFactory loggerFactory;

        #region Tests
#if !WINDOWS_UWP
        [TestMethod]
        public async Task TestFindSTMDFUModeWithFactory()
        {
            var deviceFactory = new FilterDeviceDefinition(0x0483, 0xdf11)
                .CreateWindowsUsbDeviceFactory()
                .CreateWindowsUsbDeviceFactory(classGuid: WindowsDeviceConstants.GUID_DEVINTERFACE_USB_DEVICE);

            var devices = await deviceFactory.GetConnectedDeviceDefinitionsAsync();

            Assert.IsTrue(devices.Count() > 0);
        }

        [TestMethod]
        public async Task TestFindSTMDFUModeWithFactory2()
        {
            var device = await new FilterDeviceDefinition(0x0483, 0xdf11)
                .CreateWindowsUsbDeviceFactory(classGuid: WindowsDeviceConstants.GUID_DEVINTERFACE_USB_DEVICE)
                .GetFirstDeviceAsync();

            await device.InitializeAsync();
        }

        [TestMethod]
        public async Task TestConnectToSTMDFUMode()
        {
            const string deviceID = @"\\?\usb#vid_0483&pid_df11#00000008ffff#{a5dcbf10-6530-11d2-901f-00c04fb951ed}";
            var windowsUsbDevice = new UsbDevice(deviceID, new WindowsUsbInterfaceManager(deviceID));
            await windowsUsbDevice.InitializeAsync();
        }

        [TestMethod]
        public async Task TestSTMDFUModePerformControlTransferReceive()
        {
            const string deviceID = @"\\?\usb#vid_0483&pid_df11#00000008ffff#{a5dcbf10-6530-11d2-901f-00c04fb951ed}";
            var windowsUsbDevice = new UsbDevice(deviceID, new WindowsUsbInterfaceManager(deviceID, loggerFactory: loggerFactory));
            await windowsUsbDevice.InitializeAsync();

            const byte DFU_GETSTATUS = 0x03;
            const byte STATE_DFU_IDLE = 0x02;

            var getStatusSetupPacket = new SetupPacket
            (
                requestType: new UsbDeviceRequestType(
                    RequestDirection.In,
                    RequestType.Class,
                    RequestRecipient.Interface),
                request: DFU_GETSTATUS,
                length: 6
            );

            await windowsUsbDevice.PerformControlTransferWithRetry(async () =>
            {
                var dfuStatus = await windowsUsbDevice.PerformControlTransferAsync(getStatusSetupPacket);

                // Assert that the received buffer has the requested lenght ADN that DFU State (at position 4) is STATE_DFU_IDLE
                Assert.IsTrue(
                    dfuStatus.BytesTransferred == getStatusSetupPacket.Length &&
                    dfuStatus.Data[4] == STATE_DFU_IDLE);
            });
        }

        [TestMethod]
        public async Task TestSTMDFUModePerformControlTransferSend()
        {
            const string deviceID = @"\\?\usb#vid_0483&pid_df11#00000008ffff#{a5dcbf10-6530-11d2-901f-00c04fb951ed}";
            var windowsUsbDevice = new UsbDevice(deviceID, new WindowsUsbInterfaceManager(deviceID, loggerFactory: loggerFactory));
            await windowsUsbDevice.InitializeAsync();

            const byte DFU_DNLOAD = 0x01;
            const byte DFU_GETSTATUS = 0x03;
            const byte DFU_CLRSTATUS = 0x04;
            const byte STATE_DFU_IDLE = 0x02;
            const byte STATE_DFU_ERROR = 0x0A;
            const byte STATUS_errTARGET = 0x01;

            // this sequence aims to test the "send data to device" using the Control Transfer
            // executes a DFU "Set Address Pointer" command through the DFU DNLOAD request

            ////////////////////////////////////////////////////////////
            // required to perform a DFU Clear Status request beforehand

            var clearSatusSetupPacket = new SetupPacket
            (
                requestType: new UsbDeviceRequestType(
                    RequestDirection.Out,
                    RequestType.Class,
                    RequestRecipient.Interface),
                request: DFU_CLRSTATUS
            );

            TransferResult dfuRequestResult;

            dfuRequestResult = await windowsUsbDevice.PerformControlTransferAsync(clearSatusSetupPacket);

            ////////////////////////////////////
            // 1st step: send DFU_DNLOAD request

            // buffer: set address pointer command (0x21), address pointer (0x08000000)
            var buffer = new byte[] {
                0x21,
                0x00,
                0x00,
                0x00,
                0x08
            };

            var downloadRequestSetupPacket = new SetupPacket
            (
                requestType: new UsbDeviceRequestType(
                    RequestDirection.Out,
                    RequestType.Class,
                    RequestRecipient.Interface),
                request: DFU_DNLOAD,
                length: (ushort)buffer.Length
            );

            dfuRequestResult = await windowsUsbDevice.PerformControlTransferAsync(downloadRequestSetupPacket, buffer);

            // Assert that the bytes transfered match the buffer lenght
            Assert.IsTrue(dfuRequestResult.BytesTransferred == buffer.Length);

            ///////////////////////////////////////
            // 2nd step: send DFU_GETSTATUS request

            var getStatusSetupPacket = new SetupPacket
            (
                requestType: new UsbDeviceRequestType(
                    RequestDirection.In,
                    RequestType.Class,
                    RequestRecipient.Interface),
                request: DFU_GETSTATUS,
                length: 6
            );

            await windowsUsbDevice.PerformControlTransferWithRetry(async () =>
            {
                dfuRequestResult = await windowsUsbDevice.PerformControlTransferAsync(getStatusSetupPacket);

                // Assert that the received buffer has the requested lenght 
                Assert.IsTrue(dfuRequestResult.BytesTransferred == getStatusSetupPacket.Length );
            });

            // check DFU status
            Assert.IsTrue(dfuRequestResult.Data[4] != STATE_DFU_IDLE);

            ///////////////////////////////////////////////////////////////
            // 3rd step: send new DFU_GETSTATUS request to check execution

            await windowsUsbDevice.PerformControlTransferWithRetry(async () =>
            {
                dfuRequestResult = await windowsUsbDevice.PerformControlTransferAsync(getStatusSetupPacket);

                // Assert that the received buffer has the requested lenght 
                Assert.IsTrue(dfuRequestResult.BytesTransferred == getStatusSetupPacket.Length);
            });

            // check DFU status
            // status has to be different from STATUS_errTARGET
            // state has to be different from STATE_DFU_ERROR
            Assert.IsTrue(
                dfuRequestResult.Data[0] != STATUS_errTARGET &&
                dfuRequestResult.Data[4] != STATE_DFU_ERROR);
        }

#endif

        [TestMethod]
        public Task TestWriteAndReadFromTrezorUsb() => TestWriteAndReadFromTrezor(
            new FilterDeviceDefinition(vendorId: 0x1209, productId: 0x53C1, label: "Trezor One Firmware 1.7.x")
            .GetUsbDeviceFactory()
        );

        [TestMethod]
        public Task TestWriteAndReadFromTrezorHid() => TestWriteAndReadFromTrezor(
            new FilterDeviceDefinition(vendorId: 0x534C, productId: 0x0001, label: "Trezor One Firmware 1.6.x", usagePage: 65280)
            .GetHidDeviceFactory(0)
            );

        [TestMethod]
        public Task TestWriteAndReadFromKeepKeyUsb() => TestWriteAndReadFromTrezor(
        new FilterDeviceDefinition(vendorId: 0x2B24, productId: 0x2)
            .GetUsbDeviceFactory()
           );

        [TestMethod]
        public Task TestWriteAndReadFromTrezorModelTUsb() => TestWriteAndReadFromTrezor(
        new FilterDeviceDefinition(vendorId: 0x1209, productId: 0x53c1)
            .GetUsbDeviceFactory()
            );

        private async Task TestWriteAndReadFromTrezor(IDeviceFactory deviceFactory, int expectedDataLength = 64)
        {
            //Send the request part of the Message Contract
            var request = new byte[64];
            request[0] = 0x3f;
            request[1] = 0x23;
            request[2] = 0x23;

            var integrationTester = new IntegrationTester(
                deviceFactory);
            await integrationTester.TestAsync(request, AssertTrezorResult, expectedDataLength);
        }

        [TestMethod]
        public async Task TestWriteAndReadFromTemperHid()
        {
            //Send the request part of the Message Contract
            var request = new byte[9] { 0x00, 0x01, 0x80, 0x33, 0x01, 0x00, 0x00, 0x00, 0x00 };

            var filterDeviceDefinition = new FilterDeviceDefinition(vendorId: 0x413d, productId: 0x2107, usagePage: 65280);

            var integrationTester = new IntegrationTester(
                filterDeviceDefinition.GetHidDeviceFactory());
            await integrationTester.TestAsync(request, async (result, device) =>
            {
                Assert.IsTrue(device.IsInitialized);

                var temperatureTimesOneHundred = (result.Data[4] & 0xFF) + (result.Data[3] << 8);
                var temp = Math.Round(temperatureTimesOneHundred / 100.0m, 2, MidpointRounding.ToEven);

                //I think my room should pretty much always be between these temperatures
                Assert.IsTrue(temp > 10 && temp < 35);

#if WINDOWS_UWP
                var windowsHidDevice = (UWPHidDevice)device;
#else
                var windowsHidDevice = (WindowsHidDevice)device;
                //TODO: Share these with UWP
                Assert.AreEqual(9, device.ConnectedDeviceDefinition.ReadBufferSize);
                Assert.AreEqual(9, device.ConnectedDeviceDefinition.WriteBufferSize);
                Assert.AreEqual(9, windowsHidDevice.ReadBufferSize);
                Assert.AreEqual(9, windowsHidDevice.WriteBufferSize);
#endif
            }, 9);
        }

        [TestMethod]
        public async Task TestWriteAndReadFromNanoHid()
        {
            //Send the request part of the Message Contract
            var request = new byte[64];
            request[0] = 63;
            request[1] = 62;
            request[2] = 1;
            request[3] = 1;
            request[4] = 1;

            var filterDeviceDefinition = new FilterDeviceDefinition(productId: 4112, vendorId: 10741);

            var integrationTester = new IntegrationTester(
                filterDeviceDefinition.GetHidDeviceFactory());
            await integrationTester.TestAsync(request, async (result, device) =>
             {
                 Assert.AreEqual(64, result.Data.Length);
                 Assert.AreEqual(63, result.Data[0]);
                 Assert.AreEqual(62, result.Data[1]);

#if WINDOWS_UWP
                 var windowsHidDevice = (UWPHidDevice)device;
#else
                 var windowsHidDevice = (WindowsHidDevice)device;
                 //TODO: share this with UWP
                 Assert.AreEqual(DeviceType.Hid, device.ConnectedDeviceDefinition.DeviceType);
                 Assert.AreEqual("AirNetix", device.ConnectedDeviceDefinition.Manufacturer);
                 Assert.AreEqual(filterDeviceDefinition.ProductId, device.ConnectedDeviceDefinition.ProductId);
                 Assert.AreEqual(filterDeviceDefinition.VendorId, device.ConnectedDeviceDefinition.VendorId);
                 Assert.AreEqual("STS-170", device.ConnectedDeviceDefinition.ProductName);
                 Assert.AreEqual(64, device.ConnectedDeviceDefinition.ReadBufferSize);
                 Assert.AreEqual(64, device.ConnectedDeviceDefinition.WriteBufferSize);
                 Assert.AreEqual("000000000001", device.ConnectedDeviceDefinition.SerialNumber);
                 Assert.AreEqual((ushort)1, device.ConnectedDeviceDefinition.Usage);
                 Assert.AreEqual((ushort)65280, device.ConnectedDeviceDefinition.UsagePage);
                 Assert.AreEqual((ushort)256, device.ConnectedDeviceDefinition.VersionNumber);
                 Assert.AreEqual(64, windowsHidDevice.ReadBufferSize);
                 Assert.AreEqual(64, windowsHidDevice.WriteBufferSize);
#endif
             }, 64);
        }
        #endregion

        #region Setup
        [TestInitialize]
        public void Setup()
        {
            var hostBuilder = Host.CreateDefaultBuilder().
            ConfigureLogging((builderContext, loggingBuilder) =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                loggingBuilder.AddConsole((options) =>
                {
                    //This displays arguments from the scope
                    options.IncludeScopes = true;
                });
            });
            var host = hostBuilder.Build();
            loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        }
        #endregion

        #region Private Methods
        private static Task AssertTrezorResult(TransferResult responseData, IDevice device)
        {
            //Specify the response part of the Message Contract
            var expectedResult = new byte[] { 63, 35, 35 };

            //Assert that the response part meets the specification
            Assert.IsTrue(expectedResult.SequenceEqual(responseData.Data.Take(3)));

            return Task.FromResult(true);
        }
        #endregion
    }
}

#endif