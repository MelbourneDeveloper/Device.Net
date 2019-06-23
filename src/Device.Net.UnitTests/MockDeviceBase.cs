using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    public abstract class MockDeviceBase : DeviceBase, IDevice
    {
        public override ushort WriteBufferSize => 64;
        public override ushort ReadBufferSize => 64;

        protected bool _IsInitialized;

        public override bool IsInitialized => _IsInitialized;

        protected MockDeviceBase(ILogger logger, ITracer tracer) : base(logger, tracer)
        {

        }

        public void Close()
        {
        }

        public Task InitializeAsync()
        {
            _IsInitialized = true;
            return Task.FromResult(true);
        }

        private byte[] LastWrittenBuffer;

        public override async Task<byte[]> ReadAsync()
        {
            if (LastWrittenBuffer != null)
            {
                Tracer.Trace(false, LastWrittenBuffer);
                return LastWrittenBuffer;
            }
            var data = new byte[64] { 1, 2, 3, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Tracer.Trace(false, data);
            return await Task.FromResult(data);
        }

        public override Task WriteAsync(byte[] data)
        {
            LastWrittenBuffer = data;
            Tracer.Trace(true, data);
            return Task.FromResult(true);
        }
    }
}
