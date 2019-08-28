using Microsoft.Win32.SafeHandles;

namespace Device.Net.Windows
{
    public interface IApiService
    {
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
