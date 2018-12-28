using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Device.Net.Windows
{
    /// <summary>
    /// This class remains untested
    /// </summary>
    public abstract class WindowsDeviceBase : DeviceBase, IDevice
    {
        #region Private Properties
        private string LogSection => nameof(WindowsDeviceBase);
        #endregion

        #region Public Properties
        public abstract WindowsHidDeviceDefinition DeviceInformation { get; }
        public string DeviceId { get; }
        public bool IsInitialized { get; protected set; }
        public abstract ushort WriteBufferSize { get; }
        public abstract ushort ReadBufferSize { get; }
        #endregion

        #region Constructor
        protected WindowsDeviceBase(string deviceId)
        {
            DeviceId = deviceId;
        }
        #endregion

        #region Public Methods
        public virtual void Dispose()
        {
            RaiseDisconnected();
        }

        //TODO
#pragma warning disable CS1998
        public async Task<bool> GetIsConnectedAsync()
#pragma warning restore CS1998
        {
            return IsInitialized;
        }

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