using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UnoCrossPlatform
{
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    public class AsyncCommand : ICommand
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
    {
        #region Events
        public event EventHandler? CanExecuteChanged;
        #endregion

        #region Fields
        private bool _canExecute = true;
        private readonly ILogger<AsyncCommand> _logger;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly Func<object, Task> _asyncCommandDelegate;
        #endregion

        #region Public Properties
        public bool CanExecute(object parameter) => _canExecute;
        #endregion

        #region Constructor
        public AsyncCommand(
            Func<object, Task> asyncCommandDelegate,
            ILoggerFactory loggerFactory)
        {
            _asyncCommandDelegate = asyncCommandDelegate;
            _logger = loggerFactory.CreateLogger<AsyncCommand>();
        }
        #endregion

        #region Public Methods
        public void Execute(object parameter)
        {
            //This shouldn't be necessary but the UI may not honour the flag
            if (!_canExecute) return;

            _ = PerformActionAsync(parameter);
        }
        #endregion

        #region Private Methods
        private async Task PerformActionAsync(object parameter)
        {
            try
            {
                await _semaphoreSlim.WaitAsync();

                SetCanExecute(false);

                //TODO: put a configurable timeout on this

                await _asyncCommandDelegate(parameter);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                //TODO: better logging
                _logger?.LogError(ex, "Command error");
            }
            finally
            {
                _ = _semaphoreSlim.Release();
                SetCanExecute(true);
            }
        }

        private void SetCanExecute(bool canExecute)
        {
            _canExecute = canExecute;
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}
