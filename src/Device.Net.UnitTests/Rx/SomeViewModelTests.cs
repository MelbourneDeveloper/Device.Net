using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

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
