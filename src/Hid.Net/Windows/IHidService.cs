using Microsoft.Win32.SafeHandles;
using System;

namespace Hid.Net.Windows
{
    public interface IHidService
    {
        HidAttributes GetHidAttributes(SafeFileHandle safeFileHandle);
        HidCollectionCapabilities GetHidCapabilities(SafeFileHandle readSafeFileHandle);
        Guid GetHidGuid();
        string GetManufacturer(SafeFileHandle safeFileHandle);
        string GetProduct(SafeFileHandle safeFileHandle);
        string GetSerialNumber(SafeFileHandle safeFileHandle);
    }
}