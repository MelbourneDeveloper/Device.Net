using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Net.Reactive
{
    public static class DeviceObservableExtensions
    {
        public static IObservable<T> ToObservable<T>(this Func<Task<T>> func, TimeSpan interval, CancellationToken cancellationToken)
        {
            var observable = Observable.Create(
            (IObserver<T> observer) =>
            {
                Task.Run(async () =>
                {
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

                        Thread.Sleep(interval);
                    }
                }, cancellationToken);

                return Disposable.Create(() => { });
            });

            return observable;
        }

    }
}
