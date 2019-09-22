using Device.Net.LibUsb;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using usbnet = Usb.Net;

namespace Device.Net.IntegrationTests
{
    [TestClass]
    public class TrezorLibUsbIntegrationTests
    {
        #region Fields
        #endregion

        #region Tests
        [TestMethod]
        public async Task ConnectedTestReadAsync()
        {
            using (var trezorUsbDevice = GetTrezorUsbDevice())
            {
                var logger = new DebugLogger();
                var tracer = new DebugTracer();
                using (var libUsbInterfaceManager = new LibUsbInterfaceManager(trezorUsbDevice, 3000, logger, tracer, null, null))
                {
                    var usbDevice = new usbnet.UsbDevice(trezorUsbDevice.DevicePath, libUsbInterfaceManager, logger, tracer);

                    await libUsbInterfaceManager.InitializeAsync();

                    var writeBuffer = new byte[64];
                    writeBuffer[0] = 0x3f;
                    writeBuffer[1] = 0x23;
                    writeBuffer[2] = 0x23;

                    var readResult = await usbDevice.WriteAndReadAsync(writeBuffer);

                    var expected = new byte[] { 63, 35, 35, 0, 17, 0, 0, 0, 131, 10, 9, 116, 114, 101, 122, 111, 114, 46, 105, 111, 16, 1, 24, 8, 32, 2, 50, 24, 51, 66, 69, 65, 55, 66, 50, 55, 50, 55, 66, 49, 55, 57, 50, 52, 67, 56, 67, 70, 68, 56, 53, 48, 56, 1, 64, 0, 82, 5, 66, 108, 97, 99, 107, 96 };

                    Assert.IsTrue(expected.SequenceEqual(readResult.Data));
                }
            }
        }
        #endregion

        #region Helpers
        private static UsbDevice GetTrezorUsbDevice()
        {
            var usbDeviceFinder = new UsbDeviceFinder(4617, 21441);
            var usbDevice = UsbDevice.OpenUsbDevice(usbDeviceFinder);
            Assert.IsNotNull(usbDevice);
            return usbDevice;
        }
        #endregion
    }
}
