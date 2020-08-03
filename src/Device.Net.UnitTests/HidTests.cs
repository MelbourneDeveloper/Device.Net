using Hid.Net.Windows;
using Microsoft.Extensions.Logging;
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
        public async Task TestInitializeHidDeviceWriteable()
        {
            var windowsHidDevice = await InitializeWindowsHidDevice(false);
            Assert.AreEqual(false, windowsHidDevice.IsReadOnly);
        }

        [TestMethod]
        public async Task TestInitializeHidDeviceReadOnly()
        {
            var windowsHidDevice = await InitializeWindowsHidDevice(true);
            Assert.AreEqual(true, windowsHidDevice.IsReadOnly);
        }

        private static async Task<WindowsHidDevice> InitializeWindowsHidDevice(bool isReadonly)
        {
            const string deviceId = "test";
            var hidService = Substitute.For<IHidApiService>();
            var invalidSafeFileHandle = new SafeFileHandle((IntPtr)(-1), true);
            var validSafeFileHandle = new SafeFileHandle((IntPtr)100, true);
            hidService.CreateReadConnection("", Windows.FileAccessRights.None).ReturnsForAnyArgs(validSafeFileHandle);
            hidService.CreateWriteConnection("").ReturnsForAnyArgs(!isReadonly ? validSafeFileHandle : invalidSafeFileHandle);
            hidService.GetDeviceDefinition(deviceId, validSafeFileHandle).ReturnsForAnyArgs(new ConnectedDeviceDefinition(deviceId) { ReadBufferSize = 64, WriteBufferSize = 64 });

            var readStream = Substitute.For<Stream>();
            readStream.CanRead.ReturnsForAnyArgs(true);
            hidService.OpenRead(null, 0).ReturnsForAnyArgs(readStream);

            var writeStream = Substitute.For<Stream>();
            writeStream.CanWrite.ReturnsForAnyArgs(true);
            hidService.OpenWrite(null, 0).ReturnsForAnyArgs(writeStream);

            var logger = Substitute.For<ILogger>();

            var windowsHidDevice = new WindowsHidDevice(deviceId, null, null, logger, Substitute.For<ITracer>(), hidService);
            await windowsHidDevice.InitializeAsync();

            throw new NotImplementedException();

            //if (!isReadonly)
            //{
            //    logger.Received().Log(Messages.SuccessMessageReadFileStreamOpened, nameof(WindowsHidDevice), null, LogLevel.Information);
            //}
            //else
            //{
            //    logger.Received().Log(Messages.WarningMessageOpeningInReadonlyMode(deviceId), nameof(WindowsHidDevice), null, LogLevel.Warning);
            //}

            //Assert.AreEqual(true, windowsHidDevice.IsInitialized);
            //return windowsHidDevice;
        }
    }
}
