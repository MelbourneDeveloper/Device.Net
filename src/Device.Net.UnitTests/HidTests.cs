#if !NET45

using Hid.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.SafeHandles;
using Moq;
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
                _ = new WindowsHidDevice(null, null);
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
            _ = hidService.CreateReadConnection("", Windows.FileAccessRights.None).ReturnsForAnyArgs(validSafeFileHandle);
            _ = hidService.CreateWriteConnection("").ReturnsForAnyArgs(!isReadonly ? validSafeFileHandle : invalidSafeFileHandle);
            _ = hidService.GetDeviceDefinition(deviceId, validSafeFileHandle).ReturnsForAnyArgs(
                new ConnectedDeviceDefinition(deviceId, DeviceType.Hid, readBufferSize: 64, writeBufferSize: 64));

            var readStream = Substitute.For<Stream>();
            _ = readStream.CanRead.ReturnsForAnyArgs(true);
            _ = hidService.OpenRead(null, 0).ReturnsForAnyArgs(readStream);

            var writeStream = Substitute.For<Stream>();
            _ = writeStream.CanWrite.ReturnsForAnyArgs(true);
            _ = hidService.OpenWrite(null, 0).ReturnsForAnyArgs(writeStream);

            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger<WindowsHidDevice>>();
            _ = logger.Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>())).Returns(new Mock<IDisposable>().Object);

            _ = loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var windowsHidDevice = new WindowsHidDevice(deviceId, loggerFactory: loggerFactory.Object, hidService: hidService);
            await windowsHidDevice.InitializeAsync();

            //TODO: Fix this

            if (!isReadonly)
            {
                //UnitTests.CheckLogMessageText(logger, Messages.SuccessMessageReadFileStreamOpened, LogLevel.Information, Times.Once());

                //logger.Received().Log(Messages.SuccessMessageReadFileStreamOpened, nameof(WindowsHidDevice), null, LogLevel.Information);
            }
            else
            {
                //logger.Received().Log(Messages.WarningMessageOpeningInReadonlyMode(deviceId), nameof(WindowsHidDevice), null, LogLevel.Warning);
            }

            Assert.AreEqual(true, windowsHidDevice.IsInitialized);
            return windowsHidDevice;
        }

    }
}

#endif
