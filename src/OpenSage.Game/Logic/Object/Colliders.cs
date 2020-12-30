using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Gui;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class Collider
    {
        public string Name { get; protected set; }
        protected internal Transform Transform;
        public BoundingSphere WorldBounds { get; protected set; }
        public RectangleF AxisAlignedBoundingArea { get; protected set; }
        public float Height { get; protected set; }

        protected Collider(Transform transform, float height)
        {
            Name = "";
            Height = height;
            Transform = transform;
        }

        public abstract void Update(Transform transform);

        public abstract bool Intersects(in BoundingFrustum frustum);
        public abstract bool Intersects(in Ray ray, out float depth);
        public abstract bool Intersects(RectangleF bounds);
        public abstract bool Intersects(Collider collider, bool twoDimensional = false);

        public abstract bool Contains(Vector2 point);

        public static Collider Create(Geometry geometry, Transform transform)
        {
            switch (geometry.Type)
            {
                case ObjectGeometry.Box:
                    return new BoxCollider(geometry, transform);

                case ObjectGeometry.Sphere:
                    return new SphereCollider(geometry, transform);

                case ObjectGeometry.Cylinder:
                    return new CylinderCollider(geometry, transform);

                case ObjectGeometry.None:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Collider Create(List<Collider> colliders)
        {
            if (colliders.Count == 1)
            {
                return colliders[0];
            }

            var combinedSphere = colliders[0].WorldBounds;

            for (var i = 1; i < colliders.Count; i++)
            {
                combinedSphere = BoundingSphere.CreateMerged(combinedSphere, colliders[i].WorldBounds);
            }

            var transform = new Transform(combinedSphere.Center, Quaternion.Identity);
            return new SphereCollider(transform, combinedSphere.Radius);
        }

        public abstract void DebugDraw(DrawingContext2D drawingContext, Camera camera);
    }


    public class SphereCollider : Collider
    {
        private readonly Vector3 _offset;
        public BoundingSphere SphereBounds { get; protected set; }

        public SphereCollider(Geometry geometry, Transform transform, float? radius = null, float offsetZ = 0.0f)
            : base(transform, geometry.Height)
        {
            radius ??= geometry.MajorRadius;
            Name = geometry.Name ?? "";
            _offset = geometry.Offset + new Vector3(geometry.OffsetX, 0, 0);
            SphereBounds = new BoundingSphere(Vector3.Zero + new Vector3(0, 0, offsetZ), radius.Value);
            Update(transform);
        }

        public SphereCollider(Transform transform, float radius)
            : base(transform, radius)
        {
            SphereBounds = new BoundingSphere(Vector3.Zero, radius);
            Update(transform);
        }

        protected SphereCollider(Transform transform, float radius, float height)
            : base(transform, height)
        {
            SphereBounds = new BoundingSphere(Vector3.Zero, radius);
            Update(transform);
        }

        protected SphereCollider(RectangleF rect, float height)
            : base(new Transform(Matrix4x4.Identity), height)
        {
            var diagonal = new Vector3(rect.Width, rect.Height, height);
            var radius = diagonal.Length() / 2.0f;
            SphereBounds = new BoundingSphere(diagonal * 0.5f, radius);

            var center = new Vector3(rect.X, rect.Y, 0) + SphereBounds.Center;
            WorldBounds = new BoundingSphere(center, radius);
            AxisAlignedBoundingArea = rect;
        }

        public override void Update(Transform transform)
        {
            Transform = new Transform(transform.Translation + Vector3.Transform(_offset, transform.Rotation), Quaternion.Identity);
            WorldBounds = BoundingSphere.Transform(SphereBounds, Transform.Matrix);
            var width = WorldBounds.Radius * 2.0f;
            AxisAlignedBoundingArea = new RectangleF(Transform.Translation.Vector2XY() - new Vector2(SphereBounds.Radius, SphereBounds.Radius), width, width);
        }

        public override bool Intersects(in Ray ray, out float depth) => ray.Intersects(WorldBounds, out depth);

        public bool Intersects(TransformedRectangle rect) => WorldBounds.Intersects(rect);

        public override bool Intersects(RectangleF bounds) => WorldBounds.Intersects(bounds);

        public override bool Intersects(in BoundingFrustum frustum) => frustum.Intersects(WorldBounds);

        public override bool Intersects(Collider other, bool twoDimensional = false)
        {
            var distance = twoDimensional
                ? (WorldBounds.Center.Vector2XY() - other.WorldBounds.Center.Vector2XY()).Length()
                : (WorldBounds.Center - other.WorldBounds.Center).Length();

            if (distance <= WorldBounds.Radius + other.WorldBounds.Radius)
            {
                if (other is BoxCollider box)
                {
                    return Intersects(box, twoDimensional);
                }
                return true;
            }
            return false;
        }

        private bool Intersects(BoxCollider other, bool twoDimensional = false)
        {
            var val = !Intersects(other.AxisAlignedBoundingArea);
            var val2 = !Intersects(other.BoundingArea);
            if (!Intersects(other.AxisAlignedBoundingArea) || !Intersects(other.BoundingArea))
            {
                return false;
            }
            // TODO: consider non-AA bounds of box collider
            return twoDimensional || WorldBounds.Intersects(other.WorldAABox);
        }

        public override bool Contains(Vector2 point) => WorldBounds.Contains(point);

        protected void DebugDrawAxisAlignedBoundingArea(DrawingContext2D drawingContext, Camera camera)
        {
            var strokeColor = new ColorRgbaF(0, 0, 0, 255);
            var worldPos = Transform.Translation;

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
        }

        public override void DebugDraw(DrawingContext2D drawingContext, Camera camera)
        {
            DebugDrawAxisAlignedBoundingArea(drawingContext, camera);

            // Bounding Sphere
            const int sides = 8;
            var lineColor = new ColorRgbaF(220, 220, 220, 255);

            var firstPoint = Vector2.Zero;
            var previousPoint = Vector2.Zero;

            for (var i = 0; i < sides; i++)
            {
                var angle = 2 * MathF.PI * i / sides;
                var point = camera.WorldToScreenPoint(WorldBounds.Center + new Vector3(MathF.Cos(angle), MathF.Sin(angle), 0) * SphereBounds.Radius);
                var screenPoint = point.Vector2XY();

                // No line gets drawn on the first iteration
                if (i == 0)
                {
                    firstPoint = screenPoint;
                    previousPoint = screenPoint;
                    continue;
                }

                drawingContext.DrawLine(new Line2D(previousPoint, screenPoint), 1, lineColor);

                // If this is the last point, complete the circle
                if (i == sides - 1)
                {
                    drawingContext.DrawLine(new Line2D(screenPoint, firstPoint), 1, lineColor);
                }

                previousPoint = screenPoint;

                var firstPoint2 = Vector2.Zero;
                var previousPoint2 = Vector2.Zero;
                for (var j = 0; j < sides; j++)
                {
                    var angle2 = 2 * MathF.PI * j / sides;
                    var point2 = camera.WorldToScreenPoint(WorldBounds.Center + new Vector3(MathF.Sin(angle2) * MathF.Cos(angle), MathF.Sin(angle2) * MathF.Sin(angle), MathF.Cos(angle2)) * SphereBounds.Radius);
                    var screenPoint2 = point2.Vector2XY();

                    // No line gets drawn on the first iteration
                    if (j == 0)
                    {
                        firstPoint2 = screenPoint2;
                        previousPoint2 = screenPoint2;
                        continue;
                    }

                    drawingContext.DrawLine(new Line2D(previousPoint2, screenPoint2), 1, lineColor);

                    // If this is the last point, complete the circle
                    if (j == sides - 1)
                    {
                        drawingContext.DrawLine(new Line2D(screenPoint2, firstPoint2), 1, lineColor);
                    }

                    previousPoint2 = screenPoint2;
                }
            }
        }
    }

    public class BoxCollider : SphereCollider
    {
        protected AxisAlignedBoundingBox _AABox { get; set; }
        public AxisAlignedBoundingBox WorldAABox { get; private set; }
        private BoundingBox _worldBox;

        public TransformedRectangle BoundingArea { get; protected set; }

        public BoxCollider(Geometry geometry, Transform transform)
            : base(geometry, transform)
        {
            var min = new Vector3(-geometry.MajorRadius, -geometry.MinorRadius, 0);
            var max = new Vector3(geometry.MajorRadius, geometry.MinorRadius, geometry.Height);
            _AABox = new AxisAlignedBoundingBox(min, max);
            SphereBounds = new BoundingSphere(Vector3.Zero + new Vector3(0, 0, geometry.Height / 2.0f), (max - min).Length() / 2.0f);
            Update(transform);
        }

        public BoxCollider(RectangleF rect, float height = 1.0f) : base(rect, height)
        {
            _AABox = new AxisAlignedBoundingBox(Vector3.Zero, new Vector3(rect.Width, rect.Height, height));
            _worldBox = new BoundingBox(new Vector3(rect.X, rect.Y, 0), new Vector3(rect.X + rect.Width, rect.Y + rect.Height, height));
            WorldAABox = new AxisAlignedBoundingBox(new Vector3(rect.X, rect.Y, 0), new Vector3(rect.X + rect.Width, rect.Y + rect.Height, height));
            BoundingArea = TransformedRectangle.FromRectangle(rect);
            AxisAlignedBoundingArea = rect;
        }

        public sealed override void Update(Transform transform)
        {
            base.Update(transform);
            var min = Vector3.Transform(_AABox.Min, transform.Matrix);
            var max = Vector3.Transform(_AABox.Max, transform.Matrix);
            _worldBox = new BoundingBox(min, max);
            WorldAABox = AxisAlignedBoundingBox.Transform(_AABox, Transform.Matrix);

            // TODO: improve this
            var width = WorldAABox.Max.X - WorldAABox.Min.X;
            var height = WorldAABox.Max.Y - WorldAABox.Min.Y;
            AxisAlignedBoundingArea = new RectangleF(WorldAABox.Min.X, WorldAABox.Min.Y, width, height);

            width = _AABox.Max.X - _AABox.Min.X;
            height = _AABox.Max.Y - _AABox.Min.Y;
            var rect = new RectangleF(_AABox.Min.X + Transform.Translation.X, _AABox.Min.Y + Transform.Translation.Y, width, height);
            BoundingArea = TransformedRectangle.FromRectangle(rect, Transform.Yaw);
        }

        public override bool Intersects(in Ray ray, out float depth)
        {
            if (!base.Intersects(in ray, out depth))
            {
                return false;
            }

            var result = ray.Intersects(WorldAABox, out depth);
            depth *= Transform.Scale; // Assumes uniform scaling
            return result;
        }

        public override bool Intersects(RectangleF bounds) => base.Intersects(bounds) && WorldAABox.Intersects(bounds);

        public override bool Intersects(in BoundingFrustum frustum) => frustum.Intersects(WorldAABox);

        public override bool Intersects(Collider other, bool twoDimensional = false)
        {
            if (base.Intersects(other, twoDimensional))
            {
                if (other is BoxCollider box)
                {
                    return Intersects(box, twoDimensional);
                }
                return true;
            }
            return false;
        }

        private bool Intersects(BoxCollider other, bool twoDimensional = false)
        {
            if (!base.Intersects(other, twoDimensional) || !BoundingArea.Intersects(other.BoundingArea))
            {
                return false;
            }

            // TODO: also consider non-AA box _worldBox
            return twoDimensional || WorldAABox.Intersects(other.WorldAABox);
        }

        public override bool Contains(Vector2 point)
        {
            return base.Contains(point) && AxisAlignedBoundingArea.Contains(point) && BoundingArea.Contains(point);
        }

        private void DrawBox(DrawingContext2D drawingContext, Camera camera, ColorRgbaF strokeColor, Vector3 worldPos, Vector3 xLine, Vector3 yLine)
        {
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

            var height = new Vector3(0, 0, Height);
            var ltWorldTop = worldPos + (leftSide - topSide) + height;
            var rtWorldTop = worldPos + (rightSide - topSide) + height;
            var rbWorldTop = worldPos + (rightSide - bottomSide) + height;
            var lbWorldTop = worldPos + (leftSide - bottomSide) + height;

            var ltScreenTop = camera.WorldToScreenPoint(ltWorldTop).Vector2XY();
            var rtScreenTop = camera.WorldToScreenPoint(rtWorldTop).Vector2XY();
            var rbScreenTop = camera.WorldToScreenPoint(rbWorldTop).Vector2XY();
            var lbScreenTop = camera.WorldToScreenPoint(lbWorldTop).Vector2XY();

            drawingContext.DrawLine(new Line2D(ltScreenTop, lbScreenTop), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(lbScreenTop, rbScreenTop), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(rbScreenTop, rtScreenTop), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(rtScreenTop, ltScreenTop), 1, strokeColor);

            drawingContext.DrawLine(new Line2D(ltScreen, ltScreenTop), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(lbScreen, lbScreenTop), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(rbScreen, rbScreenTop), 1, strokeColor);
            drawingContext.DrawLine(new Line2D(rtScreen, rtScreenTop), 1, strokeColor);
        }

        private void DebugDrawAxisAlignedBoundingBox(DrawingContext2D drawingContext, Camera camera)
        {
            var strokeColor = new ColorRgbaF(0, 0, 255, 255);

            var xLine = new Vector3((WorldAABox.Max.X - WorldAABox.Min.X) / 2.0f, 0, 0);
            var yLine = new Vector3(0, (WorldAABox.Max.Y - WorldAABox.Min.Y) / 2.0f, 0);

            DrawBox(drawingContext, camera, strokeColor, Transform.Translation, xLine, yLine);
        }

        public override void DebugDraw(DrawingContext2D drawingContext, Camera camera)
        {
            //base.DebugDraw(drawingContext, camera);
            //DebugDrawAxisAlignedBoundingArea(drawingContext, camera);

            DebugDrawAxisAlignedBoundingBox(drawingContext, camera);

            var strokeColor = new ColorRgbaF(220, 220, 220, 255);

            var rotation = Transform.Rotation;

            var xLine = Vector3.Transform(new Vector3((_AABox.Max.X - _AABox.Min.X) / 2.0f, 0, 0), rotation);
            var yLine = Vector3.Transform(new Vector3(0, (_AABox.Max.Y - _AABox.Min.Y) / 2.0f, 0), rotation);

            DrawBox(drawingContext, camera, strokeColor, Transform.Translation, xLine, yLine);
        }
    }

    public class CylinderCollider : BoxCollider
    {
        public float LowerRadius { get; private set; }
        public float UpperRadius { get; private set; }

        public CylinderCollider(Geometry geometry, Transform transform)
            : base(geometry, transform)
        {
            LowerRadius = geometry.MajorRadius;
            UpperRadius = geometry.MinorRadius;

            var dimension = Math.Max(geometry.MinorRadius, geometry.MajorRadius);
            var min = new Vector3(-dimension, -dimension, 0);
            var max = new Vector3(dimension, dimension, geometry.Height);
            _AABox = new AxisAlignedBoundingBox(min, max);
            SphereBounds = new BoundingSphere(Vector3.Zero + new Vector3(0, 0, geometry.Height / 2.0f), (max - min).Length() / 2.0f);
            Update(transform);
        }

        public override bool Contains(Vector2 point)
        {
            if (!base.Contains(point))
            {
                return false;
            }
            var distance = (Transform.Translation.Vector2XY() - point).Length();
            return distance <= LowerRadius;
        }

        public override void DebugDraw(DrawingContext2D drawingContext, Camera camera)
        {
            //base.DebugDraw(drawingContext, camera);

            const int sides = 8;
            var lineColor = new ColorRgbaF(220, 220, 220, 255);

            var firstPoint = Vector2.Zero;
            var previousPoint = Vector2.Zero;
            var firstPointTop = Vector2.Zero;
            var previousPointTop = Vector2.Zero;

            for (var i = 0; i < sides; i++)
            {
                var angle = 2 * MathF.PI * i / sides;
                var point = Transform.Translation + new Vector3(MathF.Cos(angle), MathF.Sin(angle), 0) * LowerRadius;
                var screenPoint = camera.WorldToScreenPoint(point).Vector2XY();
                var pointTop = Transform.Translation + new Vector3(0, 0, Height) + new Vector3(MathF.Cos(angle), MathF.Sin(angle), 0) * UpperRadius;
                var screenPointTop = camera.WorldToScreenPoint(pointTop).Vector2XY();

                // No line gets drawn on the first iteration
                if (i == 0)
                {
                    firstPoint = screenPoint;
                    previousPoint = screenPoint;
                    firstPointTop = screenPointTop;
                    previousPointTop = screenPointTop;
                    continue;
                }

                drawingContext.DrawLine(new Line2D(previousPoint, screenPoint), 1, lineColor);
                drawingContext.DrawLine(new Line2D(previousPointTop, screenPointTop), 1, lineColor);
                drawingContext.DrawLine(new Line2D(previousPoint, previousPointTop), 1, lineColor);

                // If this is the last point, complete the cylinder
                if (i == sides - 1)
                {
                    drawingContext.DrawLine(new Line2D(screenPoint, firstPoint), 1, lineColor);
                    drawingContext.DrawLine(new Line2D(screenPointTop, firstPointTop), 1, lineColor);
                    drawingContext.DrawLine(new Line2D(screenPoint, screenPointTop), 1, lineColor);
                }

                previousPoint = screenPoint;
                previousPointTop = screenPointTop;
            }
        }
    }
}
