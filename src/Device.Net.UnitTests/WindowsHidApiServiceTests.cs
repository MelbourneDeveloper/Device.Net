using Hid.Net.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class WindowsHidApiServiceTests
    {
        private SafeFileHandle CreateFileHandle()
        {
            var file = File.Create(Path.GetTempFileName(), 100, FileOptions.Asynchronous);
            return file.SafeFileHandle;
        }

        [TestMethod]
        public void OpenWriteDoesNotThrowWhenZeroLengthBuffer()
        {
            var windowsHidApiService = new WindowsHidApiService(null);
            using var handle = CreateFileHandle();
            _ = windowsHidApiService.OpenWrite(handle, 0);
        }

        [TestMethod]
        public void OpenWriteDoesNotThrowWhenMoreThanZero()
        {
            var windowsHidApiService = new WindowsHidApiService(null);
            using var handle = CreateFileHandle();
            _ = windowsHidApiService.OpenWrite(handle, 10);
        }
    }
}