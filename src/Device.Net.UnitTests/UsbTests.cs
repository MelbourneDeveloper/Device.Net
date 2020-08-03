
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Threading.Tasks;
using Usb.Net;
using Moq;
using Microsoft.Extensions.Logging;
#if !WINDOWS_UWP
using Usb.Net.Windows;
using Device.Net.Exceptions;
#endif

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

#if !WINDOWS_UWP
        [TestMethod]
        public async Task TestDeviceIdIsPersisted()
        {
            var deviceId = "asd";
            var windowsUsbDevice = new WindowsUsbDevice(deviceId, new Mock<ILoggerFactory>().Object, new DebugTracer(), 80, 80);
            Assert.AreEqual(deviceId, windowsUsbDevice.DeviceId);
        }
#endif

        [TestMethod]
        public async Task TestWrite()
        {
            await InitializeDevice();
            await _UsbDevice.WriteAsync(testreadpacket);
            await _UsbDevice.UsbInterfaceManager.WriteUsbInterface.Received().WriteAsync(testreadpacket);
        }


#if !WINDOWS_UWP
        [TestMethod]
        public void TestValidationExceptionInvalidWriteInterface()
        {
            try
            {
                var logger = Substitute.For<ILogger>();
                var tracer = Substitute.For<ITracer>();
                const string deviceId = "";
                var usbInterfaceManager = new WindowsUsbInterfaceManager(deviceId, logger, tracer, null, null);
                var usbDevice = new UsbDevice(deviceId, usbInterfaceManager, logger, tracer);
                usbDevice.UsbInterfaceManager.WriteUsbInterface = new WindowsUsbInterface(null, logger, tracer, 0, null, null);
            }
            catch (ValidationException vex)
            {
                Assert.AreEqual(Messages.ErrorMessageInvalidInterface, vex.Message);
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void TestValidationExceptionInvalidReadInterface()
        {
            try
            {
                var logger = Substitute.For<ILogger>();
                var tracer = Substitute.For<ITracer>();
                const string deviceId = "";
                var usbInterfaceManager = new WindowsUsbInterfaceManager(deviceId, logger, tracer, null, null);
                var usbDevice = new UsbDevice(deviceId, usbInterfaceManager, logger, tracer);
                usbDevice.UsbInterfaceManager.ReadUsbInterface = new WindowsUsbInterface(null, logger, tracer, 0, null, null);
            }
            catch (ValidationException vex)
            {
                Assert.AreEqual(Messages.ErrorMessageInvalidInterface, vex.Message);
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void TestValidationExceptionInvalidWriteEndpoint()
        {
            try
            {
                var usbDevice = CreateUsbDeviceWithInterface();
                usbDevice.UsbInterfaceManager.UsbInterfaces[0].WriteEndpoint = Substitute.For<IUsbInterfaceEndpoint>();
            }
            catch (ValidationException vex)
            {
                Assert.AreEqual(Messages.ErrorMessageInvalidEndpoint, vex.Message);
                return;
            }

            Assert.Fail();
        }
#endif
        #endregion

        #region Helpers
#if !WINDOWS_UWP
        private static UsbDevice CreateUsbDeviceWithInterface()
        {
            var logger = Substitute.For<ILogger>();
            var tracer = Substitute.For<ITracer>();
            const string deviceId = "";
            var usbInterfaceManager = new WindowsUsbInterfaceManager(deviceId, logger, tracer, null, null);
            var usbDevice = new UsbDevice(deviceId, usbInterfaceManager, logger, tracer);
            var windowsUsbInterface = new WindowsUsbInterface(null, logger, tracer, 0, null, null);
            usbDevice.UsbInterfaceManager.UsbInterfaces.Add(windowsUsbInterface);
            return usbDevice;
        }
#endif

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
