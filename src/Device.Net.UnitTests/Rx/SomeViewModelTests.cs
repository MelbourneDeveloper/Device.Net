
#if NETCOREAPP

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
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

            var deviceManager = new DeviceManager(
                (a) => { },
                (c, ex) => { },
                (d) => d.InitializeAsync(),
                () => Task.FromResult<IReadOnlyList<ConnectedDeviceDefinition>>(devices),
                (c, ct) => Task.FromResult(new Mock<IDevice>().Object),
                1,
                loggerFactory);

            var vm = new SomeViewModel(deviceManager);
            Assert.AreEqual(vm.DeviceDescriptions.Count, 0);
        }
    }
}

#endif