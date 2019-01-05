using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Device.Net.Windows
{
    /// <summary>
    /// This class remains untested
    /// </summary>
    public abstract class WindowsDeviceBase : DeviceBase
    {
        #region Protected Properties
        protected virtual string LogSection => nameof(WindowsDeviceBase);
        #endregion

        #region Public Properties
        /// <summary>
        /// TODO: Move this down to the DeviceBase
        /// </summary>
        public string DeviceId { get; }

        #endregion

        #region Constructor
        protected WindowsDeviceBase(string deviceId)
        {
            DeviceId = deviceId;
        }

        protected WindowsDeviceBase(uint? vendorId, uint? productId)
        {
            VendorId = vendorId;
            ProductId = productId;
        }
        #endregion

        #region Public Methods


        public abstract Task InitializeAsync();
        #endregion

        #region Public Static Methods
        public static void HandleError(bool isSuccess, string message)
        {
            if (isSuccess) return;
            var errorCode = Marshal.GetLastWin32Error();
            throw new Exception($"{message}. Error code: {errorCode}");
        }
        #endregion
    }
}