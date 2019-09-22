using LibUsbDotNet;
using LibUsbDotNet.Main;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Device.Net.IntegrationTests
{
    [TestClass]
    public class STM32BootloaderTests
    {
        [TestMethod]
        public void Test()
        {
            //USB\VID_0483&PID_DF11&REV_2200
            var usbDeviceFinder = new UsbDeviceFinder(0x0483, 0xDF11);
            var usbDevice = UsbDevice.OpenUsbDevice(usbDeviceFinder);
            Assert.IsNotNull(usbDevice);
        }
    }
}
