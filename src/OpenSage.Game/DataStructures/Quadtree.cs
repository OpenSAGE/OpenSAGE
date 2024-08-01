using System.Collections.Generic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.DataStructures;

public sealed class Quadtree<T> : IQuadtree<T> where T : class, ICollidable
{
    public readonly RectangleF Bounds;

    private readonly QuadtreeNode<T> _rootNode;

    public Quadtree(RectangleF bounds) : this(bounds, bounds, 5, 1.15f, 16)
    {
    }

    private Quadtree(RectangleF bounds, RectangleF maxSize, float minNodeSideLength, float scaleFactor, int maxItemsPerNode)
    {
        Bounds = bounds;
        _rootNode = new QuadtreeNode<T>(Bounds, maxSize, minNodeSideLength, scaleFactor, maxItemsPerNode);
    }

    public IEnumerable<T> FindNearby(T obj, Transform transform, float radius) => Find(new SphereCollider(transform, radius), obj, true, false);

    public IEnumerable<T> FindIntersecting(in RectangleF bounds) => FindIntersecting(new BoxCollider(bounds));

    public IEnumerable<T> FindIntersecting(T searcher) => Find(searcher.RoughCollider, searcher);

    public IEnumerable<T> FindIntersecting(in Collider collider, T searcher = null) => Find(collider, searcher);

    public IEnumerable<T> Find(in Collider collider, T searcher, bool twoDimensional = false, bool hasToCollideWithSearcher = true)
    {
        return !collider.Intersects(Bounds) ? [] : FindIntersectingInternal(collider, searcher, twoDimensional, hasToCollideWithSearcher);
    }

    // collider is what we use to determine intersections _unless_ hasToCollideWithSearcher is true, in which case searcher is also used
    // otherwise searcher is just excluded from the results
    // twoDimensional is passed into CollidesWith calls

    private IEnumerable<T> FindIntersectingInternal(Collider collider, T searcher, bool twoDimensional = false, bool hasToCollideWithSearcher = true)
    {
        return _rootNode.Find(collider, searcher, twoDimensional, hasToCollideWithSearcher);
    }

    public bool Insert(in T item)
    {
        return _rootNode.Insert(item);
    }

    public bool Remove(in T item)
    {
        return _rootNode.Remove(item);
    }

    public bool Update(in T item)
    {
        Remove(item);
        return Insert(item);
    }
}
