using System;
using System.Collections.Generic;

namespace OpenZH.Graphics
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

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
