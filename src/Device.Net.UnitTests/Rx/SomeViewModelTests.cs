
#if NETCOREAPP

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            _ = mockDevice.Setup(d => d.ConnectedDeviceDefinition).Returns(new ConnectedDeviceDefinition("123", DeviceType.Hid));
            var expectedConnectedDeviceDefinition = new ConnectedDeviceDefinition("123", DeviceType.Hid);
            var observable = new Observable<IDevice>();

            var deviceManager = new DeviceManager(
                (c) => observable.Next(c),
                (c, ex) => { },
                async (d) =>
                {
                    //Simulate taking some time to init
                    await Task.Delay(10);
                    await d.InitializeAsync();
                },
                () => Task.FromResult<IReadOnlyList<ConnectedDeviceDefinition>>(devices),
                (c, ct) => Task.FromResult(mockDevice.Object),
                //Note: if the polling is faster than the init, we could end up with more than one connected device connection
                50,
                loggerFactory);
            deviceManager.Start();

            var vm = new SomeViewModel(deviceManager, observable);

            //Verify that there are no devices in the list
            Assert.AreEqual(vm.DeviceDescriptions.Count, 0);

            //Verify that there are no devices in the list
            Assert.AreEqual(vm.DeviceDescriptions.Count, 0);

            var propertyChangedList = new List<string>();
            vm.PropertyChanged += (s, e) => propertyChangedList.Add(e.PropertyName);

            //Add the device
            devices.Add(expectedConnectedDeviceDefinition);

            //Wait for events to occur
            await Task.Delay(300);

            //Verify that the device is in the list
            Assert.AreEqual(vm.DeviceDescriptions.Count, 1);
            Assert.AreEqual(expectedConnectedDeviceDefinition.DeviceId, vm.DeviceDescriptions.First().Description);
            Assert.AreEqual(vm.ConnectedDevice, mockDevice.Object);

            //Note: if the polling is faster than the init, we could end up with more than one connected device connection
            Assert.IsTrue(propertyChangedList.Single(p => p == nameof(SomeViewModel.ConnectedDevice)) != null);
            Assert.IsTrue(propertyChangedList.Contains(nameof(SomeViewModel.DeviceDescriptions)));

            mockDevice.Verify(d => d.InitializeAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

#endif