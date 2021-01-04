// ReSharper disable MemberCanBePrivate.Global
namespace Device.Net
{
    public class Trace
    {
        public Trace(bool isWrite, TransferResult transferResult)
        {
            IsWrite = isWrite;
            Data = transferResult;
        }

#pragma warning disable CA1819 // Properties should not return arrays
        public TransferResult Data { get; }
#pragma warning restore CA1819 // Properties should not return arrays
        public bool IsWrite { get; }

        public override string ToString() => $"{(IsWrite ? "Write" : "Read")} - Bytes transferred: {Data.BytesTransferred} - {string.Join(", ", Data)}";
    }
}
