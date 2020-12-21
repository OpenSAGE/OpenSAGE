// Ported from SharpDX:
//
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

namespace OpenSage
{
    /// <summary>
    /// Base class for objects that implement <see cref="IDisposable"/>.
    /// Provides utility functions to easily disposable of child objects.
    /// </summary>
    public abstract class DisposableBase : IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        protected internal bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is currently being disposed.
        /// </summary>
        protected internal bool IsDisposing { get; private set; }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposing = true;
                Dispose(true);
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Disposes of object resources.
        /// </summary>
        /// <param name="disposeManagedResources">If true, managed resources should be
        /// disposed of in addition to unmanaged resources.</param>
        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                for (var i = _disposables.Count - 1; i >= 0; i--)
                {
                    _disposables[i].Dispose();
                }
            }
        }

        /// <summary>
        /// Adds a disposable object to the list of the objects to dispose.
        /// </summary>
        /// <param name="toDisposeArg">To dispose.</param>
        protected internal T AddDisposable<T>(T toDisposeArg)
            where T : IDisposable
        {
            if (!ReferenceEquals(toDisposeArg, null))
            {
                _disposables.Add(toDisposeArg);
            }

            return toDisposeArg;
        }

        // TODO: Might not need this.
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

        /// <summary>
        /// Dispose a disposable object and set the reference to null. 
        /// Removes this object from the ToDispose list.
        /// </summary>
        /// <param name="objectToDispose">Object to dispose.</param>
        protected internal void RemoveAndDispose<T>(ref T objectToDispose)
            where T : class, IDisposable
        {
            if (!ReferenceEquals(objectToDispose, null))
            {
                _disposables.Remove(objectToDispose);
                objectToDispose.Dispose();

                objectToDispose = null;
            }
        }

        /// <summary>
        /// Removes a disposable object to the list of the objects to dispose.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toDisposeArg">To dispose.</param>
        protected internal void RemoveToDispose<T>(T toDisposeArg)
            where T : IDisposable
        {
            if (!ReferenceEquals(toDisposeArg, null))
            {
                _disposables.Remove(toDisposeArg);
            }
        }
    }
}
