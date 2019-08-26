using Hid.Net.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class HidTests
    {
        [TestMethod]
        public void TestDeviceExfception()
        {
            try
            {
                new WindowsHidDevice(null, null, null);
            }
            catch (ArgumentNullException ane)
            {
                Assert.AreEqual("deviceId", ane.ParamName);
                return;
            }

            Assert.Fail();
        }
    }
}
