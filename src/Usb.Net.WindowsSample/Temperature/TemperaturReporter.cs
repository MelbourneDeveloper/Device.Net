using System;

namespace Usb.Net.WindowsSample.Temperature
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/standard/events/how-to-implement-an-observer#example
    /// </summary>
    public class TemperatureReporter : IObserver<Temperature>
    {
        private IDisposable unsubscriber;
        private bool first = true;
        private Temperature last;

        public virtual void Subscribe(IObservable<Temperature> provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe() => unsubscriber.Dispose();

        public virtual void OnCompleted() => Console.WriteLine("Additional temperature data will not be transmitted.");

#pragma warning disable CA1716 // Identifiers should not match keywords
        public virtual void OnError(Exception error)
#pragma warning restore CA1716 // Identifiers should not match keywords
        {
            // Do nothing.
        }

        public virtual void OnNext(Temperature value)
        {
            if (value == null) return;

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
