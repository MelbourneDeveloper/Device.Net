#if !NET45

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerialPort.Net.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class IntegrationTestsSerialPort
    {
        #region Fields
        private static WindowsSerialPortDeviceFactory windowsSerialPortDeviceFactory;
        #endregion

        #region Tests
        [TestMethod]
        public async Task ConnectedTestReadAsync() => await ReadAsync();

        [TestMethod]
        public async Task NotConnectedTestReadAsync()
        {
            try
            {
                await ReadAsync();
            }
            catch
            {
                //TODO: More specific exception with details of whether the device was initialized etc.
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public async Task ConnectedTestEnumerateAsync()
        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var connectedDeviceDefinitions = await GetConnectedDevicesAsync();
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        }

        [TestMethod]
        public async Task ConnectedTestEnumerateAndConnectAsync()
        {
            var connectedDeviceDefinitions = await GetConnectedDevicesAsync();
            Assert.IsTrue(connectedDeviceDefinitions.Count > 0);
            using var serialPortDevice = await windowsSerialPortDeviceFactory.GetDevice(connectedDeviceDefinitions[0]);
            await serialPortDevice.InitializeAsync();
            Assert.IsTrue(serialPortDevice.IsInitialized);
        }

        [TestMethod]
        public async Task ConnectedTestGetDevicesAsync()
        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var connectedDeviceDefinitions = await GetConnectedDevicesAsync();
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            var deviceManager = windowsSerialPortDeviceFactory.ToDeviceManager();
            var devices = await deviceManager.GetConnectedDeviceDefinitionsAsync();

            foreach (var device in devices)
            {
                Assert.AreEqual(DeviceType.SerialPort, device.DeviceType);
            }

            Assert.IsTrue(devices.Count() > 0);
        }

        [TestMethod]
        public async Task ConnectedTestGetDevicesSingletonAsync()
        {
            var deviceManager = new WindowsSerialPortDeviceFactory().ToDeviceManager();

            var devices = await deviceManager.GetConnectedDeviceDefinitionsAsync();

            Assert.IsTrue(devices.Count() > 0);
        }

        [TestMethod]
        public async Task NotConnectedTestEnumerateAsync()
        {
            var connectedDeviceDefinitions = await GetConnectedDevicesAsync();
            Assert.IsTrue(connectedDeviceDefinitions.Count == 1);
        }
        #endregion

        #region Helpers
        private async Task<List<ConnectedDeviceDefinition>> GetConnectedDevicesAsync()
        {
            if (windowsSerialPortDeviceFactory == null)
            {
                windowsSerialPortDeviceFactory = new WindowsSerialPortDeviceFactory();
            }

            return (await windowsSerialPortDeviceFactory.GetConnectedDeviceDefinitionsAsync()).ToList();
        }

        private static async Task ReadAsync()
        {
            using var serialPortDevice = new WindowsSerialPortDevice(@"\\.\COM1");
            await serialPortDevice.InitializeAsync();
            var result = await serialPortDevice.ReadAsync();
            Assert.IsTrue(result.Data.Length > 0);
            var range = result.Data.ToList().GetRange(0, 10);
            Assert.IsFalse(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }.SequenceEqual(range));
        }
        #endregion
    }
}

#endif