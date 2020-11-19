// ReSharper disable MemberCanBePrivate.Global
namespace Device.Net
{
    public class Trace
    {
        public Trace(bool isWrite, byte[] data)
        {
            IsWrite = isWrite;
            Data = data;
        }

#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Data { get; }
#pragma warning restore CA1819 // Properties should not return arrays
        public bool IsWrite { get; }

        public override string ToString() => $"{(IsWrite ? "Write" : "Read")} - {string.Join(", ", Data)}";
    }
}
