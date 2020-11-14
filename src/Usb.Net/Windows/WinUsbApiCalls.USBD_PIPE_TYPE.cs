// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Usb.Net.Windows
{
    public static partial class WinUsbApiCalls
    {
        public enum USBD_PIPE_TYPE
        {
            UsbdPipeTypeControl,
            UsbdPipeTypeIsochronous,
            UsbdPipeTypeBulk,
            UsbdPipeTypeInterrupt
        }
    }
}
