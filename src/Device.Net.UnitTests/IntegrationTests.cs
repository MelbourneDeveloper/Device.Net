#if !NET45

using Hid.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Usb.Net.Windows;

namespace Device.Net.UnitTests
{
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
        [TestMethod]
        public async Task TestWriteAndReadFromTrezorUsb() => TestWriteAndReadFromTrezor(
            new FilterDeviceDefinition { DeviceType = DeviceType.Usb, VendorId = 0x1209, ProductId = 0x53C1, Label = "Trezor One Firmware 1.7.x" },
            new WindowsUsbDeviceFactory(_loggerFactory)
        );

        [TestMethod]
        public async Task TestWriteAndReadFromTrezorHid() => TestWriteAndReadFromTrezor(
            new FilterDeviceDefinition { DeviceType = DeviceType.Hid, VendorId = 0x534C, ProductId = 0x0001, Label = "Trezor One Firmware 1.6.x", UsagePage = 65280 },
            new WindowsHidDeviceFactory(_loggerFactory)
            );

        private async Task TestWriteAndReadFromTrezor(FilterDeviceDefinition filterDeviceDefinition, IDeviceFactory deviceFactory)
        {
            //Send the request part of the Message Contract
            var request = new byte[64];
            request[0] = 0x3f;
            request[1] = 0x23;
            request[2] = 0x23;

            var integrationTester = new IntegrationTester(
                filterDeviceDefinition, deviceFactory, _loggerFactory);
            await integrationTester.TestAsync(request, AssertTrezorResult);
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

            var integrationTester = new IntegrationTester(
                new FilterDeviceDefinition
                {
                    ProductId = 4112,
                    VendorId = 10741
                }, new WindowsHidDeviceFactory(_loggerFactory), _loggerFactory);
            await integrationTester.TestAsync(request, async (a) =>
             {
                 Assert.AreEqual(64, a.Length);
                 Assert.AreEqual(63, a[0]);
                 Assert.AreEqual(62, a[1]);
             });
        }
        #endregion

        #region Private Methods
        private static Task AssertTrezorResult(byte[] responseData)
        {
            //Specify the response part of the Message Contract
            var expectedResult = new byte[] { 63, 35, 35, 0, 17, 0, 0, 0, 194, 10, 9, 116, 114, 101, 122, 111, 114, 46, 105, 111, 16, 1, 24, 9, 32, 1, 50, 24, 51, 66, 69, 65, 55, 66, 50, 55, 50, 55, 66, 49, 55, 57, 50, 52, 67, 56, 67, 70, 68, 56, 53, 48, 56, 1, 64, 0, 74, 5, 101, 110, 45, 85, 83, 82 };

            //Assert that the response part meets the specification
            Assert.IsTrue(expectedResult.SequenceEqual(responseData));

            return Task.FromResult(true);
        }
        #endregion
    }
}

#endif
