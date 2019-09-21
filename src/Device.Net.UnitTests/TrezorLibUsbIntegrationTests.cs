using LibUsbDotNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net.IntegrationTests
{
    [TestClass]
    public class TrezorLibUsbIntegrationTests
    {
        #region Fields
        #endregion

        #region Tests
        [TestMethod]
        public async Task ConnectedTestReadAsync()
        {
            var devices = UsbDevice.AllDevices.ToList();
            var trezorUsbRegistry = devices.FirstOrDefault(d => d.Pid == 21441 && d.Vid == 4617);
            Assert.IsNotNull(trezorUsbRegistry);
        }
        #endregion

        #region Helpers
        #endregion
    }
}
