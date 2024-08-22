#nullable enable

using System.Collections.Generic;
using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.DataStructures;

public class QuadtreeNode<T> where T : class, ICollidable
{
    private readonly RectangleF _box;
    private readonly int _maxItemsPerNode;
    private readonly HashSet<T> _data;
    private readonly QuadtreeNode<T>[] _children = new QuadtreeNode<T>[4];
    private bool _hasChildren;
    private readonly RectangleF _originalSize;
    private readonly float _overlapFactor;
    private readonly float _minNodeSideLength;
    private readonly Vector2 _center;

    private bool IsMinSize => _originalSize.Width * _overlapFactor / 2 < _minNodeSideLength || _originalSize.Height * _overlapFactor / 2 < _minNodeSideLength;

    public QuadtreeNode(in RectangleF bounds, in RectangleF originalSize, float minNodeSideLength, float scaleFactor, int maxItemsPerNode)
    {
        _box = bounds;
        _overlapFactor = scaleFactor;
        _originalSize = originalSize;
        _minNodeSideLength = minNodeSideLength;
        _maxItemsPerNode = maxItemsPerNode;
        _center = new Vector2(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
        _data = new HashSet<T>(maxItemsPerNode);
    }

    public IEnumerable<T> Find(Collider collider, T? searcher, bool twoDimensional, bool hasToCollideWithSearcher)
    {
        if (_hasChildren)
        {
            foreach (var child in _children)
            {
                if (!child.Touches(collider))
                {
                    continue;
                }

                foreach (var subChild in child.Find(collider, searcher, twoDimensional, hasToCollideWithSearcher))
                {
                    if (CollidesPrecise(subChild, collider, searcher, twoDimensional, hasToCollideWithSearcher))
                    {
                        yield return subChild;
                    }
                }
            }
        }

        foreach (var data in _data)
        {
            if (!collider.Intersects(data.RoughCollider.AxisAlignedBoundingArea))
            {
                continue;
            }

            if (CollidesPrecise(data, collider, searcher, twoDimensional, hasToCollideWithSearcher))
            {
                yield return data;
            }
        }
    }

    private static bool CollidesPrecise(T item, Collider collider, T? searcher, bool twoDimensional, bool hasToCollideWithSearcher)
    {
        if (searcher != null)
        {
            if (item.Equals(searcher))
            {
                return false;
            }

            if (hasToCollideWithSearcher)
            {
                if (searcher.CollidesWith(item, twoDimensional))
                {
                    return true;
                }

                return false;
            }
        }

        if (collider.Intersects(item.RoughCollider))
        {
            foreach (var c in item.Colliders)
            {
                if (collider.Intersects(c, twoDimensional))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool Insert(in T item)
    {
        if (!FullyContains(item.RoughCollider.AxisAlignedBoundingArea))
        {
            return false;
        }

        if (_data.Count >= _maxItemsPerNode && !IsMinSize)
        {
            if (!_hasChildren)
            {
                GenerateChildren();
            }

            var bestChild = BestFitChild(item);
            if (_children[bestChild].Insert(item))
            {
                return true;
            }
        }

        _data.Add(item);
        return true;
    }

    public bool Remove(in T item)
    {
        if (_hasChildren)
        {
            var bestChild = BestFitChild(item);
            if (_children[bestChild].Remove(item))
            {
                return true;
            }
        }

        return _data.Remove(item);
    }

    /// <summary>
    /// Find which child node this object would be most likely to fit in.
    /// </summary>
    private int BestFitChild(in T item)
    {
        var aabb = item.RoughCollider.AxisAlignedBoundingArea;

        var total = 0;

        // bottom left is 0
        // bottom right is 1
        // top left is 2
        // top right is 3

        if (aabb.X + aabb.Width / 2 >= _center.X)
        {
            total = 1;
        }

        if (aabb.Y + aabb.Height / 2 >= _center.Y)
        {
            total += 2;
        }

        return total;
    }

    private bool FullyContains(in RectangleF box) => _box.Contains(box);

    private bool Touches(in Collider box) => box.Intersects(_box);

    private void GenerateChildren()
    {
        var halfSize =  RectangleF.Scale(_originalSize, 0.5f);

        var halfWidth = halfSize.Width;
        var halfHeight = halfSize.Height;

        // bottom left is 0
        // bottom right is 1
        // top left is 2
        // top right is 3

        var bottomLeft = RectangleF.Scale(new RectangleF(_box.X, _box.Y, halfWidth, halfHeight), _overlapFactor);
        var bottomRight = RectangleF.Scale(new RectangleF(_center.X, _box.Y, halfWidth, halfHeight), _overlapFactor);
        var topLeft = RectangleF.Scale(new RectangleF(_box.X, _center.Y, halfWidth, halfHeight), _overlapFactor);
        var topRight = RectangleF.Scale(new RectangleF(_center.X, _center.Y, halfWidth, halfHeight), _overlapFactor);

        _children[0] = new QuadtreeNode<T>(bottomLeft, halfSize, _minNodeSideLength, _overlapFactor, _maxItemsPerNode);
        _children[1] = new QuadtreeNode<T>(bottomRight, halfSize, _minNodeSideLength, _overlapFactor, _maxItemsPerNode);
        _children[2] = new QuadtreeNode<T>(topLeft, halfSize, _minNodeSideLength, _overlapFactor, _maxItemsPerNode);
        _children[3] = new QuadtreeNode<T>(topRight, halfSize, _minNodeSideLength, _overlapFactor, _maxItemsPerNode);

        _hasChildren = true;
    }
}
