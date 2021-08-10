using Device.Net.Windows;
using Hid.Net.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;

namespace Device.Net.UnitTests
{
    [TestClass]
    public class WindowsHidApiServiceTests
    {
        private SafeFileHandle CreateFileHandle()
        {
            return APICalls.CreateFile(Path.GetTempFileName(),
                    FileAccessRights.GenericWrite | FileAccessRights.GenericRead,
                    APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting,
                    APICalls.FileAttributeNormal | APICalls.FileFlagOverlapped, IntPtr.Zero);
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