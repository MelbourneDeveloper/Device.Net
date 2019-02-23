using Device.Net;
using System.Threading.Tasks;

namespace Hid.Net
{
    public interface IHidDevice : IDevice
    {
        Task WriteAsync(byte[] data, byte reportId);
    }
}
