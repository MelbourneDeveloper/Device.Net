using System.Diagnostics;

namespace Device.Net
{
    public class DebugTracer : ITracer
    {
        public void Trace(bool isWrite, byte[] data)
        {
            Debug.WriteLine($"{(isWrite ? "Write" : "Read")}: ({string.Join(",", data)}) ({data.Length})");
        }
    }
}
