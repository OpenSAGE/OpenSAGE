using System;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Gui;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class Collider
    {
        protected internal Transform Transform;
        public BoundingSphere WorldBounds { get; protected set; }
        public TransformedRectangle BoundingArea { get; protected set; }
        public RectangleF AxisAlignedBoundingArea { get; protected set; }
        public float Height { get; protected set; }

        protected Collider(Transform transform, float height)
        {
            Height = height;
            Transform = transform;
        }

        public abstract void Update(Transform transform);

        public abstract bool Intersects(in BoundingFrustum frustum);
        public abstract bool Intersects(in Ray ray, out float depth);

        public abstract bool Intersects(RectangleF bounds);

        public abstract bool Intersects(Collider collider);

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

        public abstract void DebugDraw(DrawingContext2D drawingContext, Camera camera);
    }

    public class SphereCollider : Collider
    {
        protected readonly BoundingSphere SphereBounds;

        public SphereCollider(ObjectDefinition def, Transform transform)
            : base(transform, def.Geometry.Height)
        {
            var radius = def.Geometry.MajorRadius;
            SphereBounds = new BoundingSphere(Vector3.Zero, radius);
            Update(transform);
        }

        protected SphereCollider(Transform transform, float radius, float height)
            : base(transform, height)
        {
            SphereBounds = new BoundingSphere(Vector3.Zero, radius);
            Update(transform);
        }

        protected SphereCollider(RectangleF rect)
            : base(new Transform(Matrix4x4.Identity), 0)
        {
            var radius = new Vector2(rect.Width, rect.Height).Length() / 2.0f;
            SphereBounds = new BoundingSphere(Vector3.Zero, radius);

            var center = new Vector3(rect.X + rect.Width / 2.0f, rect.Y + rect.Height / 2.0f, 0);
            WorldBounds = new BoundingSphere(center, radius);
            AxisAlignedBoundingArea = rect;
            BoundingArea = TransformedRectangle.FromRectangle(rect);
        }

        public override void Update(Transform transform)
        {
            WorldBounds = BoundingSphere.Transform(SphereBounds, transform.Matrix);
            var width = SphereBounds.Radius * 2.0f;
            AxisAlignedBoundingArea = new RectangleF(transform.Translation.Vector2XY(), width, width);
            BoundingArea = TransformedRectangle.FromRectangle(AxisAlignedBoundingArea);
        }

        public override bool Intersects(in Ray ray, out float depth)
        {
            var result = ray.Intersects(WorldBounds, out depth);
            depth *= Transform.Scale; // Assumes uniform scaling
            return result;
        }

        public bool Intersects(TransformedRectangle rect) => WorldBounds.Intersects(rect);

        public override bool Intersects(RectangleF bounds) => WorldBounds.Intersects(bounds);

        public override bool Intersects(in BoundingFrustum frustum) => frustum.Intersects(WorldBounds);

        public override bool Intersects(Collider other)
        {
            var distance = (WorldBounds.Center - other.WorldBounds.Center).Length();
            if (distance <= WorldBounds.Radius + other.WorldBounds.Radius)
            {
                if (other is BoxCollider box)
                {
                    return Intersects(box);
                }
                return true;
            }
            return false;
        }

        public bool Intersects(BoxCollider other) => Intersects(other.AxisAlignedBoundingArea)/* && Intersects(other.BoundingArea)*/;

        public override void DebugDraw(DrawingContext2D drawingContext, Camera camera)
        {
            var strokeColor = new ColorRgbaF(0, 0, 220, 255);

            var worldPos = Transform.Translation;
            //var rotation = Transform.Rotation;

            // AxisAlignedBoundingArea 
            var bottomLeft = new Vector3(AxisAlignedBoundingArea.X, AxisAlignedBoundingArea.Y, worldPos.Z);
            var bottomRight = new Vector3(AxisAlignedBoundingArea.X + AxisAlignedBoundingArea.Width, AxisAlignedBoundingArea.Y, worldPos.Z);
            var topLeft = new Vector3(AxisAlignedBoundingArea.X, AxisAlignedBoundingArea.Y + AxisAlignedBoundingArea.Height, worldPos.Z);
            var topRight = new Vector3(AxisAlignedBoundingArea.X + AxisAlignedBoundingArea.Width, AxisAlignedBoundingArea.Y + AxisAlignedBoundingArea.Height, worldPos.Z);

            var lbScreen = camera.WorldToScreenPoint(bottomLeft).Vector2XY();
            var rbScreen = camera.WorldToScreenPoint(bottomRight).Vector2XY();
            var ltScreen = camera.WorldToScreenPoint(topLeft).Vector2XY();
            var rtScreen = camera.WorldToScreenPoint(topRight).Vector2XY();

            drawingContext.DrawLine(new Line2D(ltScreen, lbScreen), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(lbScreen, rbScreen), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(rbScreen, rtScreen), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(rtScreen, ltScreen), 1, strokeColor);

            //TODO implement BoundingSphere
        }
    }

    public class CylinderCollider : SphereCollider
    {
        public CylinderCollider(ObjectDefinition def, Transform transform)
             : base(transform, def.Geometry.MajorRadius, def.Geometry.Height)
        {
        }

        public override void DebugDraw(DrawingContext2D drawingContext, Camera camera)
        {
            base.DebugDraw(drawingContext, camera);

            const int sides = 8;
            var lineColor = new ColorRgbaF(220, 220, 220, 255);

            var firstPoint = Vector2.Zero;
            var previousPoint = Vector2.Zero;

            for (var i = 0; i < sides; i++)
            {
                var angle = 2 * MathF.PI * i / sides;
                var point = Transform.Translation + new Vector3(MathF.Cos(angle), MathF.Sin(angle), 0) * SphereBounds.Radius;
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

    public class BoxCollider : SphereCollider
    {
        private readonly BoundingBox _boxBounds; // axis aligned!
        private BoundingBox _worldBoxBounds; // axis aligned!

        public BoxCollider(ObjectDefinition def, Transform transform)
            : base(transform, def.Geometry.MajorRadius, def.Geometry.Height)
        {
            var min = new Vector3(-def.Geometry.MajorRadius, -def.Geometry.MinorRadius, 0);
            var max = new Vector3(def.Geometry.MajorRadius, def.Geometry.MinorRadius, def.Geometry.Height);
            _boxBounds = new BoundingBox(min, max);
            Update(transform);
        }

        public BoxCollider(RectangleF rect) : base(rect)
        {
            _boxBounds = new BoundingBox(Vector3.Zero, new Vector3(rect.Width, rect.Height, 0));
            _worldBoxBounds = new BoundingBox(new Vector3(rect.X, rect.Y, 0), new Vector3(rect.X + rect.Width, rect.Y + rect.Height, 0));
            BoundingArea = TransformedRectangle.FromRectangle(rect);
            AxisAlignedBoundingArea = rect;
        }

        public sealed override void Update(Transform transform)
        {
            base.Update(transform);
            _worldBoxBounds = BoundingBox.Transform(_boxBounds, transform.Matrix);
            var width = _worldBoxBounds.Max.X - _worldBoxBounds.Min.X;
            var height = _worldBoxBounds.Max.Y - _worldBoxBounds.Min.Y;
            AxisAlignedBoundingArea = new RectangleF(_worldBoxBounds.Min.X, _worldBoxBounds.Min.Y, width, height);

            width = _boxBounds.Max.X - _boxBounds.Min.X;
            height = _boxBounds.Max.Y - _boxBounds.Min.Y;
            var rect = new RectangleF(_boxBounds.Min.X + transform.Translation.X, _boxBounds.Min.Y + transform.Translation.Y, width, height);
            BoundingArea = TransformedRectangle.FromRectangle(rect, transform.Rotation.Z);
        }

        public override bool Intersects(in Ray ray, out float depth)
        {
            if (!base.Intersects(in ray, out depth))
            {
                return false;
            }

            var result = ray.Intersects(_worldBoxBounds, out depth);
            depth *= Transform.Scale; // Assumes uniform scaling
            return result;
        }

        public override bool Intersects(RectangleF bounds)
        {
            return base.Intersects(bounds) && _worldBoxBounds.Intersects(bounds);
        }

        public override bool Intersects(in BoundingFrustum frustum) => frustum.Intersects(_worldBoxBounds);

        public override void DebugDraw(DrawingContext2D drawingContext, Camera camera)
        {
            base.DebugDraw(drawingContext, camera);

            var strokeColor = new ColorRgbaF(220, 220, 220, 255);

            var worldPos = Transform.Translation;
            var rotation = Transform.Rotation;

            var xLine = Vector3.Transform(new Vector3(_boxBounds.Max.X, 0, 0), rotation);
            var yLine = Vector3.Transform(new Vector3(0, _boxBounds.Max.Y, 0), rotation);

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

        public override bool Intersects(Collider other)
        {
            if (base.Intersects(other))
            {
                if (other is BoxCollider box)
                {
                    return Intersects(box);
                }
                return true;
            }
            return false;
        }

        // TODO: This ignores the Z dimension. Does that matter?
        public new bool Intersects(BoxCollider other) => base.Intersects(other) && BoundingArea.Intersects(other.BoundingArea);
    }
}
