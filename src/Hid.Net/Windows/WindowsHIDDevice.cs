using Device.Net;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hid.Net.Windows
{
    public class WindowsHidDevice : DeviceBase, IDevice
    {
        #region Fields
        private HidCollectionCapabilities _HidCollectionCapabilities;
        private FileStream _ReadFileStream;
        private FileStream _WriteFileStream;
        private SafeFileHandle _ReadSafeFileHandle;
        private SafeFileHandle _WriteSafeFileHandle;
        #endregion

        #region Private Properties
        private ushort OutputReportByteLength => _HidCollectionCapabilities.OutputReportByteLength > 0 ? _HidCollectionCapabilities.OutputReportByteLength : (ushort)DeviceInformation.WriteBufferSize;
        private string LogSection => nameof(WindowsHidDevice);
        #endregion

        #region Public Properties
        public bool DataHasExtraByte { get; set; } = true;
        public DeviceDefinition DeviceInformation { get; set; }
        public string DevicePath => DeviceInformation.DeviceId;
        public bool IsInitialized { get; private set; }
        public uint? ProductId => DeviceInformation.ProductId;
        public ushort Usage => _HidCollectionCapabilities.Usage;
        public ushort UsagePage => _HidCollectionCapabilities.UsagePage;
        public uint? VendorId => DeviceInformation.VendorId;
        #endregion

        #region Constructor
        public WindowsHidDevice()
        {
        }

        public WindowsHidDevice(DeviceDefinition deviceInformation) : this()
        {
            DeviceInformation = deviceInformation;
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

            if (DeviceInformation == null)
            {
                throw new WindowsHidException($"{nameof(DeviceInformation)} must be specified before {nameof(Initialize)} can be called.");
            }

            _ReadSafeFileHandle = APICalls.CreateFile(DeviceInformation.DeviceId, APICalls.GenericRead | APICalls.GenericWrite, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, 0, IntPtr.Zero);
            _WriteSafeFileHandle = APICalls.CreateFile(DeviceInformation.DeviceId, APICalls.GenericRead | APICalls.GenericWrite, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, 0, IntPtr.Zero);

            if (_ReadSafeFileHandle.IsInvalid)
            {
                throw new Exception("Could not open connection for reading");
            }

            if (_WriteSafeFileHandle.IsInvalid)
            {
                throw new Exception("Could not open connection for writing");
            }

            _HidCollectionCapabilities = HidAPICalls.GetHidCapabilities(_ReadSafeFileHandle);

            _ReadFileStream = new FileStream(_ReadSafeFileHandle, FileAccess.ReadWrite, _HidCollectionCapabilities.OutputReportByteLength, false);
            _WriteFileStream = new FileStream(_WriteSafeFileHandle, FileAccess.ReadWrite, _HidCollectionCapabilities.InputReportByteLength, false);

            IsInitialized = true;

            RaiseConnected();

            return true;
        }

        public async Task InitializeAsync()
        {
            await Task.Run(() => Initialize());
        }

        public override async Task<byte[]> ReadAsync()
        {
            if (_ReadFileStream == null)
            {
                throw new Exception("The device has not been initialized");
            }

            var bytes = new byte[_HidCollectionCapabilities.InputReportByteLength];

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

            if (data.Length > OutputReportByteLength)
            {
                throw new Exception($"Data is longer than {_HidCollectionCapabilities.OutputReportByteLength - 1} bytes which is the device's OutputReportByteLength.");
            }

            byte[] bytes;
            if (DataHasExtraByte)
            {
                if (OutputReportByteLength == data.Length)
                {
                    throw new DeviceException("The data sent to the device was a the same length as the HidCollectionCapabilities.OutputReportByteLength. This probably indicates that DataHasExtraByte should be set to false.");
                }

                bytes = new byte[OutputReportByteLength];
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