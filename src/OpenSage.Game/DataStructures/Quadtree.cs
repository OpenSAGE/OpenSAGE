using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.DataStructures
{
    public sealed class Quadtree<T> where T : class
    {
        private const int MaxDepth = 8;
        private const int MaxItemsPerLeaf = 2;
        private const bool RemoveEmptyNodes = false;

        public readonly RectangleF Bounds;
        // TODO: Should we cache this?
        private SizeF QuadSize => new SizeF(Bounds.Size.Width / 2.0f, Bounds.Size.Height / 2.0f);

        // If this is a leaf, this array is null.
        // If this a tree node, this array is initialised, though subtrees are null until they get their first item.
        private Quadtree<T>[] _children;

        // This is null until the first item is added.
        private List<(RectangleF, T)> _items;

        private readonly int _depth;

        private bool IsLeaf => _children == null;
        private bool ReachedItemLimit => _items != null && _items.Count >= MaxItemsPerLeaf;
        private bool ReachedDepthLimit => _depth >= MaxDepth;

        private int ItemCount => _items?.Count ?? 0;
        private bool HasSubtrees => _children != null && _children.Any(x => x != null);
        private bool IsEmpty => !HasSubtrees && ItemCount == 0;

        public Quadtree(RectangleF bounds) : this(bounds, 0) { }

        private Quadtree(RectangleF bounds, int depth)
        {
            Bounds = bounds;
            _depth = depth;
        }

        private Quadtree<T> GetChild(Quad quad)
        {
            return _children.Length == 0 ? null : _children[(int) quad];
        }

        private RectangleF GetChildBounds(int i)
        {
            return GetChildBounds((Quad) i);
        }

        private RectangleF GetChildBounds(Quad quad)
        {
            var childBounds = GetChild(quad)?.Bounds;

            if (childBounds != null)
            {
                return childBounds.Value;
            }

            switch (quad)
            {
                case Quad.UpperLeft:
                    return new RectangleF(Bounds.Position, QuadSize);
                case Quad.UpperRight:
                    return new RectangleF(Bounds.Position + new Vector2(QuadSize.Width, 0), QuadSize);
                case Quad.LowerLeft:
                    return new RectangleF(Bounds.Position + new Vector2(0, QuadSize.Height), QuadSize);
                case Quad.LowerRight:
                    return new RectangleF(Bounds.Position + QuadSize.ToVector2(), QuadSize);
                default:
                    // This should be unreachable.
                    return new RectangleF();
            }
        }

        public IEnumerable<T> FindIntersecting(in RectangleF itemBounds)
        {
            if (!itemBounds.IntersectsWith(Bounds))
            {
                return Enumerable.Empty<T>();
            }

            return FindIntersectingInternal(itemBounds);
        }

        // Iterators cannot have in parameters, so we must copy the rectangle :(
        private IEnumerable<T> FindIntersectingInternal(RectangleF itemBounds)
        {
            if (_children != null)
            {
                foreach (var subtree in _children)
                {
                    if (subtree == null)
                    {
                        continue;
                    }

                    var containment = subtree.Bounds.Intersect(itemBounds);

                    if (containment == ContainmentType.Disjoint)
                    {
                        continue;
                    }

                    foreach (var item in subtree.FindIntersectingInternal(itemBounds))
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

            if (_items != null)
            {
                // TODO: This copies the tuple and/or rect unnecessarily.
                // Impossible to avoid with List<T> :(
                foreach (var (rect, item) in _items)
                {
                    if (itemBounds.IntersectsWith(rect))
                    {
                        yield return item;
                    }
                }
            }
        }

        private void Subdivide()
        {
            // TODO: Should we initialie all subtrees while we're at it?
            _children = new Quadtree<T>[4];

            // We'll replace the old _items list with 0, as Subdivide might modify it.
            var oldItems = _items;
            _items = null;

            // More unnecessary copies :(
            foreach (var (oldItemBounds, oldItem) in oldItems)
            {
                Insert(oldItemBounds, oldItem);
            }
        }

        private void InsertToItems(in RectangleF itemBounds, T item)
        {
            if (_items == null)
            {
                _items = new List<(RectangleF, T)>();
            }

            _items.Add((itemBounds, item));
        }

        public void Insert<A>(A item) where A : T, IHasBounds
        {
            Insert(item.Bounds, item);
        }

        public void Insert(in RectangleF itemBounds, T item)
        {
            if (!Bounds.Contains(itemBounds))
            {
                throw new ArgumentException($"Item must be fully contained within {Bounds}, was ${itemBounds}.", nameof(itemBounds));
            }

            InsertInternal(itemBounds, item);
        }

        private void InsertInternal(in RectangleF itemBounds, T item)
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
                    InsertToItems(itemBounds, item);
                    return;
                }
            }

            // 2. Check if the item fully fits into any of the children.
            for (var i = 0; i < _children.Length; i++)
            {
                var childBounds = GetChildBounds(i);
                var containment = childBounds.Intersect(itemBounds);

                switch (containment)
                {
                    case ContainmentType.Disjoint: continue;

                    case ContainmentType.Contains:
                    {
                        if (_children[i] == null)
                        {
                            _children[i] = new Quadtree<T>(childBounds, _depth + 1);
                        }
                        _children[i].InsertInternal(itemBounds, item);
                        return;
                    }

                    // 3. Item fits into multiple subtrees.
                    // In that case, add it to this node while ignoring the item limit.
                    case ContainmentType.Intersects:
                    {
                        InsertToItems(itemBounds, item);
                        return;
                    }
                    // This should be unreachable.
                    default:
                        return;
                }
            }
        }

        public void Remove<A>(A item) where A : T, IHasBounds
        {
            Remove(item.Bounds);
        }

        // TODO: Should this also take the value?
        // If the tree contains duplicate rectangles with different values,
        // this will remove the one that was inserted first.
        public T Remove(in RectangleF itemBounds)
        {
            if (!Bounds.Contains(itemBounds))
            {
                return null;
            }

            return RemoveInternal(itemBounds);
        }

        private T RemoveInternal(in RectangleF itemBounds)
        {
            if (_children != null)
            {
                for (var i = 0; i < _children.Length; i++)
                {
                    var subtree = _children[i];
                    if (subtree == null)
                    {
                        continue;
                    }

                    var containment = subtree.Bounds.Intersect(itemBounds);

                    switch (containment)
                    {
                        case ContainmentType.Disjoint: continue;

                        case ContainmentType.Contains:
                        {
                            var result = subtree.RemoveInternal(itemBounds);

                            if (RemoveEmptyNodes)
                            {
                                if (result != null && subtree.IsEmpty)
                                {
                                    _children[i] = null;

                                }
                            }

                            return result;
                        }

                        // In this case we'll know the item is either stored in this node or nowhere,
                        // so we can fall through to the for loop below.
                        case ContainmentType.Intersects: break;
                    }

                    break;
                }
            }

            if (_items != null)
            {
                for (var i = 0; i < _items.Count; i++)
                {
                    if (!_items[i].Item1.Equals(itemBounds))
                    {
                        continue;
                    }

                    var result = _items[i].Item2;

                    _items.RemoveAt(i);

                    return result;
                }
            }

            return null;
        }

        public void Update(in RectangleF oldRect, in RectangleF newRect)
        {
            var item = Remove(oldRect);

            if (item != null)
            {
                Insert(newRect, item);
            }
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
