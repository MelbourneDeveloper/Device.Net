using Device.Net.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class UnitTests
    {
        private static readonly Mock<ILogger> logger = new Mock<ILogger>();
        private static readonly MockTracer tracer = new MockTracer();
        private static readonly IDeviceManager _DeviceManager = new DeviceManager();

        #region Tests
        [TestInitialize]
        public void Startup()
        {
            _DeviceManager.RegisterDeviceFactory(new MockHidFactory(logger.Object, tracer));
            _DeviceManager.RegisterDeviceFactory(new MockUsbFactory(logger.Object, tracer));
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
            var connectedDeviceDefinitions = (await _DeviceManager.GetConnectedDeviceDefinitionsAsync(new FilterDeviceDefinition { ProductId = pid, VendorId = vid })).ToList();

            if (connectedDeviceDefinitions.Count > 0)
            {
                foreach (var connectedDeviceDefinition in connectedDeviceDefinitions)
                {
                    var device = _DeviceManager.GetDevice(connectedDeviceDefinition);

                    if (device != null && connectedDeviceDefinition.DeviceType == DeviceType.Hid)
                    {
                        Assert.IsTrue(logger.LogText.Contains(string.Format(MockHidFactory.FoundMessage, connectedDeviceDefinition.DeviceId)));
                    }

                    if (device != null && connectedDeviceDefinition.DeviceType == DeviceType.Usb)
                    {
                        Assert.IsTrue(logger.LogText.Contains(string.Format(MockUsbFactory.FoundMessage, connectedDeviceDefinition.DeviceId)));
                    }
                }
            }

            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(expectedCount, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        [DataRow(true, true, MockHidDevice.VendorId, MockHidDevice.ProductId)]
        [DataRow(true, false, MockHidDevice.VendorId, MockHidDevice.ProductId)]
        public async Task TestWriteAndReadThreadSafety(bool isHidConnected, bool isUsbConnected, uint vid, uint pid)
        {
            var readtraceCount = tracer.ReadCount;
            var writetraceCount = tracer.WriteCount;

            MockHidFactory.IsConnectedStatic = isHidConnected;
            MockUsbFactory.IsConnectedStatic = isUsbConnected;
            var connectedDeviceDefinition = (await _DeviceManager.GetConnectedDeviceDefinitionsAsync(new FilterDeviceDefinition { ProductId = pid, VendorId = vid })).ToList().First();


            var mockHidDevice = new MockHidDevice(connectedDeviceDefinition.DeviceId, logger.Object, tracer);

            var writeAndReadTasks = new List<Task<ReadResult>>();

            //TODO: Does this properly test thread safety?

            const int count = 10;

            for (byte i = 0; i < count; i++)
            {
                writeAndReadTasks.Add(mockHidDevice.WriteAndReadAsync(new byte[64] { i, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
            }

            var results = await Task.WhenAll(writeAndReadTasks);

            for (byte i = 0; i < results.Length; i++)
            {
                var result = results[i];
                Assert.IsTrue(result.Data[0] == i);
            }

            Assert.AreEqual(readtraceCount + count, tracer.ReadCount);
            Assert.AreEqual(writetraceCount + count, tracer.WriteCount);

            Assert.IsTrue(logger.LogText.Contains(Messages.SuccessMessageWriteAndReadCalled));
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
            var connectedDeviceDefinitions = (await _DeviceManager.GetConnectedDeviceDefinitionsAsync(new FilterDeviceDefinition { ProductId = 0, VendorId = 0 })).ToList();
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
            var connectedDeviceDefinitions = (await _DeviceManager.GetConnectedDeviceDefinitionsAsync(null)).ToList();
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
            _DeviceManager.DeviceFactories.Clear();

            try
            {
                await _DeviceManager.GetConnectedDeviceDefinitionsAsync(null);
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
            _DeviceManager.DeviceFactories.Clear();

            try
            {
                var deviceListner = new DeviceListener(_DeviceManager, new List<FilterDeviceDefinition>(), 1000);
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

        #region Exceptions
        [TestMethod]
        public void TestDeviceException()
        {
            try
            {
                var deviceManager = new DeviceManager();
                var device = deviceManager.GetDevice(new ConnectedDeviceDefinition("a"));
            }
            catch (DeviceException dex)
            {
                Assert.AreEqual(Messages.ErrorMessageCouldntGetDevice, dex.Message);
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public async Task TestCancellationException()
        {
            try
            {
                var device = new MockHidDevice("asd", null, null);
                var cancellationTokenSource = new CancellationTokenSource();

                var task1 = device.WriteAndReadAsync(new byte[] { 1, 2, 3 }, cancellationTokenSource.Token);
                var task2 = Task.Run(() => { cancellationTokenSource.Cancel(); });

                await Task.WhenAll(new Task[] { task1, task2 });
            }
            catch (OperationCanceledException oce)
            {
                Assert.AreEqual(Messages.ErrorMessageOperationCanceled, oce.Message);
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }

            Assert.Fail();
        }

        #endregion

        #endregion

        #region Helpers
        private async Task<bool> ListenForDeviceAsync()
        {
            var listenTaskCompletionSource = new TaskCompletionSource<bool>();

            var deviceListener = new DeviceListener(_DeviceManager, new List<FilterDeviceDefinition> { new FilterDeviceDefinition { VendorId = MockHidDevice.VendorId, ProductId = MockHidDevice.ProductId } }, 1000);
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

#if !NET45
        [TestMethod]
        public async Task TestSynchronizeWithCancellationToken()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var completed = false;

            var task = Task.Run(() =>
            {
                //Iterate for one second
                for (var i = 0; i < 100; i++)
                {
                    Thread.Sleep(10);
                }

                return true;
            });

            var cancellationTokenSource = new CancellationTokenSource();

            //Start a task that will cancel in 500 milliseconds
            var cancelTask = Task.Run(() =>
            {
                Thread.Sleep(500);
                cancellationTokenSource.Cancel();
                return true;
            });

            //Get a task that will finish when the cancellation token is cancelled
            var syncTask = task.SynchronizeWithCancellationToken(cancellationToken: cancellationTokenSource.Token);

            //Wait for the first task to finish
            var completedTask = (Task<bool>)await Task.WhenAny(new Task[]
            {
                syncTask,
                cancelTask
            });

            //Ensure the task didn't wait a long time
            Assert.IsTrue(stopWatch.ElapsedMilliseconds < 1000);

            //Ensure the task wasn't completed
            Assert.IsFalse(completed);
        }
#endif
    }
}
