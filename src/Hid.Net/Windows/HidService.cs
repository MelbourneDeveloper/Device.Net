using Device.Net;
using Microsoft.Win32.SafeHandles;
using System;

namespace Hid.Net.Windows
{
    public class HidService : IHidService
    {
        #region Public Properties
        public ILogger Logger { get; }
        public SafeFileHandle SafeFileHandle { get; }
        #endregion

        public HidService(SafeFileHandle safeFileHandle, ILogger logger)
        {
            Logger = logger;
            SafeFileHandle = safeFileHandle;
        }

        public HidAttributes GetHidAttributes(SafeFileHandle safeFileHandle)
        {
            return HidAPICalls.GetHidAttributes(safeFileHandle);
        }

        public HidCollectionCapabilities GetHidCapabilities(SafeFileHandle readSafeFileHandle)
        {
            return HidAPICalls.GetHidCapabilities(readSafeFileHandle);
        }

        public string GetManufacturer(SafeFileHandle safeFileHandle)
        {
            return HidAPICalls.GetManufacturer(safeFileHandle, Logger);
        }

        public string GetProduct(SafeFileHandle safeFileHandle)
        {
            return HidAPICalls.GetProduct(safeFileHandle, Logger);
        }

        public string GetSerialNumber(SafeFileHandle safeFileHandle)
        {
            return HidAPICalls.GetSerialNumber(safeFileHandle, Logger);
        }

        public Guid GetHidGuid()
        {
            return HidAPICalls.GetHidGuid();
        }
    }
}
