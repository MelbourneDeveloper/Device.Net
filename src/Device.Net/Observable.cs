using System;
using System.Collections.Generic;

namespace Device.Net
{
    public class Observable<T> : IObservable<T>
    {
        private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            Locked(() => _observers.Add(observer));
            return new UnsubscribeDisposable(() => Unsubscribe(observer));
        }

        private void Locked(Action action)
        {
            lock (_observers) action();
        }

        internal void Unsubscribe(IObserver<T> observer) => Locked(() => _observers.Remove(observer));

        public void OnNext(T item) => Locked(() => _observers.ForEach(o => o.OnNext(item)));
        public void OnCompleted() => Locked(() => _observers.ForEach(o => o.OnCompleted()));
        public void OnError(Exception exception) => Locked(() => _observers.ForEach(o => o.OnError(exception)));
    }
}
