using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.DebugOverlay;
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

        public abstract List<DebugPoint> GetGridPoints();
        public abstract bool Intersects(in BoundingFrustum frustum);

        protected abstract bool IntersectsTransformedRay(in Ray ray, out float depth);

        public abstract Rectangle GetBoundingRectangle(Camera camera);
        public abstract void Draw(DrawingContext2D drawingContext, Camera camera);

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
        private readonly DebugPoint[] _corners;

        public BoxCollider(ObjectDefinition def, Transform transform)
            : base(transform)
        {
            var min = new Vector3(-def.Geometry.MajorRadius, -def.Geometry.MinorRadius, 0);
            var max = new Vector3(def.Geometry.MajorRadius, def.Geometry.MinorRadius, def.Geometry.Height);
            _bounds = new BoundingBox(min, max);
            _corners = new DebugPoint[4];
        }

        public override List<DebugPoint> GetGridPoints()
        {
            List<DebugPoint> result = new List<DebugPoint>();

            CalculateCorners();
            for (var x = 0; x < _bounds.Max.X * 2; x += 10)
            {
                for (var y = 0; y < _bounds.Max.Y * 2; y += 10)
                {
                    //TODO some points are still visibile as grid points
                    var yPos = _corners[1].Position + Vector3.Transform(new Vector3(x, y, 0), Transform.Rotation);
                    result.Add(new DebugPoint(yPos));
                }
            }

            return result;
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
            return worldBounds.GetBoundingRectangle(camera);
        }

        private void CalculateCorners()
        {
            var leftSide = new DebugPoint(Transform.Translation + Vector3.Transform(Vector3.UnitX, Transform.Rotation) * _bounds.Max.X);
            var rightSide = new DebugPoint(Transform.Translation - Vector3.Transform(Vector3.UnitX, Transform.Rotation) * _bounds.Max.X);
            var topSide = new DebugPoint(Transform.Translation + Vector3.Transform(Vector3.UnitY, Transform.Rotation) * _bounds.Max.Y);
            var bottomSide = new DebugPoint(Transform.Translation - Vector3.Transform(Vector3.UnitY, Transform.Rotation) * _bounds.Max.Y);

            _corners[0] = new DebugPoint(leftSide.Position + (Transform.Translation - topSide.Position));
            _corners[1] = new DebugPoint(rightSide.Position + (Transform.Translation - topSide.Position));
            _corners[2] = new DebugPoint(rightSide.Position + (Transform.Translation - bottomSide.Position));
            _corners[3] = new DebugPoint(leftSide.Position + (Transform.Translation - bottomSide.Position));
        }

        public override void Draw(DrawingContext2D drawingContext, Camera camera)
        {
            CalculateCorners();
            var rectLt = _corners[0].GetBoundingRectangle(camera);
            var rectRt = _corners[1].GetBoundingRectangle(camera);
            var rectRb = _corners[2].GetBoundingRectangle(camera);
            var rectLb = _corners[3].GetBoundingRectangle(camera);

            drawingContext.DrawLine(new Line2D(new Vector2(rectLt.X, rectLt.Y), new Vector2(rectLb.X, rectLb.Y)), 1,
                new ColorRgbaF(220, 220, 220, 255));
            drawingContext.DrawLine(new Line2D(new Vector2(rectLb.X, rectLb.Y), new Vector2(rectRb.X, rectRb.Y)), 1,
                new ColorRgbaF(220, 220, 220, 255));
            drawingContext.DrawLine(new Line2D(new Vector2(rectRb.X, rectRb.Y), new Vector2(rectRt.X, rectRt.Y)), 1,
                new ColorRgbaF(220, 220, 220, 255));
            drawingContext.DrawLine(new Line2D(new Vector2(rectRt.X, rectRt.Y), new Vector2(rectLt.X, rectLt.Y)), 1,
                new ColorRgbaF(220, 220, 220, 255));
        }
    }

    public class SphereCollider : Collider
    {
        private readonly BoundingSphere _bounds;

        public SphereCollider(ObjectDefinition def, Transform transform)
            : base(transform)
        {
            _bounds = new BoundingSphere(Vector3.Zero, def.Geometry.MajorRadius);
        }

        protected override bool IntersectsTransformedRay(in Ray transformedRay, out float depth)
        {
            return transformedRay.Intersects(_bounds, out depth);
        }

        public override List<DebugPoint> GetGridPoints()
        {
            return new List<DebugPoint>(); // TODO implement
        }

        public override bool Intersects(in BoundingFrustum frustum)
        {
            var worldBounds = BoundingSphere.Transform(_bounds, Transform.Matrix);
            return frustum.Intersects(worldBounds);
        }

        public override Rectangle GetBoundingRectangle(Camera camera)
        {
            // TODO: Implement this.
            // Or don't, since Generals has only 4 spherical selectable objects,
            // and all of them are debug objects.
            return new Rectangle(0, 0, 0, 0);
        }

        public override void Draw(DrawingContext2D drawingContext, Camera camera)
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
            var height = def.Geometry.Height;

            _bounds = new BoundingBox(
                new Vector3(-radius, -radius, 0),
                new Vector3(radius, radius, height));
        }

        protected override bool IntersectsTransformedRay(in Ray transformedRay, out float depth)
        {
            return transformedRay.Intersects(_bounds, out depth);
        }

        public override List<DebugPoint> GetGridPoints()
        {
            return new List<DebugPoint>(); // TODO implement
        }

        public override bool Intersects(in BoundingFrustum frustum)
        {
            var worldBounds = BoundingBox.Transform(_bounds, Transform.Matrix);
            return frustum.Intersects(worldBounds);
        }

        public override Rectangle GetBoundingRectangle(Camera camera)
        {
            var worldBounds = BoundingBox.Transform(_bounds, Transform.Matrix);
            return worldBounds.GetBoundingRectangle(camera);
        }

        public override void Draw(DrawingContext2D drawingContext, Camera camera)
        {
            var points = new DebugPoint[10];
            var firstPoint = new Rectangle();
            var lastPoint = new Rectangle();
            for (var i = 0; i <= points.Length; i++)
            {
                var point = new DebugPoint(new Vector3(
                    Transform.Translation.X +
                    (float) Math.Cos(360f / points.Length * i / 180f * Math.PI) * _bounds.Max.X,
                    Transform.Translation.Y +
                    (float) Math.Sin(360f / points.Length * i / 180f * Math.PI) * _bounds.Max.X,
                    Transform.Translation.Z));

                var pointRect = point.GetBoundingRectangle(camera);

                if (i == 0)
                {
                    firstPoint = pointRect;
                    lastPoint = pointRect;
                }
                else if (i == points.Length)
                {
                    drawingContext.DrawLine(
                        new Line2D(new Vector2(lastPoint.X, lastPoint.Y), new Vector2(pointRect.X, pointRect.Y)), 1,
                        new ColorRgbaF(220, 220, 220, 255));
                    drawingContext.DrawLine(
                        new Line2D(new Vector2(pointRect.X, pointRect.Y), new Vector2(firstPoint.X, firstPoint.Y)), 1,
                        new ColorRgbaF(220, 220, 220, 255));
                }
                else
                {
                    drawingContext.DrawLine(
                        new Line2D(new Vector2(lastPoint.X, lastPoint.Y), new Vector2(pointRect.X, pointRect.Y)), 1,
                        new ColorRgbaF(220, 220, 220, 255));
                    lastPoint = pointRect;
                }
            }
        }
    }
}
