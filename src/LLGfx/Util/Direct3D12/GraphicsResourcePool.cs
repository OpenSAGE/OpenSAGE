using System;
using System.Collections.Generic;

namespace LLGfx.Util
{
    internal abstract class GraphicsResourcePool<T> : GraphicsObject
        where T : class, IDisposable
    {
        private readonly List<T> _resourcePool;
        private readonly Queue<Tuple<long, T>> _readyResources;
        private readonly object _lockObject = new object();

        protected GraphicsDevice GraphicsDevice { get; }

        public GraphicsResourcePool(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;

            _resourcePool = new List<T>();
            _readyResources = new Queue<Tuple<long, T>>();
        }

        protected abstract T CreateResource();

        protected abstract void ResetResource(T resource);

        public T AcquireResource(long completedFenceValue)
        {
            lock (_lockObject)
            {
                T resource = null;

                // Try to find available resource.
                if (_readyResources.Count > 0)
                {
                    var resourceTuple = _readyResources.Peek();

                    if (resourceTuple.Item1 <= completedFenceValue)
                    {
                        resource = resourceTuple.Item2;
                        ResetResource(resource);
                        _readyResources.Dequeue();
                    }
                }

                // Otherwise, create a new one.
                if (resource == null)
                {
                    resource = CreateResource();
                    _resourcePool.Add(resource);
                }

                return resource;
            }
        }

        public void ReleaseResource(long fenceValue, T resource)
        {
            lock (_lockObject)
            {
                _readyResources.Enqueue(Tuple.Create(fenceValue, resource));
            }
        }

        // TODO: When resources haven't been used for a few frames, dispose of them.

        protected override void Dispose(bool disposing)
        {
            // TODO: Don't dispose of resources that are still live.
            // Instead, schedule them for disposal.
            foreach (var resourceEntry in _readyResources)
            {
                resourceEntry.Item2.Dispose();
            }
            _readyResources.Clear();

            base.Dispose(disposing);
        }
    }
}
