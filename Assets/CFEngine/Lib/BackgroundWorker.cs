using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CrystalFrost.Lib
{
    /// <summary>
    /// a base class for things that run on repeat in the background.
    /// its not threads, its tasks, It scales the concurrency up automatically
    /// based on logical processor count (CPU Cores)
    /// </summary>
    public abstract class BackgroundWorker : IDisposable
    {
        private readonly string _name;
        protected readonly ILogger _log;
        private readonly IProvideShutdownSignal _runningIndicator;
        private bool _hasShutdown = false;
        private int _concurrencyCount = 0;
        private readonly int _targetConcurrency = 1;
        private readonly SemaphoreSlim semaphore = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundWorker"/> class.
        /// </summary>
        /// <param name="name">The name of the worker, used for logging.</param>
        /// <param name="targetConcurrency">The desired number of concurrent tasks.</param>
        /// <param name="log">The logger for recording messages.</param>
        /// <param name="runningIndicator">The provider for shutdown signals.</param>
        protected BackgroundWorker(
            string name,
			int targetConcurrency,
            ILogger log,
            IProvideShutdownSignal runningIndicator)
        {
            _name = name;
            _log = log;
            _runningIndicator = runningIndicator;
            _runningIndicator.OnShutdown += ShutdownSignaled;
			_targetConcurrency = targetConcurrency == 0 ? Math.Max(Environment.ProcessorCount / 2, 1) : targetConcurrency;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="BackgroundWorker"/> object.
        /// </summary>
        public virtual void Dispose()
        {
            _runningIndicator.OnShutdown -= ShutdownSignaled;
            GC.SuppressFinalize(this);
        }

        private void ShutdownSignaled()
        {
            _hasShutdown = true;
            ShuttingDown();
        }

        /// <summary>
        /// Descendant classes can override this to
        /// preform any shutdown tasks.
        /// </summary>
        protected virtual void ShuttingDown() { }

        /// <summary>
        /// Descendant class should call.
        /// Will start a task if not at concurrency and output is not backlogged.
        /// </summary>
        protected void CheckForWork()
        {
            if (_hasShutdown) return;
            semaphore.Wait();
            try
            {
                if (_concurrencyCount >= _targetConcurrency) return;
                if (OutputIsBacklogged()) return;
                _concurrencyCount++;
                _log.BackgroundWorkerConcurrency(_name, _concurrencyCount);
                // start a task, and when it completes, call ContinueWork.
                _ = Task.Run(() => TryDoWork()).ContinueWith(ContinuteWork);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private void ContinuteWork(Task<bool> completed)
        {
            // when a task complets, this code runs.
            semaphore.Wait();
            try
            {
                if (completed.Result && !_hasShutdown)
                {
                    // if the task completed, and we haven't shut down,
                    // try to do more work.
                    _ = Task.Run(() => TryDoWork()).ContinueWith(ContinuteWork);
                }
                else
                {
                    // either the task signaled there was no more work,
                    // or we are shutting down, so we're done.
                    // concurrrency is going down.
                    _concurrencyCount--;
                    _log.BackgroundWorkerConcurrency(_name, _concurrencyCount);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task<bool> TryDoWork()
        {
            try
            {
                return await DoWork();
            }
            catch (Exception ex)
            {
                _log.BackgroundWorkerTaskFailed(_name, ex);
                // tough choice: should it:
                // return true and retry, but maybe infinite loop?
                // return false, and maybe not finish the pending work?
                // lets go with true, because the descendant class can be
                // programmed to catch exceptions and decide for itself
                // what the right thing to do is.
                return true;
            }
        }

        /// <summary>
        /// Descendant class must implement.
        /// If this throws and the unit of work has not been removed
        /// from the queue, then it will be retried. So always dequeue,
        /// and try to not throw.
        /// </summary>
        /// <returns>Return True indicate completion.</returns>
        protected abstract Task<bool> DoWork();

        /// <summary>
        /// Descendant class must implement.
        /// Return true to signal that this worker should wait.
        /// </summary>
        /// <returns></returns>
        protected abstract bool OutputIsBacklogged();
    }
}
