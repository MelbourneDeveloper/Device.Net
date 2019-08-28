
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Device.Net.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CommTimeouts
    {
        [SuppressMessage("NDepend", "ND1905:AFieldMustNotBeAssignedFromOutsideItsParentHierarchyTypes", Justification = "...")]
        public uint ReadIntervalTimeout;
        [SuppressMessage("NDepend", "ND1905:AFieldMustNotBeAssignedFromOutsideItsParentHierarchyTypes", Justification = "...")]
        public uint ReadTotalTimeoutMultiplier;
        [SuppressMessage("NDepend", "ND1905:AFieldMustNotBeAssignedFromOutsideItsParentHierarchyTypes", Justification = "...")]
        public uint ReadTotalTimeoutConstant;
        [SuppressMessage("NDepend", "ND1905:AFieldMustNotBeAssignedFromOutsideItsParentHierarchyTypes", Justification = "...")]
        public uint WriteTotalTimeoutMultiplier;
        [SuppressMessage("NDepend", "ND1905:AFieldMustNotBeAssignedFromOutsideItsParentHierarchyTypes", Justification = "...")]
        public uint WriteTotalTimeoutConstant;
    }
}