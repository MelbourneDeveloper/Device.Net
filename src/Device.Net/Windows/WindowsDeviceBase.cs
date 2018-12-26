﻿using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Device.Net.Windows
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
        private FileStream _ReadFileStream;
        private SafeFileHandle _ReadSafeFileHandle;
        private FileStream _WriteFileStream;
        private SafeFileHandle _WriteSafeFileHandle;
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
            var pointerToBuffer = Marshal.AllocHGlobal(126);

            _ReadSafeFileHandle = APICalls.CreateFile(DeviceId, APICalls.GenericRead | APICalls.GenericWrite, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, 0, IntPtr.Zero);
            _WriteSafeFileHandle = APICalls.CreateFile(DeviceId, APICalls.GenericRead | APICalls.GenericWrite, APICalls.FileShareRead | APICalls.FileShareWrite, IntPtr.Zero, APICalls.OpenExisting, 0, IntPtr.Zero);

            //TODO: Deal with issues here

            Marshal.FreeHGlobal(pointerToBuffer);

            //TODO: Deal with issues here

            if (_ReadSafeFileHandle.IsInvalid)
            {
                throw new Exception("Read handle no good");
            }

            if (_WriteSafeFileHandle.IsInvalid)
            {
                throw new Exception("Write handle no good");
            }

            _ReadFileStream = new FileStream(_ReadSafeFileHandle, FileAccess.ReadWrite, ReadBufferSize, false);
            _WriteFileStream = new FileStream(_WriteSafeFileHandle, FileAccess.ReadWrite, WriteBufferSize, false);

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
            if (_ReadFileStream == null)
            {
                throw new Exception("The device has not been initialized");
            }

            var bytes = new byte[ReadBufferSize];

            try
            {
                _ReadFileStream.Read(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                Logger.Log(Helpers.ReadErrorMessage, ex, LogSection);
                throw new IOException(Helpers.ReadErrorMessage, ex);
            }

            Tracer?.Trace(false, bytes);

            return bytes;
        }

        public async Task WriteAsync(byte[] data)
        {
            if (_WriteFileStream == null)
            {
                throw new Exception("The device has not been initialized");
            }

            if (data.Length > WriteBufferSize)
            {
                throw new Exception($"Data is longer than {WriteBufferSize} bytes which is the device's OutputReportByteLength.");
            }
      
            if (_WriteFileStream.CanWrite)
            {
                try
                {
                    await _WriteFileStream.WriteAsync(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Logger.Log(Helpers.WriteErrorMessage, ex, LogSection);
                    throw new IOException(Helpers.WriteErrorMessage, ex);
                }

                Tracer?.Trace(true, data);
            }
            else
            {
                throw new IOException("The file stream cannot be written to");
            }
        }
        #endregion
    }
}