using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hid.Net.Windows
{
    public class WindowsHidDevice : WindowsDeviceBase, IDevice
    {
        #region Fields
        private FileStream _ReadFileStream;
        private FileStream _WriteFileStream;
        private SafeFileHandle _ReadSafeFileHandle;
        private SafeFileHandle _WriteSafeFileHandle;
        private WindowsDeviceDefinition _DeviceDefinition;
        #endregion

        #region Private Properties
        private string LogSection => nameof(WindowsHidDevice);
        #endregion

        #region Public Overrides
        public override ushort WriteBufferSize => DeviceInformation == null ? throw new Exception("Device has not been initialized") : (ushort)DeviceInformation.WriteBufferSize.Value;
        public override ushort ReadBufferSize => DeviceInformation == null ? throw new Exception("Device has not been initialized") : (ushort)DeviceInformation.ReadBufferSize.Value;
        #endregion

        #region Public Properties
        public bool DataHasExtraByte { get; set; } = true;
        public string DevicePath => DeviceInformation.DeviceId;
        public uint? ProductId => DeviceInformation.ProductId;
        public uint? VendorId => DeviceInformation.VendorId;
        #endregion

        #region Constructor
        public WindowsHidDevice(string deviceId) : base(deviceId)
        {
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            IsInitialized = false;

            _ReadFileStream?.Dispose();
            _WriteFileStream?.Dispose();

            if (_ReadSafeFileHandle != null && !(_ReadSafeFileHandle.IsInvalid))
            {
                _ReadSafeFileHandle.Dispose();
            }

            if (_WriteSafeFileHandle != null && !_WriteSafeFileHandle.IsInvalid)
            {
                _WriteSafeFileHandle.Dispose();
            }

            RaiseDisconnected();
        }

        public bool Initialize()
        {
            Dispose();

            if (string.IsNullOrEmpty(DeviceId))
            {
                throw new WindowsHidException($"{nameof(DeviceId)} must be specified before {nameof(Initialize)} can be called.");
            }

            _ReadSafeFileHandle = APICalls.CreateFile(DeviceId, APICalls.GenericRead | APICalls.GenericWrite, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, 0, IntPtr.Zero);
            _WriteSafeFileHandle = APICalls.CreateFile(DeviceId, APICalls.GenericRead | APICalls.GenericWrite, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, 0, IntPtr.Zero);

            if (_ReadSafeFileHandle.IsInvalid)
            {
                throw new Exception("Could not open connection for reading");
            }

            if (_WriteSafeFileHandle.IsInvalid)
            {
                throw new Exception("Could not open connection for writing");
            }

            _DeviceDefinition = WindowsHidDeviceFactory.GetDeviceDefinition(DeviceId, _ReadSafeFileHandle);

            _ReadFileStream = new FileStream(_ReadSafeFileHandle, FileAccess.ReadWrite, ReadBufferSize, false);
            _WriteFileStream = new FileStream(_WriteSafeFileHandle, FileAccess.ReadWrite, WriteBufferSize, false);

            IsInitialized = true;

            RaiseConnected();

            return true;
        }

        public override async Task InitializeAsync()
        {
            await Task.Run(() => Initialize());
        }

        public override async Task<byte[]> ReadAsync()
        {
            if (_ReadFileStream == null)
            {
                throw new Exception("The device has not been initialized");
            }

            var bytes = new byte[ReadBufferSize];

            try
            {
                await _ReadFileStream.ReadAsync(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                Logger.Log(Helpers.ReadErrorMessage, ex, LogSection);
                throw new IOException(Helpers.ReadErrorMessage, ex);
            }

            byte[] retVal;
            if (DataHasExtraByte)
            {
                retVal = Helpers.RemoveFirstByte(bytes);
            }
            else
            {
                retVal = bytes;
            }

            Tracer?.Trace(false, retVal);

            return retVal;
        }

        public override async Task WriteAsync(byte[] data)
        {
            if (_WriteFileStream == null)
            {
                throw new Exception("The device has not been initialized");
            }

            if (data.Length > WriteBufferSize)
            {
                throw new Exception($"Data is longer than {WriteBufferSize - 1} bytes which is the device's OutputReportByteLength.");
            }

            byte[] bytes;
            if (DataHasExtraByte)
            {
                if (WriteBufferSize == data.Length)
                {
                    throw new DeviceException("The data sent to the device was a the same length as the HidCollectionCapabilities.OutputReportByteLength. This probably indicates that DataHasExtraByte should be set to false.");
                }

                bytes = new byte[WriteBufferSize];
                Array.Copy(data, 0, bytes, 1, data.Length);
                bytes[0] = 0;
            }
            else
            {
                bytes = data;
            }

            if (_WriteFileStream.CanWrite)
            {
                try
                {
                    await _WriteFileStream.WriteAsync(bytes, 0, bytes.Length);
                }
                catch (Exception ex)
                {
                    Logger.Log(Helpers.WriteErrorMessage, ex, LogSection);
                    throw new IOException(Helpers.WriteErrorMessage, ex);
                }

                Tracer?.Trace(true, bytes);
            }
            else
            {
                throw new IOException("The file stream cannot be written to");
            }
        }
        #endregion
    }
}