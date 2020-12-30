
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Usb.Net;
using System.Collections.Generic;
using System.Diagnostics;

#if !WINDOWS_UWP
using Device.Net.LibUsb;
using Hid.Net.Windows;
using Usb.Net.Windows;
using Device.Net.Windows;
#else
using Hid.Net.UWP;
#endif

namespace Device.Net.UnitTests
{
    [TestClass]
    [TestCategory("NotPipelineReady")]
    public class IntegrationTests
    {
        private const byte STATE_DFU_IDLE = 0x02;
        private const byte STATE_DFU_ERROR = 0x0A;
        private const byte STATUS_errTARGET = 0x01;
        public const int TrezorVendorId = 0x1209;
        public const int TrezorOneProductId = 0x53C1;
        public const int StmDfuVendorId = 0x0483;
        public const int StmDfuProductId = 0xdf11;

#if !NET45
        private readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => _ = builder.AddDebug().SetMinimumLevel(LogLevel.Trace));
#else
        private readonly ILoggerFactory _loggerFactory;
#endif

        #region Tests

#if !WINDOWS_UWP
        [TestMethod]
        public async Task TestFindSTMDFUModeWithFactory()
        {
            var deviceFactory = new FilterDeviceDefinition(StmDfuVendorId, StmDfuProductId)
                .CreateWindowsUsbDeviceFactory(classGuid: WindowsDeviceConstants.GUID_DEVINTERFACE_USB_DEVICE);

            var devices = await deviceFactory.GetConnectedDeviceDefinitionsAsync();

            Assert.IsTrue(devices.Count() > 0);
        }

        [TestMethod]
        public async Task TestFindSTMDFUModeWithFactory2()
        {
            var device = await new FilterDeviceDefinition(StmDfuVendorId, StmDfuProductId)
                .CreateWindowsUsbDeviceFactory(classGuid: WindowsDeviceConstants.GUID_DEVINTERFACE_USB_DEVICE)
                .GetFirstDeviceAsync();

            await device.InitializeAsync();
        }

        [TestMethod]
        public async Task TestConnectToSTMDFUMode_GUID_DEVINTERFACE_USB_DEVICE()
        {
            const string deviceID = @"\\?\usb#vid_0483&pid_df11#00000008ffff#{a5dcbf10-6530-11d2-901f-00c04fb951ed}";
            var windowsUsbDevice = new UsbDevice(deviceID, new WindowsUsbInterfaceManager(deviceID));
            await windowsUsbDevice.InitializeAsync();
        }

        [TestMethod]
        public async Task TestSTMDFUModePerformControlTransferReceive_GUID_DEVINTERFACE_USB_DEVICE()
        {
            const string deviceID = @"\\?\usb#vid_0483&pid_df11#00000008ffff#{a5dcbf10-6530-11d2-901f-00c04fb951ed}";
            var windowsUsbDevice = new UsbDevice(deviceID, new WindowsUsbInterfaceManager(deviceID, loggerFactory: loggerFactory));
            await windowsUsbDevice.InitializeAsync();

            await windowsUsbDevice.ClearStatusAsync();

            var dfuStatus = await windowsUsbDevice.PerformControlTransferWithRetry(ud => windowsUsbDevice.GetStatusAsync());

            //TODO: This sometimes fails. Perhaps it should be part of the retry?

            // Assert that the received buffer has the requested lenght ADN that DFU State (at position 4) is STATE_DFU_IDLE
            Assert.IsTrue(
                dfuStatus.BytesTransferred == StmDfuExtensions.GetStatusPacketLength &&
                dfuStatus.Data[4] == STATE_DFU_IDLE);
        }

        [TestMethod]
        public async Task TestSTMDFUModePerformControlTransferSend_GUID_DEVINTERFACE_USB_DEVICE()
        {
            const string deviceID = @"\\?\usb#vid_0483&pid_df11#00000008ffff#{a5dcbf10-6530-11d2-901f-00c04fb951ed}";
            var stmDfuDevice = new UsbDevice(deviceID,
                new WindowsUsbInterfaceManager(deviceID, loggerFactory: loggerFactory)
                );

            await stmDfuDevice.InitializeAsync();

            await PerformStmDfTest(stmDfuDevice);
        }

        [TestMethod]
        public async Task TestSTMDFUModePerformControlTransferSend_DefaultGuid_WinUSBGuid()
        {
            var stmDfuDevice = await new FilterDeviceDefinition(StmDfuVendorId, StmDfuProductId)
                .CreateWindowsUsbDeviceFactory(loggerFactory)
                .ConnectFirstAsync();

            await PerformStmDfTest((IUsbDevice)stmDfuDevice);
        }

        [TestMethod]
        public async Task TestSTMDFUModePerformControlTransferSend_Zadig()
        {
            //This is the Zadig driver on my machine apparently...
            const string deviceID = @"\\\\?\\usb#vid_0483&pid_df11#00000008ffff#{f1e6f51b-72ea-43e1-b267-30056cd69e81}";
            var stmDfuDevice = new UsbDevice(deviceID,
                new WindowsUsbInterfaceManager(deviceID, loggerFactory: loggerFactory)
                );
            await stmDfuDevice.InitializeAsync();

            await PerformStmDfTest(stmDfuDevice);
        }

        [TestMethod]
        public Task TestWriteAndReadFromTrezorLibUsb()
            => TestWriteAndReadFromTrezor(new FilterDeviceDefinition(vendorId: TrezorVendorId, productId: TrezorOneProductId, label: "Trezor One Firmware 1.7.x")
            .CreateLibUsbDeviceFactory(loggerFactory)
        );
#endif

        [TestMethod]
        public async Task TestSTMDFUModePerformControlTransferSend_LibUsb()
        {
            var stmDfuDevice = await new FilterDeviceDefinition(StmDfuVendorId, StmDfuProductId)
                .GetUsbDeviceFactory(loggerFactory)
                .ConnectFirstAsync();

            await PerformStmDfTest((IUsbDevice)stmDfuDevice);
        }

        [TestMethod]
        public Task TestWriteAndReadFromTrezorUsb() => TestWriteAndReadFromTrezor(
            new FilterDeviceDefinition(vendorId: TrezorVendorId, productId: TrezorOneProductId, label: "Trezor One Firmware 1.7.x")
            .GetUsbDeviceFactory(loggerFactory)
        );

        [TestMethod]
        public Task TestWriteAndReadFromTrezorHid() => TestWriteAndReadFromTrezor(
            new FilterDeviceDefinition(vendorId: 0x534C, productId: 0x0001, label: "Trezor One Firmware 1.6.x", usagePage: 65280)
            .GetHidDeviceFactory(loggerFactory, 0)
            );

        [TestMethod]
        public Task TestWriteAndReadFromKeepKeyUsb() => TestWriteAndReadFromTrezor(
        new FilterDeviceDefinition(vendorId: 0x2B24, productId: 0x2)
            .GetUsbDeviceFactory(loggerFactory)
           );

        [TestMethod]
        public Task TestWriteAndReadFromTrezorModelTUsb() => TestWriteAndReadFromTrezor(
        new FilterDeviceDefinition(vendorId: TrezorVendorId, productId: TrezorOneProductId)
            .GetUsbDeviceFactory(loggerFactory)
            );

        [TestMethod]
        public async Task TestGetAllUsbDevices()
        {
            var devices = await new List<FilterDeviceDefinition> { }
            .GetUsbDeviceFactory(loggerFactory)
            .GetConnectedDeviceDefinitionsAsync();
            Assert.IsTrue(devices.Any());
        }

        [TestMethod]
        public async Task TestGetAllHidDevices()
        {
            var devices = await new List<FilterDeviceDefinition> { }
            .GetHidDeviceFactory(loggerFactory)
            .GetConnectedDeviceDefinitionsAsync();
            Assert.IsTrue(devices.Any());
        }

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
#if WINDOWS_UWP
                filterDeviceDefinition.CreateUwpHidDeviceFactory(loggerFactory));
#else
                filterDeviceDefinition.GetHidDeviceFactory(loggerFactory));
#endif
            await integrationTester.TestAsync(request, (result, device) =>
            {
                Assert.IsTrue(device.IsInitialized);

                var temperatureTimesOneHundred = (result.Data[4] & 0xFF) + (result.Data[3] << 8);
                var temp = Math.Round(temperatureTimesOneHundred / 100.0m, 2, MidpointRounding.ToEven);

                //I think my room should pretty much always be between these temperatures
                Assert.IsTrue(temp is > 10 and < 35);

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
                return Task.FromResult(true);

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

            var integrationTester = new IntegrationTester(filterDeviceDefinition.GetHidDeviceFactory(loggerFactory));

            await integrationTester.TestAsync(request, (result, device) =>
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
                 return Task.FromResult(true);

             }, 64);
        }
        #endregion

        #region Private Methods
        public static async Task PerformStmDfTest(IUsbDevice stmDfuDevice)
        {
            ////////////////////////////////////////////////////////////
            // required to perform a DFU Clear Status request beforehand
            await stmDfuDevice.ClearStatusAsync();

            // this sequence aims to test the "send data to device" using the Control Transfer
            // executes a DFU "Set Address Pointer" command through the DFU DNLOAD request
            ////////////////////////////////////
            // 1st step: send DFU_DNLOAD request
            var dfuRequestResult = await stmDfuDevice.PerformControlTransferWithRetry(ud => ud.SendDownloadRequestAsync());

            // Assert that the bytes transfered match the buffer lenght
            Assert.IsTrue(dfuRequestResult.BytesTransferred == StmDfuExtensions.DownloadRequestLength);

            ///////////////////////////////////////
            // 2nd step: send DFU_GETSTATUS request
            dfuRequestResult = await stmDfuDevice.PerformControlTransferWithRetry(ud => ud.GetStatusAsync());

            // Assert that the received buffer has the requested lenght 
            Assert.IsTrue(dfuRequestResult.BytesTransferred == StmDfuExtensions.GetStatusPacketLength);

            // check DFU status
            Assert.IsTrue(dfuRequestResult.Data[4] != STATE_DFU_IDLE);

            ///////////////////////////////////////////////////////////////
            // 3rd step: send new DFU_GETSTATUS request to check execution
            dfuRequestResult = await stmDfuDevice.PerformControlTransferWithRetry(ud => stmDfuDevice.GetStatusAsync());

            // Assert that the received buffer has the requested lenght 
            Assert.IsTrue(dfuRequestResult.BytesTransferred == StmDfuExtensions.GetStatusPacketLength);

            // check DFU status
            // status has to be different from STATUS_errTARGET
            // state has to be different from STATE_DFU_ERROR
            Assert.IsTrue(
                dfuRequestResult.Data[0] != STATUS_errTARGET &&
                dfuRequestResult.Data[4] != STATE_DFU_ERROR);
        }

        public static Task AssertTrezorResult(TransferResult responseData, IDevice device)
        {
            //Specify the response part of the Message Contract
            var expectedResult = new byte[] { 63, 35, 35 };

            Debug.WriteLine("new byte[] {" + string.Join(", ", responseData.Data) + "}");

            //Assert that the response part meets the specification
            Assert.IsTrue(expectedResult.SequenceEqual(responseData.Data.Take(3)));

            return Task.FromResult(true);
        }
        #endregion
    }
}