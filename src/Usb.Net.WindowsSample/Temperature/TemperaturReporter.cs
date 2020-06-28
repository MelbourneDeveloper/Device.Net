using System;

namespace Usb.Net.WindowsSample.Temperature
{
    public partial class TemperatureReporter : IObserver<Temperature>
    {
        private IDisposable unsubscriber;
        private bool first = true;
        private Temperature last;

        public virtual void Subscribe(IObservable<Temperature> provider)
        {
            unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }

        public virtual void OnCompleted()
        {
            Console.WriteLine("Additional temperature data will not be transmitted.");
        }

        public virtual void OnError(Exception error)
        {
            // Do nothing.
        }

        public virtual void OnNext(Temperature value)
        {
            Console.WriteLine("The temperature is {0}°C at {1:g}", value.Degrees, value.Date);
            if (first)
            {
                last = value;
                first = false;
            }
            else
            {
                Console.WriteLine("   Change: {0}° in {1:g}", value.Degrees - last.Degrees,
                                                              value.Date.ToUniversalTime() - last.Date.ToUniversalTime());
            }
        }
    }
}
