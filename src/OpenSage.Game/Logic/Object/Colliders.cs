using System;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Gui;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class Collider
    {
        protected Transform Transform;
        public BoundingVolume WorldBounds { get; protected set; }
        public TransformedRectangle BoundingArea { get; protected set; }
        public RectangleF AABoundingArea { get; protected set; }
        public float Height { get; protected set; }

        protected Collider(Transform transform)
        {
            Transform = transform;
        }

        public abstract void Update(Transform transform);

        public abstract bool Intersects(in BoundingFrustum frustum);
        protected abstract bool Intersects(in Ray ray, out float depth);

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

        public BoxCollider(ObjectDefinition def, Transform transform)
            : base(transform)
        {
            var min = new Vector3(-def.Geometry.MajorRadius, -def.Geometry.MinorRadius, 0);
            var max = new Vector3(def.Geometry.MajorRadius, def.Geometry.MinorRadius, def.Geometry.Height);
            _bounds = new BoundingBox(min, max);
            Update(transform);
            Height = def.Geometry.Height;
        }

        public sealed override void Update(Transform transform)
        {
            WorldBounds = BoundingBox.Transform(_bounds, transform.Matrix);
            BoundingArea = TransformedRectangle.FromBoundingBox(_bounds, Transform.Translation, Transform.Rotation);
            //AABoundingArea = RectangleF.
        }

        protected override bool Intersects(in Ray ray, out float depth)
        {
            var result = ray.Intersects((BoundingBox) WorldBounds, out depth);
            depth *= Transform.Scale; // Assumes uniform scaling
            return result;
        }

        public override bool Intersects(in BoundingFrustum frustum) => frustum.Intersects((BoundingBox) WorldBounds);

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
        

        public SphereCollider(ObjectDefinition def, Transform transform)
            : base(transform)
        {
            var radius = def.Geometry.MajorRadius;
            _bounds = new BoundingSphere(Vector3.Zero, radius);
            Update(transform);
            Height = def.Geometry.Height;
        }

        public sealed override void Update(Transform transform)
        {
            WorldBounds = BoundingSphere.Transform(_bounds, transform.Matrix);
            BoundingArea = TransformedRectangle.FromBoundingSphere(_bounds, Transform.Translation);
        }

        protected override bool Intersects(in Ray ray, out float depth)
        {
            var result = ray.Intersects((BoundingSphere)WorldBounds, out depth);
            depth *= Transform.Scale; // Assumes uniform scaling
            return result;
        }

        public override bool Intersects(in BoundingFrustum frustum) => frustum.Intersects((BoundingSphere)WorldBounds);

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

        public CylinderCollider(ObjectDefinition def, Transform transform)
             : base(transform)
        {
            var radius = def.Geometry.MajorRadius;
            Height = def.Geometry.Height;

            _bounds = new BoundingBox(
                new Vector3(-radius, -radius, 0),
                new Vector3(radius, radius, def.Geometry.Height));

            Update(transform);
        }

        public sealed override void Update(Transform transform)
        {
            WorldBounds = BoundingBox.Transform(_bounds, transform.Matrix);
            BoundingArea = TransformedRectangle.FromBoundingBox(_bounds, Transform.Translation, Transform.Rotation);
        }

        protected override bool Intersects(in Ray ray, out float depth)
        {
            var result = ray.Intersects((BoundingBox)WorldBounds, out depth);
            depth *= Transform.Scale; // Assumes uniform scaling
            return result;
        }

        public override bool Intersects(in BoundingFrustum frustum) => frustum.Intersects((BoundingBox)WorldBounds);

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
