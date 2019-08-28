using Device.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerialPort.Net.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SerialPort.Net
{
    [TestClass]
    public class IntegrationTests
    {
        #region Fields
        private static WindowsSerialPortDeviceFactory windowsSerialPortDeviceFactory;
        #endregion

        #region Tests
        [TestMethod]
        public async Task ConnectedTestReadAsync()
        {
            await ReadAsync();
        }

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
            var connectedDeviceDefinitions = await GetConnectedDevicesAsync();
        }

        [TestMethod]
        public async Task ConnectedTestEnumerateAndConnectAsync()
        {
            var connectedDeviceDefinitions = await GetConnectedDevicesAsync();
            Assert.IsTrue(connectedDeviceDefinitions.Count > 1);
            using (var serialPortDevice = windowsSerialPortDeviceFactory.GetDevice(connectedDeviceDefinitions[1]))
            {
                await serialPortDevice.InitializeAsync();
                Assert.IsTrue(serialPortDevice.IsInitialized);
            }
        }

        [TestMethod]
        public async Task NotConnectedTestEnumerateAsync()
        {
            var connectedDeviceDefinitions = await GetConnectedDevicesAsync();
            Assert.IsTrue(connectedDeviceDefinitions.Count == 1);
        }
        #endregion

        #region Helpers
        private static async Task<List<ConnectedDeviceDefinition>> GetConnectedDevicesAsync()
        {
            if (windowsSerialPortDeviceFactory == null)
            {
                windowsSerialPortDeviceFactory = new WindowsSerialPortDeviceFactory();
            }

            return (await windowsSerialPortDeviceFactory.GetConnectedDeviceDefinitionsAsync(null)).ToList();
        }

        private static async Task ReadAsync()
        {
            using (var serialPortDevice = new WindowsSerialPortDevice(@"\\.\COM3"))
            {
                await serialPortDevice.InitializeAsync();
                var result = await serialPortDevice.ReadAsync();
                Assert.IsTrue(result.Data.Length > 0);
                var range = result.Data.ToList().GetRange(0, 10);
                Assert.IsFalse(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }.SequenceEqual(range));
            }
        }
        #endregion
    }
}
