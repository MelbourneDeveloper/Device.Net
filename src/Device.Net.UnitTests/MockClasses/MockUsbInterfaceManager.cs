using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Usb.Net;

namespace Device.Net.UnitTests
{
    public class MockUsbInterfaceManager : IUsbInterfaceManager
    {
        public MockUsbInterfaceManager(ILogger logger, ITracer tracer)
        {
            UsbInterfaceManager  = new Mock<UsbInterfaceManager>(new object[] { logger, tracer });
        }

        public Mock<UsbInterfaceManager> UsbInterfaceManager { get; } 

        public virtual IUsbInterface ReadUsbInterface
        {
            get

            {
                return UsbInterfaceManager.Object.ReadUsbInterface;
            }
            set
            {
            }
        }


        public virtual IUsbInterface WriteUsbInterface
        {
            get

            {
                return UsbInterfaceManager.Object.WriteUsbInterface;
            }
            set
            {
            }
        }

        public IList<IUsbInterface> UsbInterfaces => UsbInterfaceManager.Object.UsbInterfaces;

        public virtual ushort WriteBufferSize => WriteUsbInterface.WriteEndpoint.MaxPacketSize;

        public virtual ushort ReadBufferSize => ReadUsbInterface.ReadEndpoint.MaxPacketSize;

        public bool IsInitialized { get; private set; }

        public void Close()
        {

        }

        public void Dispose()
        {

        }

        public Task<ConnectedDeviceDefinitionBase> GetConnectedDeviceDefinitionAsync()
        {
            return Task.FromResult<ConnectedDeviceDefinitionBase>(new ConnectedDeviceDefinition(""));
        }

        public async Task InitializeAsync()
        {
            IsInitialized = true;
        }
    }
}
