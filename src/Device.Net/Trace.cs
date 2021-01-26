// ReSharper disable MemberCanBePrivate.Global
namespace Device.Net
{
    public class Trace
    {
        public Trace(bool isWrite, TransferResult transferResult)
        {
            IsWrite = isWrite;
            TransferResult = transferResult;
        }

#pragma warning disable CA1819 // Properties should not return arrays
        public TransferResult TransferResult { get; }
#pragma warning restore CA1819 // Properties should not return arrays
        public bool IsWrite { get; }

        public override string ToString() => $"Physical {(IsWrite ? "Write" : "Read")} - {TransferResult}";
    }
}
