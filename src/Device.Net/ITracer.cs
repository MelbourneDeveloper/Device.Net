namespace Device.Net
{
    public interface ITracer
    {
        void Trace(bool isWrite, byte[] data);
    }
}