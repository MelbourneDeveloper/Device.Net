using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Linq;
using System.Threading.Tasks;
using Usb.Net;

namespace Device.Net.IntegrationTests
{
    public abstract class TrezorUsbTestBase
    {
        #region Tests
        [TestMethod]
        public async Task ConnectedTestWriteAndReadAsync()
        {
            var logger = Substitute.For<ILogger>();
            var tracer = Substitute.For<ITracer>();

            ReadResult readResult = null;

            using (var trezorUsbInterfaceManager = GetTrezorUsbInterfaceManager(logger, tracer))
            {
                using (var trezorUsbDevice = new UsbDevice("", trezorUsbInterfaceManager, logger, tracer))
                {
                    await trezorUsbInterfaceManager.InitializeAsync();

                    var writeBuffer = new byte[64];
                    writeBuffer[0] = 0x3f;
                    writeBuffer[1] = 0x23;
                    writeBuffer[2] = 0x23;

                    readResult = await trezorUsbDevice.WriteAndReadAsync(writeBuffer);
                }

                var expected = new byte[] { 63, 35, 35, 0, 17, 0, 0, 0, 131, 10, 9, 116, 114, 101, 122, 111, 114, 46, 105, 111, 16, 1, 24, 8, 32, 2, 50, 24, 51, 66, 69, 65, 55, 66, 50, 55, 50, 55, 66, 49, 55, 57, 50, 52, 67, 56, 67, 70, 68, 56, 53, 48, 56, 1, 64, 0, 82, 5, 66, 108, 97, 99, 107, 96 };

                Assert.IsTrue(expected.SequenceEqual(readResult.Data));
            }
        }
        #endregion

        #region Abstract Methods
        public abstract IUsbInterfaceManager GetTrezorUsbInterfaceManager(ILogger logger, ITracer tracer);
        #endregion
    }
}
