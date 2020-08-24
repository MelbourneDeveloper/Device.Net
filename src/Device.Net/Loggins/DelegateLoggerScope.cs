# if !NET45
using System;

namespace CallbackLogger
{
    internal class DelegateLoggerScope
    {
        protected Action<LogMessage> _action;
        protected DelegateLogger _DelegateLogger;

        public DelegateLoggerScope(
        Action<LogMessage> action,
        DelegateLogger delegateLogger
        )
        {
            _action = action;
            _DelegateLogger = delegateLogger ?? throw new ArgumentNullException(nameof(delegateLogger));
        }

    }

    internal class DelegateLoggerScope<TState> : DelegateLoggerScope, IDisposable
    {
        public TState State { get; private set; }

        public DelegateLoggerScope(
            Action<LogMessage> action,
            TState state,
            DelegateLogger delegateLogger
            ) : base(action, delegateLogger)
        {
            if (delegateLogger == null) throw new ArgumentNullException(nameof(delegateLogger));

            State = state;
        }

        public void Dispose()
        {
            //_DelegateLogger.ReleaseScope<TState>();
            _action = null;
            _DelegateLogger = null;
            State = default;
        }
    }
}
#endif