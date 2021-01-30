using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;

namespace Hid.Net.Windows
{
    /// <summary>
    /// Service to handle Hid API calls. Windows oriented for now.
    /// </summary>
    public interface IHidApiService : IApiService
    {
        ConnectedDeviceDefinition GetDeviceDefinition(string deviceId, SafeFileHandle safeFileHandle);
        Guid GetHidGuid();
        string GetManufacturer(SafeFileHandle safeFileHandle);
        string GetProduct(SafeFileHandle safeFileHandle);
        string GetSerialNumber(SafeFileHandle safeFileHandle);
        Stream OpenRead(SafeFileHandle readSafeFileHandle, ushort readBufferSize);
        Stream OpenWrite(SafeFileHandle writeSafeFileHandle, ushort writeBufferSize);
    }
}