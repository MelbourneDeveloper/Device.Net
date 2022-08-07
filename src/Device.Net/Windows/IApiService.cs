﻿using Microsoft.Win32.SafeHandles;
using System;

#pragma warning disable CA1021 // Avoid out parameters
#pragma warning disable CA1045 // Do not pass types by reference

namespace Device.Net.Windows
{
    public interface IApiService
    {
        SafeFileHandle CreateFile(string lpFileName, FileAccessRights dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        SafeFileHandle CreateWriteConnection(string deviceId);
        SafeFileHandle CreateReadConnection(string deviceId, FileAccessRights desiredAccess);
        //TODO: Get rid of read/write. They can be done with file streams...
        bool AReadFile(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToRead, out uint lpNumberOfBytesRead, int lpOverlapped);
        bool AWriteFile(SafeFileHandle hFile, byte[] lpBuffer, int nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, int lpOverlapped);
        bool AGetCommState(SafeFileHandle hFile, ref Dcb lpDCB);
        bool ASetCommState(SafeFileHandle hFile, ref Dcb lpDCB);
        bool ASetCommTimeouts(SafeFileHandle hFile, ref CommTimeouts lpCommTimeouts);
        bool APurgeComm(SafeFileHandle hFile, int dwFlags);
    }
}
