using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Rendering
{
    public sealed class RenderItemCollection
    {
        // TODO: Should this just be 0? Or somewhere in the hundreds?
        // A map usually has thousands of objects.
        private const int PreAllocatedItems = 128;
        private const double GrowthFactor = 1.5;

        // The backing storage for render items. 
        private RenderItem[] _items;

        // TODO: Bounds check?
        public ref RenderItem this[int i] => ref _items[i];

        // An array of flags indicating if a render item should be included in the culling set.
        // Must have the same capacity and length as _items.
        private bool[] _culled;

        // Number of render items in the collection.
        public int Length { get; private set; }

        // Sorted indices of objects that were chosen to be rendered.
        // Indices refer to _items.
        private readonly List<int> _culledItemIndices;
        public IReadOnlyList<int> CulledItemIndices => _culledItemIndices;

        public RenderItemCollection()
        {
            _culledItemIndices = new List<int>();
            _items = new RenderItem[PreAllocatedItems];
            _culled = new bool[PreAllocatedItems];
            Length = 0;
        }

        public void Add(RenderItem item)
        {
            if (Length == _items.Length)
            {
                var newCapacity = (int) ((Length + 1) * GrowthFactor);
                Array.Resize(ref _items, newCapacity);
                Array.Resize(ref _culled, newCapacity);
            }

            _items[Length++] = item;
        }

        // Performs frustum culling for a single render item in _items.
        // Increments the provided integer reference if the object was within the frustum.
        private void Cull(int i, in BoundingFrustum cameraFrustum)
        {
            _culled[i] = cameraFrustum.Intersects(_items[i].BoundingBox);
        }

        public void CullAndSort(in BoundingFrustum cameraFrustum, int batchSize)
        {
            if (Length == 0)
            {
                return;
            }

            // Step 1: Compute visibility for each item in _items and store the result _culled.

            // TODO: If Length <= batchSize, don't call Parallel.ForEach
            if (batchSize == -1)
            {
                // Perform culling in the main thread.
                for (var i = 0; i < Length; i++)
                {
                    Cull(i, cameraFrustum);
                }
            }
            else
            {
                // We need a copy of cameraFrustum, as we can't send in parameters to closures. 
                var frustum = cameraFrustum;

                // Perform culling using the thread pool, in batches of batchSize.
                Parallel.ForEach(Partitioner.Create(0, Length, batchSize), range =>
                {
                    var (start, end) = range;
                    for (var i = start; i < end; i++)
                    {
                        Cull(i, frustum);
                    }
                });
            }

            // Step 2: Go through _culled, and store the indices of culled values in _resultIndices.
            // Also count the number of culled render items.
            
            for (var i = 0; i < Length; i++)
            {
                if (_culled[i])
                {
                    _culledItemIndices.Add(i);
                }
            }

            // Step 3: Sort the indices by comparing render item keys.
            // If two items have the same key, compare the indices in
            // order to provider a consistent ordering and prevent flickering.
            _culledItemIndices.Sort((a, b) =>
            {
                var result = _items[a].Key.CompareTo(_items[b].Key);
                if (result == 0)
                {
                    return a.CompareTo(b);
                }

                return result;
            });
        }

        public void Clear()
        {
            Length = 0;
            _culledItemIndices.Clear();

            // TODO: Should we provide a different method for actually clearing the item buffer?
            // Otherwise there might be memory leaks when switching between scenes.
        }
    }
}
