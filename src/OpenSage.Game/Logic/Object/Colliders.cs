using System;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class Collider
    {
        private readonly Transform _transform;

        protected Collider(Transform transform)
        {
            _transform = transform;
        }

        public bool Intersects(in Ray ray, out float depth)
        {
            var transformedRay = ray.Transform(_transform.MatrixInverse);

            if (IntersectsTransformedRay(transformedRay, out depth))
            {
                // Assumes uniform scaling
                depth *= _transform.Scale;
                return true;
            }

            return false;
        }

        protected abstract bool IntersectsTransformedRay(in Ray ray, out float depth);

        public static Collider Create(ObjectDefinition definition, Transform transform)
        {
            switch (definition.Geometry)
            {
                case ObjectGeometry.Box:
                    return new BoxCollider(definition, transform);

                case ObjectGeometry.Sphere:
                    return new SphereCollider(definition, transform);

                case ObjectGeometry.Cylinder:
                    return new CylinderCollider(definition, transform);

                case ObjectGeometry.None:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class BoxCollider : Collider
    {
        public ref readonly BoundingBox Bounds => ref _bounds;
        private readonly BoundingBox _bounds;

        public BoxCollider(ObjectDefinition def, Transform transform)
            : base(transform)
        {
            var min = new Vector3(-def.GeometryMajorRadius, -def.GeometryMinorRadius, 0);
            var max = new Vector3(def.GeometryMajorRadius, def.GeometryMinorRadius, def.GeometryHeight);
            _bounds = new BoundingBox(min, max);
        }

        protected override bool IntersectsTransformedRay(in Ray transformedRay, out float depth)
        {
            return transformedRay.Intersects(_bounds, out depth);
        }
    }

    public class SphereCollider : Collider
    {
        private readonly BoundingSphere _bounds;

        public SphereCollider(ObjectDefinition def, Transform transform)
            : base(transform)
        {
            _bounds = new BoundingSphere(Vector3.Zero, def.GeometryMajorRadius);
        }

        protected override bool IntersectsTransformedRay(in Ray transformedRay, out float depth)
        {
            return transformedRay.Intersects(_bounds, out depth);
        }
    }

    // TODO: This currently uses a bounding box for collision.
    // It's a half-decent approximation fow now.
    public class CylinderCollider : Collider
    {
        private readonly BoundingBox _bounds;

        public CylinderCollider(ObjectDefinition def, Transform transform)
            : base(transform)
        {
            var radius = def.GeometryMajorRadius;
            var height = def.GeometryHeight;

            _bounds = new BoundingBox(
                new Vector3(-radius, -radius, 0),
                new Vector3(radius, radius, height));
        }

        protected override bool IntersectsTransformedRay(in Ray transformedRay, out float depth)
        {
            return transformedRay.Intersects(_bounds, out depth);
        }
    }
}
