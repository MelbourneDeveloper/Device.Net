
#if NETCOREAPP

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Device.Net.UnitTests.Rx
{
    [TestClass]
    public class SomeViewModelTests
    {
        [TestMethod]
        public async Task Test()
        {
            var loggerFactory = LoggerFactory.Create(builder => _ = builder.AddDebug().AddConsole().SetMinimumLevel(LogLevel.Trace));
            var devices = new List<ConnectedDeviceDefinition>();
            var mockDevice = new Mock<IDevice>();
            var expectedConnectedDeviceDefinition = new ConnectedDeviceDefinition("123", DeviceType.Hid);

            var deviceManager = new DeviceManager(
                (c) => { },
                (c, ex) => { },
                (d) => d.InitializeAsync(),
                () => Task.FromResult<IReadOnlyList<ConnectedDeviceDefinition>>(devices),
                (c, ct) => Task.FromResult(mockDevice.Object),
                1,
                loggerFactory);
            deviceManager.Start();

            var vm = new SomeViewModel(deviceManager);

            //Verify that there are no devices in the list
            Assert.AreEqual(vm.DeviceDescriptions.Count, 0);

            //Wait for the view model to tell us that that there is a new list of devices
            await WaitForDeviceList(vm);

            //Verify that there are no devices in the list
            Assert.AreEqual(vm.DeviceDescriptions.Count, 0);

            //Add the device
            devices.Add(expectedConnectedDeviceDefinition);

            await WaitForDeviceList(vm);

            //Verify that the device is in the list
            Assert.AreEqual(vm.DeviceDescriptions.Count, 1);
            Assert.AreEqual(expectedConnectedDeviceDefinition.DeviceId, vm.DeviceDescriptions.First().Description);
        }

        private static async Task WaitForDeviceList(SomeViewModel vm)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();
            vm.PropertyChanged += (s, e) => taskCompletionSource.TrySetResult(e.PropertyName);
            var property = await taskCompletionSource.Task;
            Assert.AreEqual(nameof(SomeViewModel.DeviceDescriptions), property);
        }
    }
}

#endif