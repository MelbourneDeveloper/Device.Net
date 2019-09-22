
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Usb.Net;
using Usb.Net.Windows;

namespace Device.Net.IntegrationTests
{
    [TestClass]
    public class TrezorWindowsIntegrationTests : TrezorUsbTestBase
    {
        #region Helpers
        public override IUsbInterfaceManager GetTrezorUsbInterfaceManager(ILogger logger, ITracer tracer)
        {
            return new WindowsUsbInterfaceManager(@"\\?\usb#vid_1209&pid_53c1&mi_00#6&1b4d0e06&1&0000#{dee824ef-729b-4a0e-9c14-b7117d33a817}", logger, tracer, null, null);
        }
        #endregion
    }
}
