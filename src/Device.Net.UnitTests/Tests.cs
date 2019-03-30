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
        public async Task TestGetDevicesDisconnectedWithMatchedFilter()
        {
            MockHidFactory.IsConnected = true;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(new FilterDeviceDefinition { ProductId = MockHidDevice.ProductId, VendorId = MockHidDevice.VendorId })).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(1, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task TestGetDevicesDisconnectedWithUnmatchedFilter()
        {
            MockHidFactory.IsConnected = false;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(new FilterDeviceDefinition { ProductId = 0, VendorId = 0 })).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(0, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task TestGetDevicesDisconnectedNullFilter()
        {
            MockHidFactory.IsConnected = false;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null)).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(0, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task TestGetDevicesConnectedNullFilter()
        {
            MockHidFactory.IsConnected = true;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null)).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(1, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task TestDeviceListener()
        {
            MockHidFactory.IsConnected = true;

            var listenTaskCompletionSource = new TaskCompletionSource<bool>();

            var deviceListener = new DeviceListener(new List<FilterDeviceDefinition> { new FilterDeviceDefinition { VendorId = MockHidDevice.VendorId, ProductId = MockHidDevice.ProductId } }, 1000);
            deviceListener.DeviceInitialized += (a, b) => { listenTaskCompletionSource.SetResult(true); };
            deviceListener.Start();

            var listenTask = listenTaskCompletionSource.Task;

            await SimulateTimeoutAsync(listenTask, 5);

            await listenTask;
        }

        private static async Task SimulateTimeoutAsync(Task<bool> task, int seconds)
        {
            var sw = new Stopwatch();
            sw.Start();

            while (task.Status != TaskStatus.RanToCompletion)
            {
                if (sw.Elapsed > new TimeSpan(0, 0, seconds))
                {
                    throw new Exception("Timed out");
                }

                await Task.Delay(1000);
            }
        }
    }
}
