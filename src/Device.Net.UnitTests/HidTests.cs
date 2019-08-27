using Hid.Net.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.SafeHandles;
using NSubstitute;
using System;
using System.IO;
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
            var safeFileHandle = new SafeFileHandle((IntPtr)100, true);
            hidService.CreateReadConnection("").ReturnsForAnyArgs(safeFileHandle);
            hidService.CreateWriteConnection("").ReturnsForAnyArgs(safeFileHandle);
            hidService.GetDeviceDefinition(deviceId, safeFileHandle).ReturnsForAnyArgs(new ConnectedDeviceDefinition(deviceId) { ReadBufferSize = 64, WriteBufferSize = 64 });

            var readStream = Substitute.For<Stream>();
            readStream.CanRead.ReturnsForAnyArgs(true);
            hidService.OpenRead(null, 0).ReturnsForAnyArgs(readStream);

            var writeStream = Substitute.For<Stream>();
            writeStream.CanWrite.ReturnsForAnyArgs(true);
            hidService.OpenWrite(null, 0).ReturnsForAnyArgs(writeStream);

            var windowsHidDevice = new WindowsHidDevice(deviceId, null, null, Substitute.For<ILogger>(), Substitute.For<ITracer>(), hidService);
            await windowsHidDevice.InitializeAsync();
        }
    }
}
