using System;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Gui;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class Collider
    {
        protected readonly Transform Transform;

        protected Collider(Transform transform)
        {
            Transform = transform;
        }

        public abstract float Height { get; }

        public bool Intersects(in Ray ray, out float depth)
        {
            var transformedRay = Ray.Transform(ray, Transform.MatrixInverse);

            if (IntersectsTransformedRay(transformedRay, out depth))
            {
                // Assumes uniform scaling
                depth *= Transform.Scale;
                return true;
            }

            return false;
        }

        public abstract bool Intersects(in BoundingFrustum frustum);

        protected abstract bool IntersectsTransformedRay(in Ray ray, out float depth);

        public abstract Rectangle GetBoundingRectangle(Camera camera);
        public abstract TransformedRectangle GetBoundingArea();
        public abstract BoundingBox GetAxisAlignedBoundingBox();
        public abstract void DebugDraw(DrawingContext2D drawingContext, Camera camera);

        public static Collider Create(ObjectDefinition definition, Transform transform)
        {
            if (definition.Geometry == null)
            {
                return null;
            }

            switch (definition.Geometry.Type)
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
        private readonly BoundingBox _bounds;
        private readonly float _height;

        public override float Height => _height;

        public BoxCollider(ObjectDefinition def, Transform transform)
            : base(transform)
        {
            var min = new Vector3(-def.Geometry.MajorRadius, -def.Geometry.MinorRadius, 0);
            var max = new Vector3(def.Geometry.MajorRadius, def.Geometry.MinorRadius, def.Geometry.Height);
            _bounds = new BoundingBox(min, max);
            _height = def.Geometry.Height;
        }

        protected override bool IntersectsTransformedRay(in Ray transformedRay, out float depth)
        {
            return transformedRay.Intersects(_bounds, out depth);
        }

        public override bool Intersects(in BoundingFrustum frustum)
        {
            var worldBounds = BoundingBox.Transform(_bounds, Transform.Matrix);
            return frustum.Intersects(worldBounds);
        }

        public override Rectangle GetBoundingRectangle(Camera camera)
        {
            var worldBounds = BoundingBox.Transform(_bounds, Transform.Matrix);
            return camera.GetBoundingRectangle(worldBounds);
        }

        public override TransformedRectangle GetBoundingArea()
        {
            return TransformedRectangle.FromBoundingBox(_bounds, Transform.Translation, Transform.Rotation);
        }

        public override BoundingBox GetAxisAlignedBoundingBox()
        {
             return BoundingBox.Transform(_bounds, Transform.Matrix);
        }

        public override void DebugDraw(DrawingContext2D drawingContext, Camera camera)
        {
            var strokeColor = new ColorRgbaF(220, 220, 220, 255);

            var worldPos = Transform.Translation;
            var rotation = Transform.Rotation;

            var xLine = Vector3.Transform(new Vector3(_bounds.Max.X, 0, 0), rotation);
            var yLine = Vector3.Transform(new Vector3(0, _bounds.Max.Y, 0), rotation);

            var leftSide = xLine;
            var rightSide = -xLine;
            var topSide = yLine;
            var bottomSide = -yLine;

            var ltWorld = worldPos + (leftSide - topSide);
            var rtWorld = worldPos + (rightSide - topSide);
            var rbWorld = worldPos + (rightSide - bottomSide);
            var lbWorld = worldPos + (leftSide - bottomSide);

            var ltScreen = camera.WorldToScreenPoint(ltWorld).Vector2XY();
            var rtScreen = camera.WorldToScreenPoint(rtWorld).Vector2XY();
            var rbScreen = camera.WorldToScreenPoint(rbWorld).Vector2XY();
            var lbScreen = camera.WorldToScreenPoint(lbWorld).Vector2XY();

            drawingContext.DrawLine(new Line2D(ltScreen, lbScreen), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(lbScreen, rbScreen), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(rbScreen, rtScreen), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(rtScreen, ltScreen), 1, strokeColor);
        }

        public bool Intersects(BoxCollider other)
        {
            // TODO: This ignores the Z dimension. Does that matter?
            var rectA = TransformedRectangle.FromBoundingBox(_bounds, Transform.Translation, Transform.Rotation);
            var rectB = TransformedRectangle.FromBoundingBox(other._bounds, other.Transform.Translation, other.Transform.Rotation);
            return rectA.Intersects(rectB);
        }
    }

    public class SphereCollider : Collider
    {
        private readonly BoundingSphere _bounds;
        private readonly float _height;

        public override float Height => _height;

        public SphereCollider(ObjectDefinition def, Transform transform)
            : base(transform)
        {
            _bounds = new BoundingSphere(Vector3.Zero, def.Geometry.MajorRadius);
            _height = def.Geometry.Height;
        }

        protected override bool IntersectsTransformedRay(in Ray transformedRay, out float depth)
        {
            return transformedRay.Intersects(_bounds, out depth);
        }

        public override bool Intersects(in BoundingFrustum frustum)
        {
            var worldBounds = BoundingSphere.Transform(_bounds, Transform.Matrix);
            return frustum.Intersects(worldBounds);
        }

        public override BoundingBox GetAxisAlignedBoundingBox()
        {
            var worldBounds = BoundingSphere.Transform(_bounds, Transform.Matrix);
            var center = worldBounds.Center;
            var radius = worldBounds.Radius;
            var min = new Vector3(center.X - radius, center.Y - radius, center.Z - radius);
            var max = new Vector3(center.X + radius, center.Y + radius, center.Z + radius);
            return new BoundingBox(min, max);
        }

        public override Rectangle GetBoundingRectangle(Camera camera)
        {
            // TODO: Implement this.
            // Or don't, since Generals has only 4 spherical selectable objects,
            // and all of them are debug objects.
            return new Rectangle(0, 0, 0, 0);
        }

        public override TransformedRectangle GetBoundingArea()
        {
            return TransformedRectangle.FromBoundingSphere(_bounds, Transform.Translation);
        }

        public override void DebugDraw(DrawingContext2D drawingContext, Camera camera)
        {
            //TODO implement
        }
    }

    // TODO: This currently uses a bounding box for collision.
    // It's a half-decent approximation fow now.
    public class CylinderCollider : Collider
    {
        private readonly BoundingBox _bounds;
        private readonly float _height;

        public override float Height => _height;

        public CylinderCollider(ObjectDefinition def, Transform transform)
             : base(transform)
        {
            var radius = def.Geometry.MajorRadius;
            _height = def.Geometry.Height;

            _bounds = new BoundingBox(
                new Vector3(-radius, -radius, 0),
                new Vector3(radius, radius, _height));
        }

        protected override bool IntersectsTransformedRay(in Ray transformedRay, out float depth)
        {
            return transformedRay.Intersects(_bounds, out depth);
        }

        public override bool Intersects(in BoundingFrustum frustum)
        {
            var worldBounds = BoundingBox.Transform(_bounds, Transform.Matrix);
            return frustum.Intersects(worldBounds);
        }

        public override Rectangle GetBoundingRectangle(Camera camera)
        {
            var worldBounds = BoundingBox.Transform(_bounds, Transform.Matrix);
            return camera.GetBoundingRectangle(worldBounds);
        }

        public override TransformedRectangle GetBoundingArea()
        {
            return TransformedRectangle.FromBoundingBox(_bounds, Transform.Translation, Transform.Rotation);
        }

        public override BoundingBox GetAxisAlignedBoundingBox()
        {
            return BoundingBox.Transform(_bounds, Transform.Matrix);
        }

        public override void DebugDraw(DrawingContext2D drawingContext, Camera camera)
        {
            const int sides = 8;
            var lineColor = new ColorRgbaF(220, 220, 220, 255);

            var radius = _bounds.Max.X;
            var firstPoint = Vector2.Zero;
            var previousPoint = Vector2.Zero;

            for (var i = 0; i < sides; i++)
            {
                var angle = 2 * MathF.PI * i / sides;
                var point = Transform.Translation + new Vector3(MathF.Cos(angle), MathF.Sin(angle), 0) * radius;
                var screenPoint = camera.WorldToScreenPoint(point).Vector2XY();

                // No line gets drawn on the first iteration
                if (i == 0)
                {
                    firstPoint = screenPoint;
                    previousPoint = screenPoint;
                    continue;
                }

                drawingContext.DrawLine(new Line2D(previousPoint, screenPoint), 1, lineColor);

                // If this is the last point, complete the cylinder
                if (i == sides - 1)
                {
                    drawingContext.DrawLine(new Line2D(screenPoint, firstPoint), 1, lineColor);
                }

                previousPoint = screenPoint;
            }
        }
    }
}
