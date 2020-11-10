namespace Usb.Net
{
    public class SetupPacket
    {
        #region Public Properties
        public byte RequestType { get; }
        public byte Request { get; }
        public ushort Value { get; }
        public ushort Index { get; }
        public ushort Length { get; }
        #endregion

        #region Constructors
        public SetupPacket
            (
         byte requestType,
         byte request,
         ushort value = 0,
         ushort index = 0,
         ushort length = 0
            )
        {
            RequestType = requestType;
            Request = request;
            Value = value;
            Index = index;
            Length = length;
        }
        #endregion

    }
}
