using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSage.Scripting
{
    // From Protogame, licenced under the MIT licence:
    // https://github.com/RedpointGames/Protogame
    public sealed class Coroutine
    {
        private readonly CoroutineScheduler _coroutineScheduler;
        private readonly Game _game;

        public Coroutine(CoroutineScheduler coroutineScheduler, Game game)
        {
            _coroutineScheduler = coroutineScheduler;
            _game = game;
        }

        private async Task WrapCoroutine(Func<ScriptExecutionContext, Task> coroutine)
        {
            await Task.Yield();

            await coroutine(new ScriptExecutionContext(_game));
        }

        private async Task<T> WrapCoroutine<T>(Func<Task<T>> coroutine)
        {
            await Task.Yield();
            return await coroutine();
        }

        public Task Run(Func<ScriptExecutionContext, Task> coroutine)
        {
            var oldContext = SynchronizationContext.Current;
            try
            {
                var syncContext = (SynchronizationContext) _coroutineScheduler;
                SynchronizationContext.SetSynchronizationContext(syncContext);
                // The await inside WrapCoroutine will cause it to be placed on the coroutine scheduler.
                var task = WrapCoroutine(coroutine);
                return task;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }
        }

        public Task<T> Run<T>(Func<Task<T>> coroutine)
        {
            var oldContext = SynchronizationContext.Current;
            try
            {
                var syncContext = (SynchronizationContext) _coroutineScheduler;
                SynchronizationContext.SetSynchronizationContext(syncContext);
                // The await inside WrapCoroutine will cause it to be placed on the coroutine scheduler.
                var task = WrapCoroutine(coroutine);
                return task;
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(oldContext);
            }
        }
    }
}
