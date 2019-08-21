using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Threading.Tasks;
using Usb.Net;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class UsbTests
    {
        #region Fields
        private UsbDevice _UsbDevice;
        private const string deviceId = "test";
        private readonly byte[] testreadpacket = { 1, 2, 3 };
        #endregion

        #region Tests
        [TestMethod]
        public async Task TestUsbDeviceInitialization()
        {
            await InitializeDevice();

            Assert.IsNotNull(_UsbDevice.ConnectedDeviceDefinition);
            Assert.AreEqual(deviceId, ((ConnectedDeviceDefinition)_UsbDevice.ConnectedDeviceDefinition).DeviceId);
        }

        [TestMethod]
        public async Task TestRead()
        {
            await InitializeDevice();
            var readResult = await _UsbDevice.ReadAsync();
            Assert.AreEqual(readResult.Data, testreadpacket);
        }

        [TestMethod]
        public async Task TestWrite()
        {
            await InitializeDevice();
            await _UsbDevice.WriteAsync(testreadpacket);
            await _UsbDevice.UsbInterfaceManager.WriteUsbInterface.Received().WriteAsync(testreadpacket);
        }
        #endregion

        #region Helpers
        private async Task InitializeDevice()
        {
            if (_UsbDevice != null) return;

            var usbInterfaceManager = Substitute.For<IUsbInterfaceManager>();

            var usbInterface = Substitute.For<IUsbInterface>();
            var endpoint = Substitute.For<IUsbInterfaceEndpoint>();
            usbInterface.UsbInterfaceEndpoints.Add(endpoint);
            usbInterfaceManager.ReadUsbInterface = usbInterface;
            usbInterfaceManager.WriteUsbInterface = usbInterface;

            usbInterface.ReadAsync(3).ReturnsForAnyArgs(testreadpacket);

            //TODO: Probably shouldn't be relying on this method
            usbInterfaceManager.GetConnectedDeviceDefinitionAsync().ReturnsForAnyArgs(new ConnectedDeviceDefinition(deviceId) { });

            _UsbDevice = new UsbDevice(deviceId, usbInterfaceManager, Substitute.For<ILogger>(), Substitute.For<ITracer>());

            await _UsbDevice.InitializeAsync();
        }
        #endregion
    }
}
