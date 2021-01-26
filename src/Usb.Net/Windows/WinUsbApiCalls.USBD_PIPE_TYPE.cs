// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace Usb.Net.Windows
{
    internal enum USBD_PIPE_TYPE
    {
        UsbdPipeTypeControl,
        UsbdPipeTypeIsochronous,
        UsbdPipeTypeBulk,
        UsbdPipeTypeInterrupt
    }
}

#pragma warning restore CA1707 // Identifiers should not contain underscores
