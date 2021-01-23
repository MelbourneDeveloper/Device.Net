using Hid.Net;
using Hid.Net.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.SafeHandles;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class HidTests
    {
        #region Private Fields

        private readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder => _ = builder.AddDebug().AddConsole().SetMinimumLevel(LogLevel.Trace));

        #endregion Private Fields


        #region Public Methods

        [TestMethod]
        public void TestDeviceIdInvalidException()
        {
            try
            {
                _ = new WindowsHidHandler(null, readTransferTransform: (a) => default, writeTransferTransform: (a, b) => default);
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("deviceId", ane.ParamName);
                return;
            }

            Assert.Fail();
        }


        [TestMethod]
        public async Task TestInitializeHidDeviceReadOnly()
        {
            var windowsHidDevice = await InitializeWindowsHidDevice(true);
            Assert.AreEqual(true, windowsHidDevice.IsReadOnly);
        }

        [TestMethod]
        public async Task TestInitializeHidDeviceWriteable()
        {
            var windowsHidDevice = await InitializeWindowsHidDevice(false);
            Assert.AreEqual(false, windowsHidDevice.IsReadOnly);
        }

        [TestMethod]
        public Task TestTrezorHid() => IntegrationTests.TestWriteAndReadFromTrezor(
            GetMockTrezorDeviceFactory(loggerFactory, (readReport)
                //We expect to get back 64 bytes but ReadAsync would normally add the Report Id back index 0
                //In the case of Trezor we just take the 64 bytes and don't put the Report Id back at index 0
                => new TransferResult(readReport.TransferResult.Data, readReport.TransferResult.BytesTransferred), 0),
            64,
            65
            );

        #endregion Public Methods

        #region Private Methods

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

            var windowsHidDevice = new WindowsHidHandler(deviceId, loggerFactory: loggerFactory.Object, readTransferTransform: (a) => default, writeTransferTransform: (a, b) => default, hidApiService: hidService.Object);
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

        private static IDeviceFactory GetMockTrezorDeviceFactory(ILoggerFactory loggerFactory, Func<Report, TransferResult> readReportTransform, byte? defaultReportId)
        {
            //TODO: Turn this in to a real device factory with a mocked GetConnectedDeviceDefinitions
            var deviceFactory = new Mock<IDeviceFactory>();

            //Mock the handler
            var deviceHandler = new Mock<IHidDeviceHandler>();

            _ = deviceHandler.Setup(dh => dh.DeviceId).Returns("123");

            var inputReport = new Report
            (
                0,
                new TransferResult(new byte[]
                { 
                    //Blank out the Report Id because the Windows / UWP handler would do this for us
                    //0,
                63, 35, 35, 0, 17, 0, 0, 0, 142, 10, 17, 98, 105, 116, 99, 111, 105, 110, 116, 114, 101, 122, 111, 114, 46, 99, 111, 109, 16, 1, 24, 6, 32, 3, 50, 24, 66, 70, 67, 69, 48, 52, 68, 52, 67, 51, 69, 68, 53, 51, 70, 68, 51, 66, 67, 57, 53, 53, 48, 54, 56, 0, 64, 1 },
                65)
            );

            _ = deviceHandler.Setup(dh => dh.ReadReportAsync(It.IsAny<CancellationToken>())).ReturnsAsync(inputReport);

            //Create an actual device
            var hidDevice = new HidDevice(deviceHandler.Object, loggerFactory, readReportTransform: readReportTransform, defaultWriteReportId: defaultReportId);

            //Set up the factory calls
            _ = deviceFactory.Setup(df => df.GetConnectedDeviceDefinitionsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
                new List<ConnectedDeviceDefinition>
                {
                    new ConnectedDeviceDefinition(deviceHandler.Object.DeviceId, DeviceType.Hid)
                }); ;

            _ = deviceFactory.Setup(df => df.GetDeviceAsync(It.IsAny<ConnectedDeviceDefinition>(), It.IsAny<CancellationToken>())).ReturnsAsync(hidDevice);

            return deviceFactory.Object;
        }
        #endregion Private Methods

    }
}