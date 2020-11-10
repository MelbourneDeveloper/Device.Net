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
         ushort value,
         ushort index,
         ushort length
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
