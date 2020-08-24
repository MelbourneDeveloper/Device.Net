# if !NET45

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace CallbackLogger
{
    public class DelegateLogger : ILogger
    {
        #region Fields
        private readonly Action<LogMessage> _action;
        private readonly ConcurrentDictionary<Type, DelegateLoggerScope> _currentScopes = new ConcurrentDictionary<Type, DelegateLoggerScope>();
        #endregion

        #region Constructor
        public DelegateLogger(Action<LogMessage> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }
        #endregion

        #region Implementation
        public IDisposable BeginScope<TState>(TState state)
        {
            //if (_currentScopes.ContainsKey(typeof(TState)))
            //{
            //    throw new ScopeExistsException($"Scope already exists for type of {typeof(TState)}");
            //}

            var scope = GetScope(state);

            Callback(null, default, state, null, true);
            return scope;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (_currentScopes.ContainsKey(typeof(TState)))
            {
                var delegateLoggerScope = (DelegateLoggerScope<TState>)_currentScopes[typeof(TState)];
                Callback(null, default, delegateLoggerScope.State, null, true);
            }

            Callback(logLevel, eventId, state, exception, false);
        }
        #endregion

        #region Non Public Methods

        //internal void ReleaseScope<T>() => _currentScopes.Remove(typeof(T), out _);

#pragma warning disable CA1822 // Mark members as static
        internal void ReleaseScope<T>()
#pragma warning restore CA1822 // Mark members as static
        {

        }


        private void Callback<TState>(LogLevel? logLevel, EventId eventId, TState state, Exception? exception, bool isScope)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            _action(new LogMessage(logLevel, eventId, state, exception, isScope, this));
        }

        private DelegateLoggerScope<TState> GetScope<TState>(TState state) => (DelegateLoggerScope<TState>)_currentScopes.GetOrAdd(typeof(TState), (t) => new DelegateLoggerScope<TState>(_action, state, this));
        #endregion
    }
}

#endif