using System.Collections.Generic;
using System.Linq;
using OpenSage.Mathematics;

namespace OpenSage.DataStructures
{
    public sealed class Quadtree<T> where T : IHasBounds
    {
        private const int MaxDepth = 8;
        private const int MaxItemsPerLeaf = 2;

        public readonly RectangleF Bounds;

        // If this is a leaf, this array is null.
        // If this a tree node, this array is initialized
        private Quadtree<T>[] _children;

        private readonly HashSet<T> _items = new HashSet<T>();

        private readonly int _depth;

        private bool IsLeaf => _children == null;
        private bool ReachedItemLimit => _items.Count >= MaxItemsPerLeaf;
        private bool ReachedDepthLimit => _depth >= MaxDepth;
        private bool IsEmpty => _children == null && _items.Count == 0;

        public Quadtree(RectangleF bounds)
        {
            Bounds = bounds;
            _depth = 0;
        }

        private Quadtree(in RectangleF parentBounds, Quad quad, int depth)
        {
            var halfWidth = parentBounds.Width / 2.0f;
            var halfHeight = parentBounds.Height / 2.0f;

            var position = parentBounds.Position;

            switch (quad)
            {
                case Quad.LowerLeft:
                    break;
                case Quad.LowerRight:
                    position.X += halfWidth;
                    break;
                case Quad.UpperLeft:
                    position.Y += halfHeight;
                    break;
                case Quad.UpperRight:
                    position.X += halfWidth;
                    position.Y += halfHeight;
                    break;
            }

            Bounds = new RectangleF(position, halfWidth, halfHeight);
            _depth = depth;
        }

        public IEnumerable<T> FindIntersecting(in RectangleF bounds)
        {
            return !bounds.IntersectsWith(Bounds) ? Enumerable.Empty<T>() : FindIntersectingInternal(bounds);
        }

        // Iterators cannot have in parameters, so we must copy the rectangle :(
        private IEnumerable<T> FindIntersectingInternal(RectangleF bounds)
        {
            if (_children != null)
            {
                foreach (var subtree in _children)
                {
                    if (subtree.IsEmpty)
                    {
                        continue;
                    }

                    var containment = subtree.Bounds.Intersect(bounds);

                    if (containment == ContainmentType.Disjoint)
                    {
                        continue;
                    }

                    foreach (var item in subtree.FindIntersectingInternal(bounds))
                    {
                        yield return item;
                    }

                    // If the rect is entirely contained in the subtree, we don't need to check other subtrees.
                    if (containment == ContainmentType.Contains)
                    {
                        yield break;
                    }
                }
            }

            foreach (var item in _items)
            {
                if (bounds.IntersectsWith(item.Bounds))
                {
                    yield return item;
                }
            }
        }

        private void Subdivide()
        {
            var depth = _depth + 1;
            _children = new []
            {
                new Quadtree<T>(Bounds, Quad.UpperLeft, depth),
                new Quadtree<T>(Bounds, Quad.UpperRight, depth),
                new Quadtree<T>(Bounds, Quad.LowerLeft, depth),
                new Quadtree<T>(Bounds, Quad.LowerRight, depth)
            };

            var oldItems = _items.ToList();
            _items.Clear();

            foreach (var oldItem in oldItems)
            {
                Insert(oldItem);
            }
        }

        public void Insert(in T item)
        {
            // 1. If this is a leaf, either insert it or subdivide.
            if (IsLeaf)
            {
                // If we've reached the limit and can subdivide, do so.
                if (ReachedItemLimit && !ReachedDepthLimit)
                {
                    Subdivide();
                    // Control flow continues to the for loop below.
                }
                else
                {
                    _items.Add(item);
                    return;
                }
            }

            // 2. Check if the item fully fits into any of the children.
            foreach (var subTree in _children)
            {
                var containment = subTree.Bounds.Intersect(item.Bounds);

                switch (containment)
                {
                    case ContainmentType.Disjoint: continue;

                    case ContainmentType.Contains:
                    {
                        subTree.Insert(item);
                        return;
                    }

                    // 3. Item fits into multiple subtrees.
                    // In that case, add it to this node while ignoring the item limit.
                    case ContainmentType.Intersects:
                    {
                        _items.Add(item);
                        return;
                    }
                    // This should be unreachable.
                    default:
                        return;
                }
            }
        }

        public void Remove(in T item)
        {
            if (_children != null)
            {
                foreach (var subTree in _children)
                {
                    var subtree = subTree;
                    if (subtree == null)
                    {
                        continue;
                    }

                    var containment = subtree.Bounds.Intersect(item.Bounds);

                    switch (containment)
                    {
                        case ContainmentType.Disjoint: continue;

                        case ContainmentType.Contains:
                        {
                            subtree.Remove(item);
                            return;
                        }

                        // In this case we'll know the item is either stored in this node or nowhere,
                        // so we can fall through to the for loop below.
                        case ContainmentType.Intersects: break;
                    }

                    break;
                }
            }

            _items.Remove(item);
        }

        public void Update(in T item)
        {
            Remove(item);
            Insert(item);
        }

        internal enum Quad
        {
            UpperLeft = 0,
            UpperRight,
            LowerLeft,
            LowerRight,
        }
    }

    public interface IHasBounds
    {
        RectangleF Bounds { get; }
    }
}
