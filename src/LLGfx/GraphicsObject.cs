using System;
using System.Collections.Generic;

namespace LLGfx
{
    public abstract class GraphicsObject : IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        protected T AddDisposable<T>(T disposable)
            where T : IDisposable
        {
            _disposables.Add(disposable);
            return disposable;
        }

        protected void AddDisposeAction(Action callback)
        {
            _disposables.Add(new ActionDisposable(callback));
        }

        private sealed class ActionDisposable : IDisposable
        {
            private readonly Action _action;

            public ActionDisposable(Action action)
            {
                _action = action;
            }

            public void Dispose()
            {
                _action();
            }
        }

        public void Dispose()
        {
            _disposables.Reverse();
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();

            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
