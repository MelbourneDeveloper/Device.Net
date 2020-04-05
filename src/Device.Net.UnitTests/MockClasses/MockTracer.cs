using System;

namespace Device.Net.UnitTests
{
    public class MockTracer : ITracer
    {
        public int WriteCount { get; private set; }
        public int ReadCount { get; private set; }

        public void Trace(bool isWrite, byte[] data)
        {
            if (data == null) throw new Exception("data must not be null");
            if (isWrite) WriteCount++;
            if (!isWrite) ReadCount++;
        }
    }
}
