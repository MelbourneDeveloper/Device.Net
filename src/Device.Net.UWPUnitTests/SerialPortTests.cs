using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace Device.Net.UWPIntegrationTests
{
    [TestClass]
    public class SerialPortTests
    {
        [TestMethod]
        public async Task TestReadAsync1()
        {
            var aqs = SerialDevice.GetDeviceSelector("COM7");
            var deviceInformationCollection = await DeviceInformation.FindAllAsync(aqs);
            var goodRead = false;
            uint maxReadCount = 1024;
            var cancellationToken = new CancellationToken();
            var deviceInfo = deviceInformationCollection.FirstOrDefault();

            Assert.IsNotNull(deviceInfo);

            using (var serialDevice = await SerialDevice.FromIdAsync(deviceInfo.Id))
            {
                serialDevice.Parity = SerialParity.None;
                serialDevice.BaudRate = 4800;
                serialDevice.DataBits = 8;

                using (var dataReader = new DataReader(serialDevice.InputStream)
                {
                    InputStreamOptions = InputStreamOptions.Partial,
                    UnicodeEncoding = UnicodeEncoding.Utf8
                })
                {
                    for (var i = 0; i < 15; i++)
                    {
                        var dataReaderLoadOperation = dataReader.LoadAsync(maxReadCount);
                        using (var dataReaderLoadTask = dataReaderLoadOperation.AsTask(cancellationToken))
                        {

                            var readByteCount = await dataReaderLoadTask;
                            var buffer = new byte[readByteCount];
                            dataReader.ReadBytes(buffer);

                            var nmeaString = System.Text.Encoding.UTF8.GetString(buffer);

                            if (!string.IsNullOrEmpty(nmeaString))
                            {
                                Console.WriteLine(nmeaString);
                                goodRead = true;
                                break;
                            }
                        }

                        await Task.Delay(900);
                    }
                }
            }

            Assert.IsTrue(goodRead);
        }

        [TestMethod]
        public async Task TestReadAsync2()
        {
            var aqs = SerialDevice.GetDeviceSelector("COM7");
            var deviceInformationCollection = await DeviceInformation.FindAllAsync(aqs);
            var goodRead = false;
            uint maxReadCount = 1024;
            var deviceInfo = deviceInformationCollection.FirstOrDefault();

            Assert.IsNotNull(deviceInfo);
            using (var serialDevice = await SerialDevice.FromIdAsync(deviceInfo.Id))
            {
                serialDevice.Parity = SerialParity.None;
                serialDevice.BaudRate = 4800;
                serialDevice.DataBits = 8;

                for (var i = 0; i < 30; i++)
                {
                    var emptyArray = new byte[maxReadCount];
                    var emptyBuffer = emptyArray.AsBuffer();
                    var buffer = await serialDevice.InputStream.ReadAsync(emptyBuffer, maxReadCount, InputStreamOptions.Partial);

                    var data = buffer.ToArray();
                    var nmeaString = System.Text.Encoding.UTF8.GetString(data);

                    if (!string.IsNullOrEmpty(nmeaString))
                    {
                        goodRead = true;
                        Console.WriteLine($"Try: {i} {nmeaString}");
                        break;
                    }
                }

                await Task.Delay(900);
            }

            Assert.IsTrue(goodRead);
        }

        [TestMethod]
        public async Task TestReadAsync3()
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
