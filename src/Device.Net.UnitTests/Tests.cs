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
        #region Tests
        [TestInitialize]
        public void Startup()
        {
            MockHidFactory.Register(null);
            MockUsbFactory.Register(null);
        }

        [TestMethod]
        public async Task TestGetDevicesDisconnectedWithMatchedFilterAsync()
        {
            MockHidFactory.IsConnectedStatic = true;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(new FilterDeviceDefinition { ProductId = MockHidDevice.ProductId, VendorId = MockHidDevice.VendorId })).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(1, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task TestGetDevicesDisconnectedWithUnmatchedFilterAsync()
        {
            MockHidFactory.IsConnectedStatic = false;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(new FilterDeviceDefinition { ProductId = 0, VendorId = 0 })).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(0, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task TestGetDevicesDisconnectedNullFilterAsync()
        {
            MockHidFactory.IsConnectedStatic = false;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null)).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(0, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task TestGetDevicesConnectedNullFilterAsync()
        {
            MockHidFactory.IsConnectedStatic = true;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null)).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(1, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task TestDeviceListenerAsync()
        {
            MockHidFactory.IsConnectedStatic = true;
            var isTimeout = await ListenForDeviceAsync();
            Assert.IsTrue(!isTimeout, "Timeout");
        }

        [TestMethod]
        public async Task TestDeviceListenerTimeoutAsync()
        {
            MockHidFactory.IsConnectedStatic = false;
            var isTimeout = await ListenForDeviceAsync();
            Assert.IsTrue(isTimeout, "Device is connected");
        }


        [TestMethod]
        public async Task TestDeviceFactoriesNotRegisteredException()
        {
            DeviceManager.Current.DeviceFactories.Clear();

            try
            {
                var connectedDevices = await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null);
            }
            catch (DeviceFactoriesNotRegisteredException)
            {
                return;
            }
            finally
            {
                Startup();
            }

            throw new Exception("The call was not stopped");
        }

        [TestMethod]
        public void TestListenerDeviceFactoriesNotRegisteredException()
        {
            DeviceManager.Current.DeviceFactories.Clear();

            try
            {
                var deviceListner = new DeviceListener(new List<FilterDeviceDefinition>(), 1000);
                deviceListner.Start();
            }
            catch (DeviceFactoriesNotRegisteredException)
            {
                return;
            }
            finally
            {
                Startup();
            }

            throw new Exception("The call was not stopped");
        }

        #endregion

        #region Helpers
        private async Task<bool> ListenForDeviceAsync()
        {
            var listenTaskCompletionSource = new TaskCompletionSource<bool>();

            var deviceListener = new DeviceListener(new List<FilterDeviceDefinition> { new FilterDeviceDefinition { VendorId = MockHidDevice.VendorId, ProductId = MockHidDevice.ProductId } }, 1000);
            deviceListener.DeviceInitialized += (a, b) => { listenTaskCompletionSource.SetResult(true); };
            deviceListener.Start();

            var listenTask = listenTaskCompletionSource.Task;
            var timeoutTask = SimulateTimeoutAsync(listenTask, 3);

            var completedTask = await Task.WhenAny(new List<Task> { listenTask, timeoutTask });
            return ReferenceEquals(completedTask, timeoutTask);
        }

        private static async Task SimulateTimeoutAsync(Task<bool> task, int seconds)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            while (task.Status != TaskStatus.RanToCompletion)
            {
                if (stopWatch.Elapsed > new TimeSpan(0, 0, seconds))
                {
                    Console.WriteLine("Timeout occurred");
                    throw new Exception("Timed out");
                }

                await Task.Delay(500);
            }
        }
        #endregion
    }
}
