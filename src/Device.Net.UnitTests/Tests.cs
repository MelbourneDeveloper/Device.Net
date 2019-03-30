using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class Tests
    {
        [TestInitialize]
        public void Startup()
        {
            MockHidFactory.Register(null);
        }

        [TestMethod]
        public async Task GetDevicesDisconnectedNullFilter()
        {
            MockHidFactory.IsConnected = false;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null)).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(0, connectedDeviceDefinitions.Count);
        }

        [TestMethod]
        public async Task GetDevicesConnectedNullFilter()
        {
            MockHidFactory.IsConnected = true;
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null)).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreEqual(1, connectedDeviceDefinitions.Count);
        }

        //var asdasd = new DeviceListener(new List<FilterDeviceDefinition> { new FilterDeviceDefinition { VendorId = 1, ProductId = 1 } }, 1000);
        // await asdasd.CheckForDevicesAsync();
        //var asdas = asdasd.
    }
}
