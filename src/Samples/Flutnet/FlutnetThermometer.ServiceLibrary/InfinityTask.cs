using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FlutnetThermometer.ServiceLibrary
{
    /// <summary>
    /// This class provide a simple wrapper for workAction that have to be relaunched.
    /// </summary>
    public class InfinityTask
    {

        // Task in execution
        public Task GetTask => _task;

        // Job name: used for debugging
        private readonly string _name;

        // Reference for kill the task
        private CancellationTokenSource _cancellation;

        //
        // Job to do: can be Action or Func<Task>
        //
        private readonly dynamic _jobToDo;
        private readonly TimeSpan? _delay, _interval;

        private Task _task;

        public InfinityTask(string name, Func<Task> doFunction, TimeSpan? delay, TimeSpan? interval)
        {
            _name = name;
            _jobToDo = doFunction;
            _delay = delay;
            _interval = interval;
        }

        public InfinityTask(string name, Action doAction, TimeSpan? delay, TimeSpan? interval)
        {
            _name = name;
            _jobToDo = doAction;
            _delay = delay;
            _interval = interval;
        }

        // Needed to syncronized all the task call
        private readonly object _locker = new { };

        /// <summary>
        /// Start the current task.
        /// </summary>
        public void Start()
        {
            lock (_locker)
            {
                if (_task == null)
                {
                    Debug.WriteLine(_name + " Start.");
                    _cancellation = new CancellationTokenSource();
                    _task = RunTask(_name, _jobToDo, _delay, _interval, _cancellation);
                }
                else
                {
                    Debug.WriteLine(_name + " is already started!");
                }
            }
        }

        /// <summary>
        /// Cancel the execution about the current task.
        /// </summary>
        public void Cancel()
        {
            lock (_locker)
            {
                Debug.WriteLine(_name + " Cancel.");
                _cancellation?.Cancel();
                _task = null;
            }
        }

        /// <summary>
        /// Restart the task.
        /// </summary>
        public void Restart()
        {
            lock (_locker)
            {
                _cancellation?.Cancel();

                Debug.WriteLine(_name + " Restart.");
                _cancellation = new CancellationTokenSource();
                _task = RunTask(_name, _jobToDo, _delay, _interval, _cancellation);

            }
        }

        /// <summary>
        /// Generate the task starting it.
        /// </summary>
        /// <returns></returns>
        static async Task RunTask(string name, Func<Task> doFunction, TimeSpan? delay, TimeSpan? interval, CancellationTokenSource token)
        {

            // Wait the delay time if specified
            if (delay != null)
            {
                await Task.Delay(delay.Value);
            }

            // Job executed n times
            while (true)
            {

                // In case the workAction is cancelled
                if (token.Token.IsCancellationRequested)
                {
                    Debug.WriteLine(name + " is Canceled.");
                    token.Token.ThrowIfCancellationRequested();
                }

                //
                // Execute the workAction
                //
                await doFunction();

                // In case the workAction is cancelled
                if (token.Token.IsCancellationRequested)
                {
                    Debug.WriteLine(name + " is Canceled.");
                    token.Token.ThrowIfCancellationRequested();
                }

                if (interval != null)
                {
                    await Task.Delay(interval.Value);
                }

            }

        }

        /// <summary>
        /// Generate the task starting it.
        /// </summary>
        /// <returns></returns>
        static async Task RunTask(string name, Action doAction, TimeSpan? delay, TimeSpan? interval, CancellationTokenSource token)
        {

            // Wait the delay time if specified
            if (delay != null)
            {
                await Task.Delay(delay.Value);
            }

            // Job executed n times
            while (true)
            {

                // In case the workAction is cancelled
                if (token.Token.IsCancellationRequested)
                {
                    Debug.WriteLine(name + " is Canceled.");
                    token.Token.ThrowIfCancellationRequested();
                }

                //
                // Execute the workAction
                //
                if (doAction != null)
                    await Task.Run(doAction);

                // In case the workAction is cancelled
                if (token.Token.IsCancellationRequested)
                {
                    Debug.WriteLine(name + " is Canceled.");
                    token.Token.ThrowIfCancellationRequested();
                }

                if (interval != null)
                {
                    await Task.Delay(interval.Value);
                }

            }

        }

    }
}
