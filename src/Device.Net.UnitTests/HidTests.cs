using Hid.Net.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var windowsHidDevice = new WindowsHidDevice("test", null, null, Substitute.For<ILogger>(), Substitute.For<ITracer>(), Substitute.For<IHidService>());
            await windowsHidDevice.InitializeAsync();
        }
    }
}
