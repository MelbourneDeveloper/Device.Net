using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hid.Net.Windows
{
    public class WindowsHidDevice : WindowsDeviceBase, IHidDevice
    {
        #region Fields
        private FileStream _ReadFileStream;
        private FileStream _WriteFileStream;
        private SafeFileHandle _ReadSafeFileHandle;
        private SafeFileHandle _WriteSafeFileHandle;
        private bool _IsClosing;
        private bool disposed = false;
        #endregion

        #region Protected Properties
        protected override string LogSection => nameof(WindowsHidDevice);
        #endregion

        #region Public Overrides
        public override bool IsInitialized => _WriteSafeFileHandle != null && !_WriteSafeFileHandle.IsInvalid;
        #endregion

        #region Public Overrides
        public override ushort WriteBufferSize => ConnectedDeviceDefinition == null ? (ushort)0 : (ushort)ConnectedDeviceDefinition.WriteBufferSize.Value;
        public override ushort ReadBufferSize => ConnectedDeviceDefinition == null ? (ushort)0 : (ushort)ConnectedDeviceDefinition.ReadBufferSize.Value;
        #endregion

        #region Public Properties
        /// <summary> 
        /// Many Hid devices on Windows have a buffer size that is one byte larger than the logical buffer size. For compatibility with other platforms etc. we need to remove the first byte. See RemoveFirstByte
        /// </summary> 
        public bool DataHasExtraByte => WriteBufferSize == 65;
        public byte DefaultReportId { get; set; }
        #endregion

        #region Constructor
        public WindowsHidDevice(string deviceId) : base(deviceId)
        {
        }
        #endregion

        #region Private Methods
        private bool Initialize()
        {
            Close();

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

            ConnectedDeviceDefinition = WindowsHidDeviceFactory.GetDeviceDefinition(DeviceId, _ReadSafeFileHandle);

            _ReadFileStream = new FileStream(_ReadSafeFileHandle, FileAccess.ReadWrite, ReadBufferSize, false);
            _WriteFileStream = new FileStream(_WriteSafeFileHandle, FileAccess.ReadWrite, WriteBufferSize, false);

            return true;
        }
        #endregion

        #region Public Methods
        public void Close()
        {
            if (_IsClosing) return;

            _IsClosing = true;

            try
            {
                _ReadFileStream?.Dispose();
                _WriteFileStream?.Dispose();

                _ReadFileStream = null;
                _WriteFileStream = null;

                if (_ReadSafeFileHandle != null)
                {
                    _ReadSafeFileHandle.Dispose();
                    _ReadSafeFileHandle = null;
                }

                if (_WriteSafeFileHandle != null)
                {
                    _WriteSafeFileHandle.Dispose();
                    _WriteSafeFileHandle = null;
                }
            }
            catch (Exception)
            {
                //TODO: Logging
            }

            _IsClosing = false;
        }

        public override void Dispose()
        {
            if (disposed) return;

            disposed = true;

            Close();

            base.Dispose();
        }

        public override async Task InitializeAsync()
        {
            if (disposed) throw new Exception(DeviceDisposedErrorMessage);

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
                Log(Helpers.ReadErrorMessage, ex);
                throw new IOException(Helpers.ReadErrorMessage, ex);
            }

            var retVal = DataHasExtraByte ? RemoveFirstByte(bytes) : bytes;

            Tracer?.Trace(false, retVal);

            return retVal;
        }

        public override Task WriteAsync(byte[] data)
        {
            return WriteAsync(data, 0);
        }

        public  async Task WriteAsync(byte[] data, byte? reportId)
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
                bytes[0] = reportId ?? DefaultReportId;
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
                    Log(Helpers.WriteErrorMessage, ex);
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