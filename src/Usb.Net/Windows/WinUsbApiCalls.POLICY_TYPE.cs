namespace Usb.Net.Windows
{
    public static partial class WinUsbApiCalls
    {
        public enum POLICY_TYPE
        {
            SHORT_PACKET_TERMINATE = 1,
            AUTO_CLEAR_STALL,
            PIPE_TRANSFER_TIMEOUT,
            IGNORE_SHORT_PACKETS,
            ALLOW_PARTIAL_READS,
            AUTO_FLUSH,
            RAW_IO
        }
    }
}
