using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Usb.Net;

namespace Device.Net.IntegrationTests
{
    public abstract class TrezorUsbTestBase
    {
        public static byte[] TrezorRequest;
        public static byte[] TrezorResponse { get; } = new byte[] { 63, 35, 35, 0, 17, 0, 0, 0, 131, 10, 9, 116, 114, 101, 122, 111, 114, 46, 105, 111, 16, 1, 24, 8, 32, 2, 50, 24, 51, 66, 69, 65, 55, 66, 50, 55, 50, 55, 66, 49, 55, 57, 50, 52, 67, 56, 67, 70, 68, 56, 53, 48, 56, 1, 64, 0, 82, 5, 66, 108, 97, 99, 107, 96 };

        static TrezorUsbTestBase()
        {
            TrezorRequest = new byte[64];
            TrezorRequest[0] = 0x3f;
            TrezorRequest[1] = 0x23;
            TrezorRequest[2] = 0x23;
        }

        #region Tests
        [TestMethod]
        public async Task ConnectedTestWriteAndReadAsync()
        {
            var logger = new Mock<ILogger>();
            var tracer = new Mock<ITracer>();

            ReadResult readResult = null;

            using (var trezorUsbInterfaceManager = GetTrezorUsbInterfaceManager(logger.Object, tracer.Object))
            {
                using (var trezorUsbDevice = new UsbDevice("", trezorUsbInterfaceManager, logger.Object, tracer.Object))
                {
                    await trezorUsbInterfaceManager.InitializeAsync();

                    readResult = await trezorUsbDevice.WriteAndReadAsync(TrezorRequest);
                }

                var expected = TrezorResponse;

                Assert.IsTrue(expected.SequenceEqual(readResult.Data));
            }

            tracer.Verify(t => t.Trace(true, TrezorRequest));
        }
        #endregion

        #region Abstract Methods
        public abstract IUsbInterfaceManager GetTrezorUsbInterfaceManager(ILogger logger, ITracer tracer);
        #endregion
    }
}
