using Device.Net.Windows;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Device.Net
{
    /// <summary>
    /// This class remains untested
    /// </summary>
    public abstract class WindowsDeviceBase : DeviceBase, IDevice
    {
        //TODO: Implement
        #region Events
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        #endregion

        #region Fields
        private IntPtr _WriteHandle;
        #endregion

        #region Private Properties
        private string LogSection => nameof(WindowsDeviceBase);
        #endregion

        #region Public Properties
        public string DeviceId { get; }
        public bool IsInitialized { get; private set; }
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
        public void Dispose()
        {
            Disconnected?.Invoke(this, new EventArgs());
        }

        //TODO
#pragma warning disable CS1998
        public async Task<bool> GetIsConnectedAsync()
#pragma warning restore CS1998
        {
            return IsInitialized;
        }

        public bool Initialize()
        {
            Dispose();

            if (string.IsNullOrEmpty(DeviceId))
            {
                throw new WindowsException($"{nameof(DeviceDefinition)} must be specified before {nameof(Initialize)} can be called.");
            }

            _WriteHandle = APICalls.CreateFile(DeviceId, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.OpenOrCreate, 0, IntPtr.Zero);

            //if (_WriteHandle.IsInvalid)
            //{
            var readerrorCode = Marshal.GetLastWin32Error();

            if (readerrorCode > 0)
                throw new Exception($"Write handle no good. Error code: {readerrorCode}");
            //}

            IsInitialized = true;

            Connected?.Invoke(this, new EventArgs());

            return true;
        }

        public async Task InitializeAsync()
        {
            await Task.Run(() => Initialize());
        }

        public async Task<byte[]> ReadAsync()
        {
            var bytes = new byte[ReadBufferSize];

            var isSuccess = APICalls.ReadFile(_WriteHandle, bytes, ReadBufferSize, out var asdds, 0);

            var errorCode = Marshal.GetLastWin32Error();

            if (!isSuccess)
            {
                throw new Exception($"Error code {errorCode}");
            }

            Tracer?.Trace(false, bytes);

            return bytes;
        }

        public async Task WriteAsync(byte[] data)
        {
            if (data.Length > WriteBufferSize)
            {
                throw new Exception($"Data is longer than {WriteBufferSize} bytes which is the device's OutputReportByteLength.");
            }


            var isSuccess = APICalls.WriteFile(_WriteHandle, data, (uint)data.Length, out var asdds, 0);

            var errorCode = Marshal.GetLastWin32Error();

            //if (!isSuccess)
            //{
            //    throw new Exception($"Error code {errorCode}");
            //}
        }
        #endregion
    }
}