using Device.Net;
using System.Threading.Tasks;

namespace Usb.Net.WindowsSample
{
    public delegate Task<IDevice> GetDevice(string name);

}
