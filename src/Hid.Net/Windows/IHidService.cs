using Device.Net;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;

namespace Hid.Net.Windows
{
    /// <summary>
    /// Service to handle Hid API calls. Windows oriented for now.
    /// </summary>
    public interface IHidService
    {
        ConnectedDeviceDefinition GetDeviceDefinition(string deviceId, SafeFileHandle safeFileHandle);
        HidAttributes GetHidAttributes(SafeFileHandle safeFileHandle);
        HidCollectionCapabilities GetHidCapabilities(SafeFileHandle readSafeFileHandle);
        Guid GetHidGuid();
        string GetManufacturer(SafeFileHandle safeFileHandle);
        string GetProduct(SafeFileHandle safeFileHandle);
        string GetSerialNumber(SafeFileHandle safeFileHandle);
        SafeFileHandle CreateWriteConnection(string deviceId);
        SafeFileHandle CreateReadConnection(string deviceId);
        Stream OpenRead(SafeFileHandle readSafeFileHandle, ushort readBufferSize);
        Stream OpenWrite(SafeFileHandle writeSafeFileHandle, ushort writeBufferSize);
    }
}