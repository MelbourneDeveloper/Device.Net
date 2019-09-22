using Device.Net.LibUsb;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using usbnet = Usb.Net;

namespace Device.Net.IntegrationTests
{
    [TestClass]
    public class TrezorLibUsbIntegrationTests : TrezorUsbTestBase
    {
        #region Helpers
        public override usbnet.IUsbInterfaceManager GetTrezorUsbInterfaceManager(ILogger logger, ITracer tracer)
        {
            var usbDeviceFinder = new UsbDeviceFinder(4617, 21441);
            var usbDevice = UsbDevice.OpenUsbDevice(usbDeviceFinder);
            Assert.IsNotNull(usbDevice);
            return new LibUsbInterfaceManager(usbDevice, 3000, logger, tracer, null, null);
        }
        #endregion
    }
}
