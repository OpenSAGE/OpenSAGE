using System;
using System.Collections.Generic;

namespace OpenZH.Graphics.LowLevel
{
    public abstract class GraphicsObject : IDisposable
    {
        private List<IDisposable> _disposables;

        protected T AddDisposable<T>(T disposable)
            where T : IDisposable
        {
            if (_disposables == null)
            {
                _disposables = new List<IDisposable>();
            }

            _disposables.Add(disposable);

            return disposable;
        }

        public void Dispose()
        {
            if (_disposables != null)
            {
                foreach (var disposable in _disposables)
                {
                    disposable.Dispose();
                }
            }

            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
