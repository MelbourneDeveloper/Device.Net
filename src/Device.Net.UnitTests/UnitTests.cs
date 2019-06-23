using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class UnitTests
    {
        #region Tests
        [TestInitialize]
        public void Startup()
        {
            //TODO: Test logging and tracing
            MockHidFactory.Register(null, null);
            MockUsbFactory.Register(null, null);
        }

        [TestMethod]
        [DataRow(true, true, 1, MockHidDevice.VendorId, MockHidDevice.ProductId)]
        [DataRow(true, false, 1, MockHidDevice.VendorId, MockHidDevice.ProductId)]
        [DataRow(false, true, 0, MockHidDevice.VendorId, MockHidDevice.ProductId)]
        [DataRow(false, false, 0, MockHidDevice.VendorId, MockHidDevice.ProductId)]

        [DataRow(true, true, 1, MockUsbDevice.VendorId, MockUsbDevice.ProductId)]
        [DataRow(true, false, 0, MockUsbDevice.VendorId, MockUsbDevice.ProductId)]
        [DataRow(false, true, 1, MockUsbDevice.VendorId, MockUsbDevice.ProductId)]
        [DataRow(false, false, 0, MockUsbDevice.VendorId, MockUsbDevice.ProductId)]
        public async Task TestWithMatchedFilterAsync(bool isHidConnected, bool isUsbConnected, int expectedCount, uint vid, uint pid)
        {
            MockHidFactory.IsConnectedStatic = isHidConnected;
            MockUsbFactory.IsConnectedStatic = isUsbConnected;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(new FilterDeviceDefinition { ProductId = pid, VendorId = vid })).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(expectedCount, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        [DataRow(true, true, MockHidDevice.VendorId, MockHidDevice.ProductId)]
        [DataRow(true, false, MockHidDevice.VendorId, MockHidDevice.ProductId)]
        public async Task TestWriteAndReadThreadSafety(bool isHidConnected, bool isUsbConnected, uint vid, uint pid)
        {
            MockHidFactory.IsConnectedStatic = isHidConnected;
            MockUsbFactory.IsConnectedStatic = isUsbConnected;
            var connectedDeviceDefinition = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(new FilterDeviceDefinition { ProductId = pid, VendorId = vid })).ToList().First();
            //TODO: Test logging and tracing
            var mockHidDevice = new MockHidDevice(new DebugLogger(), new DebugTracer()) { DeviceId = connectedDeviceDefinition.DeviceId };

            var writeAndReadTasks = new List<Task<byte[]>>();

            //TODO: Does this properly test thread safety?

            for (byte i = 0; i < 10; i++)
            {
                writeAndReadTasks.Add(mockHidDevice.WriteAndReadAsync(new byte[64] { i, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
            }

            var results = await Task.WhenAll(writeAndReadTasks);

            for (byte i = 0; i < results.Length; i++)
            {
                var result = results[i];
                Assert.IsTrue(result[0] == i);
            }
        }

        [TestMethod]
        [DataRow(true, true, 0)]
        [DataRow(true, false, 0)]
        [DataRow(false, true, 0)]
        [DataRow(false, false, 0)]
        public async Task TestWithUnmatchedFilterAsync(bool isHidConnected, bool isUsbConnected, int expectedCount)
        {
            MockHidFactory.IsConnectedStatic = isHidConnected;
            MockUsbFactory.IsConnectedStatic = isUsbConnected;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(new FilterDeviceDefinition { ProductId = 0, VendorId = 0 })).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(expectedCount, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        [DataRow(true, true, 2)]
        [DataRow(true, false, 1)]
        [DataRow(false, false, 0)]
        public async Task TestNullFilterAsync(bool isHidConnected, bool isUsbConnected, int expectedCount)
        {
            MockHidFactory.IsConnectedStatic = isHidConnected;
            MockUsbFactory.IsConnectedStatic = isUsbConnected;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null)).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(expectedCount, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task TestDeviceListenerAsync()
        {
            MockHidFactory.IsConnectedStatic = true;
            MockUsbFactory.IsConnectedStatic = true;
            var isTimeout = await ListenForDeviceAsync();
            Assert.IsTrue(!isTimeout, "Timeout");
        }

        //Is this a bug?
        [TestMethod]
        public async Task TestDeviceListenerTimeoutAsync()
        {
            MockHidFactory.IsConnectedStatic = false;
            MockUsbFactory.IsConnectedStatic = false;
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
            deviceListener.DeviceInitialized += (a, deviceEventArgs) =>
            {
                Console.WriteLine($"{deviceEventArgs.Device?.DeviceId} connected");
                listenTaskCompletionSource.SetResult(true);
            };
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
