using System;

namespace Device.Net
{
    /// <summary>
    /// This class is a fill in for the Reactive extensions so that we don't need to depend on those at the low level
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Observer<T> : IObserver<T>
    {
        private readonly Action<T> _onNext;
        private readonly Action<Exception> _onError;
        private readonly Action _onCompleted;

        public Observer(
        Action<T> onNext,
        Action<Exception> onError = null,
        Action onCompleted = null
            )
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
        }


        public void OnCompleted() => _onCompleted();
        public void OnError(Exception error) => _onError(error);
        public void OnNext(T value) => _onNext(value);
    }
}
