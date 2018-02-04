using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class ColliderComponent : EntityComponent
    {
        public abstract bool Intersects(in Ray ray, out float depth);

        public static void AddColliders(Entity entity, ObjectDefinition definition)
        {
            switch (definition.Geometry)
            {
                case ObjectGeometry.Box:
                    entity.AddComponent(new BoxColliderComponent(definition));
                    break;
                case ObjectGeometry.Sphere:
                    entity.AddComponent(new SphereColliderComponent(definition));
                    break;
                case ObjectGeometry.Cylinder:
                    entity.AddComponent(new CylinderColliderComponent(definition));
                    break;
            }
        }
    }

    public class BoxColliderComponent : ColliderComponent
    {
        private readonly BoundingBox _bounds;

        public BoxColliderComponent(ObjectDefinition def)
        {
            var min = new Vector3(-def.GeometryMajorRadius, -def.GeometryMinorRadius, 0);
            var max = new Vector3(def.GeometryMajorRadius, def.GeometryMinorRadius, def.GeometryHeight);
            _bounds = new BoundingBox(min, max);
        }

        public override bool Intersects(in Ray ray, out float depth)
        {
            var transformedRay = ray.Transform(Transform.WorldToLocalMatrix);

            if (transformedRay.Intersects(_bounds, out depth))
            {
                // Assumes uniform scaling
                depth *= Transform.LocalScale.X;
                return true;
            }
            return false;
        }
    }

    public class SphereColliderComponent : ColliderComponent
    {
        private readonly BoundingSphere _bounds;

        public SphereColliderComponent(ObjectDefinition def)
        {
            _bounds = new BoundingSphere(Vector3.Zero, def.GeometryMajorRadius);
        }

        public override bool Intersects(in Ray ray, out float depth)
        {
            var transformedRay = ray.Transform(Transform.WorldToLocalMatrix);

            if (transformedRay.Intersects(_bounds, out depth))
            {
                // Assumes uniform scaling
                depth *= Transform.LocalScale.X;
                return true;
            }
            return false;
        }
    }

    // TODO: This currently uses a bounding box for collision.
    // It's a half-decent approximation fow now.
    public class CylinderColliderComponent : ColliderComponent
    {
        private readonly BoundingBox _bounds;

        public CylinderColliderComponent(ObjectDefinition def)
        {
            var radius = def.GeometryMajorRadius;
            var height = def.GeometryHeight;

            _bounds = new BoundingBox(new Vector3(-radius, -radius, 0), new Vector3(radius, radius, height));
        }

        public override bool Intersects(in Ray ray, out float depth)
        {
            var transformedRay = ray.Transform(Transform.WorldToLocalMatrix);

            if (transformedRay.Intersects(_bounds, out depth))
            {
                // Assumes uniform scaling
                depth *= Transform.LocalScale.X;
                return true;
            }
            return false;
        }
    }
}
