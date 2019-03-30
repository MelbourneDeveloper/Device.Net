using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class Tests
    {
        [TestInitialize]
        public void Startup()
        {
            MockHidFactory.Register(null);
        }

        [TestMethod]
        public async Task GetDevicesDisconnectedWithMatchedFilter()
        {
            MockHidFactory.IsConnected = true;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(new FilterDeviceDefinition { ProductId = MockHidDevice.ProductId, VendorId = MockHidDevice.VendorId })).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(1, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task GetDevicesDisconnectedWithUnmatchedFilter()
        {
            MockHidFactory.IsConnected = false;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(new FilterDeviceDefinition { ProductId = 0, VendorId = 0 })).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(0, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task GetDevicesDisconnectedNullFilter()
        {
            MockHidFactory.IsConnected = false;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null)).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(0, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task GetDevicesConnectedNullFilter()
        {
            MockHidFactory.IsConnected = true;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null)).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(1, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task DeviceListen()
        {
            MockHidFactory.IsConnected = true;

            var listenTaskCompletionSource = new TaskCompletionSource<bool>();

            var deviceListener = new DeviceListener(new List<FilterDeviceDefinition> { new FilterDeviceDefinition { VendorId = MockHidDevice.VendorId, ProductId = MockHidDevice.ProductId } }, 1000);
            deviceListener.DeviceInitialized += (a, b) => { listenTaskCompletionSource.SetResult(true); };

            deviceListener.Start();

            var sw = new Stopwatch();
            sw.Start();

            while (listenTaskCompletionSource.Task.Status != TaskStatus.RanToCompletion)
            {
                await Task.Delay(1000);
                if (sw.Elapsed > new TimeSpan(0, 0, 5))
                {
                    throw new Exception("Timed out");
                }
            }

            await listenTaskCompletionSource.Task;
        }

    }
}
