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
            var connectedDeviceDefinitions = (await DeviceManager.Current.GetConnectedDeviceDefinitionsAsync(null)).ToList();
            Assert.IsNotNull(connectedDeviceDefinitions);
            Assert.AreSame(0, connectedDeviceDefinitions.Count);
        }

        //var asdasd = new DeviceListener(new List<FilterDeviceDefinition> { new FilterDeviceDefinition { VendorId = 1, ProductId = 1 } }, 1000);
        // await asdasd.CheckForDevicesAsync();
        //var asdas = asdasd.
    }
}
