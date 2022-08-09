using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Device.Net
{
    public class FactoryBuilder : IFactoryBuilder
    {
        public ILoggerFactory LoggerFactory { get; }
        public ReadOnlyCollection<IDeviceFactory> Factories { get; }

        public FactoryBuilder(ILoggerFactory loggerFactory = null)
        {
            LoggerFactory = loggerFactory;
            Factories = new List<IDeviceFactory>().AsReadOnly();
        }

        public FactoryBuilder(IDeviceFactory deviceFactory, ILoggerFactory loggerFactory = null)
        {
            LoggerFactory = loggerFactory;
            Factories = deviceFactory == null
                ? new List<IDeviceFactory>().AsReadOnly()
                : new ReadOnlyCollection<IDeviceFactory>(new IDeviceFactory[] { deviceFactory });
        }

        public FactoryBuilder(IList<IDeviceFactory> deviceFactories, ILoggerFactory loggerFactory = null)
        {
            LoggerFactory = loggerFactory;
            Factories = deviceFactories == null
                ? new List<IDeviceFactory>().AsReadOnly()
                : new ReadOnlyCollection<IDeviceFactory>(deviceFactories);
        }

        public IDeviceFactory Build() => Factories.Aggregate();
    }
}
