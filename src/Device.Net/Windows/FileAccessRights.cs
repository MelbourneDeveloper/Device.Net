using System;

namespace Device.Net.Windows
{
    [Flags]
#pragma warning disable CA1028 
    public enum FileAccessRights : uint
#pragma warning restore CA1028 
    {
        None = 0,
        GenericRead = 2147483648,
        GenericWrite = 1073741824
    }
}
