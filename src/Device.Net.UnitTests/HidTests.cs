#if !NET45

using Hid.Net;
using Hid.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.SafeHandles;
using Moq;
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
                _ = new WindowsHidHandler(null, null);
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


        private static async Task<WindowsHidHandler> InitializeWindowsHidDevice(bool isReadonly)
        {
            const string deviceId = "test";
            var hidService = new Mock<IHidApiService>();
            var invalidSafeFileHandle = new SafeFileHandle((IntPtr)(-1), true);
            var validSafeFileHandle = new SafeFileHandle((IntPtr)100, true);

            _ = hidService.Setup(s => s.CreateReadConnection(deviceId, Windows.FileAccessRights.GenericRead)).Returns(validSafeFileHandle);
            _ = hidService.Setup(s => s.CreateWriteConnection(deviceId)).Returns(!isReadonly ? validSafeFileHandle : invalidSafeFileHandle);
            _ = hidService.Setup(s => s.GetDeviceDefinition(deviceId, validSafeFileHandle)).Returns(new ConnectedDeviceDefinition(deviceId, DeviceType.Hid, readBufferSize: 64, writeBufferSize: 64));

            var readStream = new Mock<Stream>();
            _ = readStream.Setup(s => s.CanRead).Returns(true);
            _ = hidService.Setup(s => s.OpenRead(It.IsAny<SafeFileHandle>(), It.IsAny<ushort>())).Returns(readStream.Object);

            var writeStream = new Mock<Stream>();
            _ = readStream.Setup(s => s.CanWrite).Returns(!isReadonly);
            _ = hidService.Setup(s => s.OpenWrite(It.IsAny<SafeFileHandle>(), It.IsAny<ushort>())).Returns(readStream.Object);

            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger<HidDevice>>();
            _ = logger.Setup(l => l.BeginScope(It.IsAny<It.IsAnyType>())).Returns(new Mock<IDisposable>().Object);

            _ = loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var windowsHidDevice = new WindowsHidHandler(deviceId, (a) => default, loggerFactory: loggerFactory.Object, hidApiService: hidService.Object);
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

            hidService.Verify(s => s.OpenRead(It.IsAny<SafeFileHandle>(), It.IsAny<ushort>()));

            if (!isReadonly)
            {
                hidService.Verify(s => s.OpenWrite(It.IsAny<SafeFileHandle>(), It.IsAny<ushort>()));
            }

            return windowsHidDevice;
        }

    }
}

#endif
