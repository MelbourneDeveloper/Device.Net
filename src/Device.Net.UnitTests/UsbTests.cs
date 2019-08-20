using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Threading.Tasks;
using Usb.Net;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class UsbTests
    {
        [TestMethod]
        public async Task TestUsbDeviceInitialization()
        {
            const string deviceId = "test";
            var usbInterfaceManager = Substitute.For<IUsbInterfaceManager>();

            //TODO: Probably shouldn't be relying on this method
            usbInterfaceManager.GetConnectedDeviceDefinitionAsync().ReturnsForAnyArgs(new ConnectedDeviceDefinition(deviceId) { });

            var usbDevice = new UsbDevice(deviceId, usbInterfaceManager, Substitute.For<ILogger>(), Substitute.For<ITracer>());

            await usbDevice.InitializeAsync();

            Assert.IsNotNull(usbDevice.ConnectedDeviceDefinition);
            Assert.AreEqual(deviceId, ((ConnectedDeviceDefinition)usbDevice.ConnectedDeviceDefinition).DeviceId);
        }
    }
}
