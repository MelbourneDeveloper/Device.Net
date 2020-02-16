using System;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.UnitTests
{
    public abstract class MockDeviceBase : DeviceBase, IDevice
    {
        public override ushort WriteBufferSize => 64;
        public override ushort ReadBufferSize => 64;

        protected bool _IsInitialized;

        public override bool IsInitialized => _IsInitialized;

        protected MockDeviceBase(string deviceId, ILogger logger, ITracer tracer) : base(deviceId, logger, tracer)
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

        public async override Task<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            if (LastWrittenBuffer != null)
            {
                Tracer?.Trace(false, LastWrittenBuffer);
                return LastWrittenBuffer;
            }
            var data = new byte[] { 1, 2, 3, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Tracer?.Trace(false, data);

            //Wait a possible cancellation
            for (var i = 0; i < 5000; i++)
            {
                await Task.Delay(1);
                if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException();
            }

            return data;
        }

        public async override Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            LastWrittenBuffer = data;
            Tracer?.Trace(true, data);
            //Wait a possible cancellation
            for (var i = 0; i < 5000; i++)
            {
                await Task.Delay(1);
                if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException();
            }
        }
    }
}
