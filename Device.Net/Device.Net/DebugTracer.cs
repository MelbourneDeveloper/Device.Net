using System.Diagnostics;

namespace Device.Net
{
    public class DebugTracer : ITracer
    {
        public void Trace(bool isWrite, byte[] data)
        {
            Debug.WriteLine($"({string.Join(",", data)}) - {(isWrite ? "Write" : "Read")} ({data.Length})");
        }
    }
}
