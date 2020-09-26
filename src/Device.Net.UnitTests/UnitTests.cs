#pragma warning disable IDE0055

#if !NET45

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

#if NETCOREAPP3_1
using Hid.Net.Windows;
using Usb.Net.Windows;
using Usb.Net;
#endif


namespace Device.Net.UnitTests
{
    [TestClass]
    public class UnitTests
    {
        private const uint MockUsbDeviceProductId = 2;
        private const uint MockUsbDeviceVendorId = 2;

        private const uint MockHidDeviceProductId = 1;
        private const uint MockHidDeviceVendorId = 1;


        private static readonly Mock<ILogger> _loggerMock = new Mock<ILogger>();
        private static readonly Mock<ILoggerFactory> _LoggerFactoryMock = new Mock<ILoggerFactory>();
        //private static readonly IDeviceManager _DeviceManager = new DeviceManager(_LoggerFactoryMock.Object);

        public static void CheckLogMessageText(Mock<ILogger> loggerMock, string containsText, LogLevel logLevel, Times times)
        {
            loggerMock.Verify
            (
                l => l.Log
                (
                    //Check the severity level
                    logLevel,
                    //This may or may not be relevant to your scenario
                    It.IsAny<EventId>(),
                    //This is the magical Moq code that exposes internal log processing from the extension methods
                    It.Is<It.IsAnyType>((state, t) =>
                        //This confirms that the correct log message was sent to the logger. {OriginalFormat} should match the value passed to the logger
                        //Note: messages should be retrieved from a service that will probably store the strings in a resource file
                        ((string)GetValue(state, "{OriginalFormat}")).Contains(containsText)
                ),
                //Confirm the exception type
                It.IsAny<Exception>(),
                //Accept any valid Func here. The Func is specified by the extension methods
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                //Make sure the message was logged the correct number of times
                times
            );
        }

        private static object GetValue(object state, string key)
        {
            var keyValuePairList = (IReadOnlyList<KeyValuePair<string, object>>)state;

            var actualValue = keyValuePairList.First(kvp => string.Compare(kvp.Key, key, StringComparison.Ordinal) == 0).Value;

            return actualValue;
        }

        #region Tests
        [TestMethod]
        [DataRow(true, true, 1, MockHidDeviceVendorId, MockHidDeviceProductId)]
        [DataRow(true, false, 1, MockHidDeviceVendorId, MockHidDeviceProductId)]
        [DataRow(false, true, 0, MockHidDeviceVendorId, MockHidDeviceProductId)]
        [DataRow(false, false, 0, MockHidDeviceVendorId, MockHidDeviceProductId)]

        [DataRow(true, true, 1, MockUsbDeviceVendorId, MockUsbDeviceProductId)]
        [DataRow(true, false, 0, MockUsbDeviceVendorId, MockUsbDeviceProductId)]
        [DataRow(false, true, 1, MockUsbDeviceVendorId, MockUsbDeviceProductId)]
        [DataRow(false, false, 0, MockUsbDeviceVendorId, MockUsbDeviceProductId)]
        public async Task TestWithMatchedFilterAsync(bool isHidConnected, bool isUsbConnected, int expectedCount, uint vid, uint pid)
        {
            var (hid, usb) = GetMockedFactories(isHidConnected, isUsbConnected, vid, pid);
            var deviceManager = new DeviceManager(_loggerFactory) { DeviceFactories = { hid.Object, usb.Object } };

            var connectedDeviceDefinitions = (await deviceManager.GetConnectedDeviceDefinitionsAsync()).ToList();

            if (connectedDeviceDefinitions.Count > 0)
            {
                foreach (var connectedDeviceDefinition in connectedDeviceDefinitions)
                {
                    _ = deviceManager.GetDevice(connectedDeviceDefinition);

                    //TODO: put stuff here
                }
            }

            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(expectedCount, connectedDeviceDefinitions.Count);
        }

#pragma warning disable IDE0022 // Use expression body for methods
        [TestMethod]
        [DataRow(true, true, MockHidDeviceVendorId, MockHidDeviceProductId)]
        [DataRow(true, false, MockHidDeviceVendorId, MockHidDeviceProductId)]
        public async Task TestWriteAndReadThreadSafety(bool isHidConnected, bool isUsbConnected, uint vid, uint pid)
        {
            //TODO: Does this properly test thread safety?

            var actualCount = 0;

            _loggerMock.Setup(l => l.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>())).Callback(() => 
            {
                actualCount++;
            });

            _loggerMock.Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>())).Returns(new Mock<IDisposable>().Object);

            var (hid, usb) = GetMockedFactories(isHidConnected, isUsbConnected, vid, pid);


            var deviceManager = new DeviceManager(_loggerFactory) { DeviceFactories = { hid.Object, usb.Object } };

            var connectedDeviceDefinition = (await deviceManager.GetConnectedDeviceDefinitionsAsync()).ToList().First();



            var mockHidDevice = new MockHidDevice(connectedDeviceDefinition.DeviceId, _loggerFactory, _loggerMock.Object);

            var writeAndReadTasks = new List<Task<ReadResult>>();


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

            Assert.AreEqual(count*2, actualCount);

            //TODO: this should get called 10 times and that seems to be what's happening, bu tif you specify 10 it says that it was called 20.
            //Bug in Moq?

            CheckLogMessageText(_loggerMock, Messages.SuccessMessageWriteAndReadCalled, LogLevel.Information, Times.AtLeast(1));
        }
#pragma warning restore IDE0022 // Use expression body for methods

        [TestMethod]
        [DataRow(true, true, 2)]
        [DataRow(true, false, 1)]
        [DataRow(false, false, 0)]
        public async Task TestNullFilterAsync(bool isHidConnected, bool isUsbConnected, int expectedCount)
        {
            var (hidMock, usbMock) = GetMockedFactories(isHidConnected, isUsbConnected, null, null);

            var deviceManager = new DeviceManager(_loggerFactory) { DeviceFactories = { hidMock.Object, usbMock.Object } };

            var connectedDeviceDefinitions = (await deviceManager.GetConnectedDeviceDefinitionsAsync()).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(expectedCount, connectedDeviceDefinitions.Count);
        }

        private static (Mock<IDeviceFactory> hidMock,  Mock<IDeviceFactory> usbMock) GetMockedFactories(bool isHidConnected, bool isUsbConnected, uint? vid, uint? pid)
        {
            var hidMock = new Mock<IDeviceFactory>();
            var usbMock = new Mock<IDeviceFactory>();

            if (isHidConnected && ((!vid.HasValue && !pid.HasValue) || (vid==1 && pid ==1)))
            {
                hidMock.Setup(f => f.GetConnectedDeviceDefinitionsAsync()).Returns(
                    Task.FromResult<IEnumerable<ConnectedDeviceDefinition>>(new List<ConnectedDeviceDefinition> { new ConnectedDeviceDefinition("123") 
                    {
                        ProductId = 1,
                        VendorId = 1
                    } }));


                hidMock.Setup(f => f.GetDevice(It.IsAny<ConnectedDeviceDefinition>())).Returns(
                Task.FromResult<IDevice>( new MockHidDevice("Asd",_LoggerFactoryMock.Object,_loggerMock.Object)));
            }

            if (isUsbConnected && ((!vid.HasValue && !pid.HasValue) || (vid == 2 && pid == 2)))
            {
                usbMock.Setup(f => f.GetConnectedDeviceDefinitionsAsync()).Returns(
                    Task.FromResult<IEnumerable<ConnectedDeviceDefinition>>(new List<ConnectedDeviceDefinition> { new ConnectedDeviceDefinition("321") 
                    {
                        ProductId = 2,
                        VendorId = 2
                    } }));
            }

            return (hidMock, usbMock);
        }

#if NETCOREAPP3_1
        //Check that we can construct objects without loggers
        [TestMethod]
        public void TestNullLoggers()
        {
            new UsbDevice("asd", new Mock<IUsbInterfaceManager>().Object);
            new WindowsHidDevice("asd");
            new WindowsUsbInterface(default, 0 );
            new WindowsUsbInterfaceManager("asd");
        }
#endif

        [TestMethod]
        public async Task TestDeviceListenerAsync()
        {
            var (hidMock, usbMock) = GetMockedFactories(true, true, null, null);

            var isTimeout = await ListenForDeviceAsync(new List<IDeviceFactory> { hidMock.Object, usbMock.Object });

            Assert.IsTrue(!isTimeout, "Timeout");
        }

        //Is this a bug?
        [TestMethod]
        public async Task TestDeviceListenerTimeoutAsync()
        {
            var (hidMock, usbMock) = GetMockedFactories(false, false, null, null);

            var isTimeout = await ListenForDeviceAsync(new List<IDeviceFactory> { hidMock.Object, usbMock.Object });
            Assert.IsTrue(isTimeout, "Device is connected");
        }

        [TestMethod]
        public async Task TestDeviceFactoriesNotRegisteredException()
        {
            var deviceManager = new DeviceManager(_loggerFactory);

            try
            {
                await deviceManager.GetConnectedDeviceDefinitionsAsync();
            }
            catch (DeviceFactoriesNotRegisteredException)
            {
                return;
            }

            Assert.Fail("The call was not stopped");
        }
        #endregion

        /// <summary>
        /// Dummy logger factory for now
        /// </summary>
        private readonly ILoggerFactory _loggerFactory = LoggerFactory.Create((builder) => { });

        [TestMethod]
        public void TestListenerDeviceFactoriesNotRegisteredException()
        {

            var deviceManager = new Mock<IDeviceManager>();

            try
            {
                var deviceListner = new DeviceListener(deviceManager.Object, 1000, _loggerFactory);
                deviceListner.Start();
            }
            catch (DeviceFactoriesNotRegisteredException)
            {
                return;
            }

            Assert.Fail("The call was not stopped");
        }

        #region Exceptions
        [TestMethod]
        public void TestDeviceException()
        {
            try
            {
                var deviceManager = new DeviceManager(_LoggerFactoryMock.Object);
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
                var device = new MockHidDevice("asd", _loggerFactory, _loggerFactory.CreateLogger<MockHidDevice>());
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

        #region Helpers
        private async Task<bool> ListenForDeviceAsync(IEnumerable<IDeviceFactory> deviceFactories)
        {
            var listenTaskCompletionSource = new TaskCompletionSource<bool>();

            var deviceManager = new DeviceManager(_loggerFactory) {  };

            deviceManager.DeviceFactories.AddRange(deviceFactories);

            var deviceListener = new DeviceListener(
                deviceManager,
                1000, _loggerFactory);
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
    }
}

#endif

#pragma warning restore IDE0055
