using System.Collections.Generic;
using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.DataStructures;

public interface ICollidable
{
    Collider RoughCollider { get; }
    List<Collider> Colliders { get; }
    Vector3 Translation { get; }

    bool CollidesWith(ICollidable other, bool twoDimensional);
}
