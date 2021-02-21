using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Device.Net
{
    public class FactoryBuilder : IFactoryBuilder
    {
        public ILoggerFactory LoggerFactory { get; }
        public ReadOnlyCollection<IDeviceFactory> Factories { get; }

        public FactoryBuilder(ILoggerFactory loggerFactory = null) => LoggerFactory = loggerFactory;

        public FactoryBuilder(IDeviceFactory deviceFactory, ILoggerFactory loggerFactory = null)
            : this(loggerFactory)
        {
            if (deviceFactory != null)
            {
                Factories = new ReadOnlyCollection<IDeviceFactory>(new IDeviceFactory[] { deviceFactory });
            }
        }

        public FactoryBuilder(IList<IDeviceFactory> deviceFactories, ILoggerFactory loggerFactory = null)
            : this(loggerFactory)
        {
            if (deviceFactories != null)
            {
                Factories = new ReadOnlyCollection<IDeviceFactory>(deviceFactories);
            }
        }

        public IDeviceFactory Build() => Factories.Aggregate();

    }
}
