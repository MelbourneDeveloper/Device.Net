#if !NET45

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

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
    public static class GetFactoryExtensions
    {
        public static IDeviceFactory GetUsbDeviceFactory(this FilterDeviceDefinition filterDeviceDefinition, ILoggerFactory loggerFactory) =>
#if !WINDOWS_UWP
            filterDeviceDefinition.CreateWindowsUsbDeviceFactory(loggerFactory);
#else
            filterDeviceDefinition.CreateUwpUsbDeviceFactory(loggerFactory);
#endif


        public static IDeviceFactory GetHidDeviceFactory(this FilterDeviceDefinition filterDeviceDefinition, ILoggerFactory loggerFactory, byte? defultReportId = null) =>
#if !WINDOWS_UWP
            filterDeviceDefinition.CreateWindowsHidDeviceFactory(loggerFactory, defaultReportId: defultReportId);
#else
            filterDeviceDefinition.CreateUwpHidDeviceFactory(loggerFactory, defaultReportId: defultReportId);
#endif
    }

    [TestClass]
    public class IntegrationTests

    {
        #region Fields
        private ILoggerFactory _loggerFactory;
        #endregion

        #region Setup
        [TestInitialize]
        public void Setup()
        {
            //Note: creating a LoggerFactory like this is easier than creating a mock
            _loggerFactory = LoggerFactory.Create((builder) =>
            {
                _ = builder.AddDebug().SetMinimumLevel(LogLevel.Trace);
            });
        }
        #endregion

        #region Tests
#if !WINDOWS_UWP
        [TestMethod]
        public async Task TestConnectToSTMDFUMode()
        {
            const string deviceID = @"USB\VID_0483&PID_DF11\00000008FFFF";
            var windowsUsbDevice = new UsbDevice(deviceID, new WindowsUsbInterfaceManager(deviceID, _loggerFactory, null, null), _loggerFactory);
            await windowsUsbDevice.InitializeAsync();
        }
#endif

        [TestMethod]
        public Task TestWriteAndReadFromTrezorUsb() => TestWriteAndReadFromTrezor(
            new FilterDeviceDefinition { VendorId = 0x1209, ProductId = 0x53C1, Label = "Trezor One Firmware 1.7.x" }
            .GetUsbDeviceFactory(_loggerFactory)
        );

        [TestMethod]
        public Task TestWriteAndReadFromTrezorHid() => TestWriteAndReadFromTrezor(
            new FilterDeviceDefinition { VendorId = 0x534C, ProductId = 0x0001, Label = "Trezor One Firmware 1.6.x", UsagePage = 65280 }
            .GetHidDeviceFactory(_loggerFactory, 0)
            );

        [TestMethod]
        public Task TestWriteAndReadFromKeepKeyUsb() => TestWriteAndReadFromTrezor(
        new FilterDeviceDefinition { VendorId = 0x2B24, ProductId = 0x2 }
            .GetUsbDeviceFactory(_loggerFactory)
           );

        [TestMethod]
        public Task TestWriteAndReadFromTrezorModelTUsb() => TestWriteAndReadFromTrezor(
        new FilterDeviceDefinition { VendorId = 0x1209, ProductId = 0x53c1 }
            .GetUsbDeviceFactory(_loggerFactory)
            );

        private async Task TestWriteAndReadFromTrezor(IDeviceFactory deviceFactory, int expectedDataLength = 64)
        {
            //Send the request part of the Message Contract
            var request = new byte[64];
            request[0] = 0x3f;
            request[1] = 0x23;
            request[2] = 0x23;

            var integrationTester = new IntegrationTester(
                deviceFactory, _loggerFactory);
            await integrationTester.TestAsync(request, AssertTrezorResult, expectedDataLength);
        }

        [TestMethod]
        public async Task TestWriteAndReadFromTemperHid()
        {
            //Send the request part of the Message Contract
            var request = new byte[9] { 0x00, 0x01, 0x80, 0x33, 0x01, 0x00, 0x00, 0x00, 0x00 };

            var filterDeviceDefinition = new FilterDeviceDefinition { VendorId = 0x413d, ProductId = 0x2107, UsagePage = 65280 };

            var integrationTester = new IntegrationTester(
                filterDeviceDefinition.GetHidDeviceFactory(_loggerFactory), _loggerFactory);
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

            var filterDeviceDefinition = new FilterDeviceDefinition
            {
                ProductId = 4112,
                VendorId = 10741
            };

            var integrationTester = new IntegrationTester(
                filterDeviceDefinition.GetHidDeviceFactory(_loggerFactory), _loggerFactory);
            await integrationTester.TestAsync(request, async (result, device) =>
             {
                 Assert.AreEqual(64, result.Data.Length);
                 Assert.AreEqual(63, result.Data[0]);
                 Assert.AreEqual(62, result.Data[1]);

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

#if WINDOWS_UWP
                 var windowsHidDevice = (UWPHidDevice)device;
#else
                 var windowsHidDevice = (WindowsHidDevice)device;
#endif
                 Assert.AreEqual(64, windowsHidDevice.ReadBufferSize);
                 Assert.AreEqual(64, windowsHidDevice.WriteBufferSize);
             }, 64);
        }
        #endregion

        #region Private Methods
        private static Task AssertTrezorResult(ReadResult responseData, IDevice device)
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
