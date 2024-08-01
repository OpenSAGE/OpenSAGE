using System.Collections.Generic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.DataStructures;

public interface IQuadtree<T> where T : class, ICollidable
{
    IEnumerable<T> FindNearby(T obj, Transform transform, float radius);
    IEnumerable<T> FindIntersecting(in RectangleF bounds);
    IEnumerable<T> FindIntersecting(T searcher);
    IEnumerable<T> FindIntersecting(in Collider collider, T searcher = null);
    bool Insert(in T item);
    bool Remove(in T item);
    bool Update(in T item);
}
