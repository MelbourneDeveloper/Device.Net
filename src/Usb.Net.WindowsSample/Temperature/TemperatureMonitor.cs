using System;
using System.Collections.Generic;

namespace Usb.Net.WindowsSample.Temperature
{

    public partial class TemperatureReporter
    {
        public class TemperatureMonitor : IObservable<Temperature>
        {
            readonly List<IObserver<Temperature>> observers;

            public TemperatureMonitor()
            {
                observers = new List<IObserver<Temperature>>();
            }

            private class Unsubscriber : IDisposable
            {
                private readonly List<IObserver<Temperature>> _observers;
                private readonly IObserver<Temperature> _observer;

                public Unsubscriber(List<IObserver<Temperature>> observers, IObserver<Temperature> observer)
                {
                    _observers = observers;
                    _observer = observer;
                }

                public void Dispose()
                {
                    if (!(_observer == null)) _observers.Remove(_observer);
                }
            }

            public IDisposable Subscribe(IObserver<Temperature> observer)
            {
                if (!observers.Contains(observer))
                    observers.Add(observer);

                return new Unsubscriber(observers, observer);
            }

            public void GetTemperature()
            {
                // Create an array of sample data to mimic a temperature device.
                decimal?[] temps = {14.6m, 14.65m, 14.7m, 14.9m, 14.9m, 15.2m, 15.25m, 15.2m,
                                   15.4m, 15.45m, null };
                // Store the previous temperature, so notification is only sent after at least .1 change.
                decimal? previous = null;
                bool start = true;

                foreach (var temp in temps)
                {
                    System.Threading.Thread.Sleep(2500);
                    if (temp.HasValue)
                    {
                        if (start || (Math.Abs(temp.Value - previous.Value) >= 0.1m))
                        {
                            Temperature tempData = new Temperature(temp.Value, DateTime.Now);
                            foreach (var observer in observers)
                                observer.OnNext(tempData);
                            previous = temp;
                            if (start) start = false;
                        }
                    }
                    else
                    {
                        foreach (var observer in observers.ToArray())
                            if (observer != null) observer.OnCompleted();

                        observers.Clear();
                        break;
                    }
                }
            }
        }
    }
}
