
using Device.Net.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Usb.Net;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class TrezorMockUsbTests : TrezorUsbTestBase
    {
        #region Helpers
        public override IUsbInterfaceManager GetTrezorUsbInterfaceManager(ILogger logger, ITracer tracer)
        {
            var usbInterface = new Mock<IUsbInterface>();
            var readEndpoint = new Mock<IUsbInterfaceEndpoint>();

            var mockUsbInterfaceManager = new MockUsbInterfaceManager(logger, tracer);

            mockUsbInterfaceManager.UsbInterfaceManager.Setup(u => u.WriteUsbInterface).Returns(usbInterface.Object);
            mockUsbInterfaceManager.UsbInterfaceManager.Setup(u => u.ReadUsbInterface).Returns(usbInterface.Object);
            usbInterface.Setup(i => i.ReadEndpoint).Returns(readEndpoint.Object);
            usbInterface.Setup(i => i.ReadAsync(It.IsAny<uint>())).Returns(Task.FromResult(new ReadResult(TrezorResponse, (uint)TrezorResponse.Length)));

            usbInterface.Setup(a => a.WriteAsync(It.IsAny<byte[]>())).Returns(() =>
            {
               tracer.Trace(true, TrezorRequest);
               return Task.FromResult(true);
            });

            return mockUsbInterfaceManager;
        }
        #endregion
    }
}
