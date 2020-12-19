using Device.Net.UWP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Device.Net.UWPUnitTests
{
    [TestClass]
    public class UWPUnitTests
    {
        [TestMethod]
        public void TestGetAqsSingleHidDevice()
        {
            var aqs = AqsHelpers.GetAqs(new List<FilterDeviceDefinition> { new FilterDeviceDefinition(10741, 4112) }, DeviceType.Hid);
            Assert.AreEqual("System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True AND (System.DeviceInterface.Hid.VendorId:=10741 AND System.DeviceInterface.Hid.ProductId:=4112)", aqs);
        }
    }
}
