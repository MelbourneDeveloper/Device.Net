using Device.Net.LibUsb;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

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
            var usbDeviceFinder = new UsbDeviceFinder(4617, 21441);
            using (var usbDevice = UsbDevice.OpenUsbDevice(usbDeviceFinder))
            {
                Assert.IsNotNull(usbDevice);

                using (var libUsbInterfaceManager = new LibUsbInterfaceManager(usbDevice, 3000, new DebugLogger(), new DebugTracer(), null, null))
                {
                    await libUsbInterfaceManager.InitializeAsync();
                }
            }
        }
        #endregion

        #region Helpers
        #endregion
    }
}
