using Hid.Net.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.SafeHandles;
using NSubstitute;
using System;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class HidTests
    {
        [TestMethod]
        public void TestDeviceIdInvalidException()
        {
            try
            {
                new WindowsHidDevice(null, null, null);
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("deviceId", ane.ParamName);
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public async Task TestInitializeHidDevice()
        {
            const string deviceId = "test";
            var hidService = Substitute.For<IHidService>();
            var returnThis = new SafeFileHandle((IntPtr)100, true);
            hidService.CreateReadConnection("").ReturnsForAnyArgs(returnThis);
            hidService.CreateWriteConnection("").ReturnsForAnyArgs(returnThis);
            hidService.GetDeviceDefinition(deviceId, returnThis).ReturnsForAnyArgs(new ConnectedDeviceDefinition(deviceId) { ReadBufferSize = 64, WriteBufferSize = 64 });
            var windowsHidDevice = new WindowsHidDevice(deviceId, null, null, Substitute.For<ILogger>(), Substitute.For<ITracer>(), hidService);
            await windowsHidDevice.InitializeAsync();
        }
    }
}
