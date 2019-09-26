using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

namespace Device.Net.UWPUnitTests
{
    [TestClass]
    public class SerialPortTests
    {
        [TestMethod]
        public async Task TestReadAsync()
        {
            var aqs = SerialDevice.GetDeviceSelector("COM7");
            var deviceInformationCollection = await DeviceInformation.FindAllAsync(aqs);
            var goodRead = false;
            var maxReadCount = 1024;
            var deviceInfo = deviceInformationCollection.FirstOrDefault();

            Assert.IsNotNull(deviceInfo);
            using (var serialDevice = await SerialDevice.FromIdAsync(deviceInfo.Id))
            {
                serialDevice.BaudRate = 4800;
                serialDevice.Parity = SerialParity.None;
                serialDevice.DataBits = 8;

                var cancellationTokenSource = new CancellationTokenSource();

                using (var stream = serialDevice.InputStream.AsStreamForRead(0))
                {
                    for (var i = 0; i < 100; i++)
                    {
                        var buffer = new byte[maxReadCount];
                        var readCount = await stream.ReadAsync(buffer, 0, maxReadCount, cancellationTokenSource.Token);

                        if (readCount > 0)
                        {
                            var nmeaString = System.Text.Encoding.UTF8.GetString(buffer);

                            if (!string.IsNullOrWhiteSpace(nmeaString))
                            {
                                return;
                            }
                        }

                        Task.Delay(10);
                    }
                }

            }

            Assert.IsTrue(goodRead);
        }

    }
}
