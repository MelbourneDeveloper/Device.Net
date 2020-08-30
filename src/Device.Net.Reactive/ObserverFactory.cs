using System;
using System.Threading.Tasks;

namespace Device.Net.Reactive
{
    public class ObserverFactory<T> : IDisposable
    {
        private bool isRunning = true;
        private readonly Func<Task<T>> _func;

        public ObserverFactory(Func<Task<T>> func)
        {
            _func = func;
        }

        public void Dispose() => isRunning = false;

        public IDisposable Subscribe(IObserver<T> observer)
        {
            Task.Run(async () =>
            {
                while (isRunning)
                {
                    if (_func == null)
                    {
                        //TODO: Is this right?
                        await Task.Delay(1000);
                    }
                    else
                    {
                        var result = await _func();

                        //TODO: Is this right?
                        observer.OnNext(result);
                    }
                }
            });

            return this;
        }
    }
}
