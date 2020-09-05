using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.Reactive
{
    public static class DeviceObservableExtensions
    {
        public static IObservable<T> ToObservable<T>(this Func<Task<T>> func, TimeSpan interval = default, CancellationToken cancellationToken = default)
        {
            var observable = Observable.Create(
            (IObserver<T> observer) =>
            {
                //This keeps the task running until it is cancelled
                Task.Run(async () =>
                {
                    var delay = interval;

                    if (delay == default)
                    {
                        delay = TimeSpan.FromMilliseconds(5000);
                    }

                    while (true)
                    {
                        try
                        {
                            if (func != null)
                            {
                                observer.OnNext(await func());
                            }
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                        }

                        await Task.Delay(delay);
                    }
                }, cancellationToken);

                return Disposable.Create(() => { });
            });

            return observable;
        }

    }
}
