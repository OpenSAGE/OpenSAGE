using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Gui;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.DataStructures
{
    public sealed class Quadtree<T> where T : class, IHasCollider
    {
        private const int MaxDepth = 8;
        private const int MaxItemsPerLeaf = 2;

        public readonly RectangleF Bounds;

        // If this is a leaf, this array is null.
        // If this a tree node, this array is initialized
        private Quadtree<T>[] _children;

        private HashSet<T> _items = new HashSet<T>();

        private readonly int _depth;

        private bool IsLeaf => _children == null;
        private bool ReachedItemLimit => _items.Count >= MaxItemsPerLeaf;
        private bool ReachedDepthLimit => _depth >= MaxDepth;
        private bool IsEmpty => _children == null && _items.Count == 0;

        public List<T> Items()
        {
            var result = new List<T>();
            if (_children != null)
            {
                foreach (var child in _children)
            {
                result.AddRange(child.Items());
            }
            }
            
            result.AddRange(_items.ToList());
            return result;
        }

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

            // position this 'child'-quadtree according to its Quad position
            switch (quad)
            {
                case Quad.LowerLeft: // our lower left corner aligns with the parents lower left corner so nothing to do
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

        public IEnumerable<T> FindNearby(T obj, Transform transform, float radius) => FindIntersecting(new SphereCollider(transform, radius), obj);

        public IEnumerable<T> FindIntersecting(in RectangleF bounds) => FindIntersecting(new BoxCollider(bounds));

        public IEnumerable<T> FindIntersecting(in Collider collider, T searcher = null)
        {
            return !collider.Intersects(Bounds) ? Enumerable.Empty<T>() : FindIntersectingInternal(collider, searcher);
        }

        private IEnumerable<T> FindIntersectingInternal(Collider collider, T searcher)
        {
            if (!IsLeaf)
            {
                foreach (var subtree in _children)
                {
                    if (subtree.IsEmpty)
                    {
                        continue;
                    }

                    var containment = subtree.Bounds.Intersect(collider.AxisAlignedBoundingArea);

                    if (containment == ContainmentType.Disjoint)
                    {
                        continue;
                    }

                    foreach (var item in subtree.FindIntersectingInternal(collider, searcher))
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
                if (!item.Equals(searcher) && item.Collider.Intersects(collider))
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

            var oldItems = _items;
            _items = new HashSet<T>();

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
                var containment = subTree.Bounds.Intersect(item.Collider.AxisAlignedBoundingArea);

                switch (containment)
                {
                    case ContainmentType.Disjoint:
                        continue;

                    case ContainmentType.Contains:
                        subTree.Insert(item);
                        return;

                    // 3. Item fits into multiple subtrees.
                    // In that case, add it to this node while ignoring the item limit.
                    case ContainmentType.Intersects:
                        _items.Add(item);
                        return;

                    // This should be unreachable.
                    default:
                        return;
                }
            }
        }

        public bool Remove(in T item)
        {
            if (_children != null)
            {
                foreach (var subTree in _children)
                {
                    if (subTree.Remove(item))
                    {
                        return true;
                    }
                }
            }

            return _items.Remove(item);
        }

        public void Update(in T item)
        {
            Remove(item);
            Insert(item);
        }

        public void DebugDraw(DrawingContext2D drawingContext, Camera camera)
        {
            var strokeColor = new ColorRgbaF(0, 220, 0, 255);

            var ltWorld = new Vector3(Bounds.Position + new Vector2(0, Bounds.Height), 0);
            var rtWorld = new Vector3(Bounds.Position + new Vector2(Bounds.Width, Bounds.Height), 0);
            var rbWorld = new Vector3(Bounds.Position + new Vector2(Bounds.Width, 0), 0);
            var lbWorld = new Vector3(Bounds.Position, 0);

            var ltScreen = camera.WorldToScreenPoint(ltWorld).Vector2XY();
            var rtScreen = camera.WorldToScreenPoint(rtWorld).Vector2XY();
            var rbScreen = camera.WorldToScreenPoint(rbWorld).Vector2XY();
            var lbScreen = camera.WorldToScreenPoint(lbWorld).Vector2XY();

            drawingContext.DrawLine(new Line2D(ltScreen, lbScreen), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(lbScreen, rbScreen), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(rbScreen, rtScreen), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(rtScreen, ltScreen), 1, strokeColor);

            if (!IsLeaf)
            {
                foreach (var child in _children)
                {
                    child.DebugDraw(drawingContext, camera);
                }
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

    public interface IHasCollider
    {
        Collider Collider { get; }
    }
}
