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
        public abstract bool Contains(Vector3 point);

        public static Collider Create(GeometryShape geometryShape, Transform transform)
        {
            switch (geometryShape.Type)
            {
                case GeometryType.Box:
                    return new BoxCollider(geometryShape, transform);

                case GeometryType.Sphere:
                    return new SphereCollider(geometryShape, transform);

                case GeometryType.Cylinder:
                    return new CylinderCollider(geometryShape, transform);

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
    }


    public class SphereCollider : Collider
    {
        private readonly Vector3 _offset;
        public BoundingSphere SphereBounds { get; protected set; }

        public SphereCollider(GeometryShape geometry, Transform transform, float? radius = null, float offsetZ = 0.0f)
            : base(transform, geometry.Height)
        {
            radius ??= geometry.MajorRadius;
            Name = geometry.Name ?? "";
            _offset = geometry.Offset;
            SphereBounds = new BoundingSphere(Vector3.Zero + new Vector3(0, 0, offsetZ), radius.Value);
            Update(transform);
        }

        public SphereCollider(Transform transform, float radius)
            : base(transform, radius)
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
            Transform = new Transform(transform.Translation + Vector3.Transform(_offset, transform.Rotation), transform.Rotation);
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
            if (!Intersects(other.AxisAlignedBoundingArea))
            {
                return false;
            }
            // TODO: consider non-AA bounds of box collider
            return twoDimensional || WorldBounds.Intersects(other.WorldAABox);
        }

        public override bool Contains(Vector2 point) => WorldBounds.Contains(point);
        public override bool Contains(Vector3 point) => WorldBounds.Contains(point.X, point.Y, point.Z);

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
    }

    public class BoxCollider : SphereCollider
    {
        protected AxisAlignedBoundingBox _AABox { get; set; }
        public AxisAlignedBoundingBox WorldAABox { get; private set; }
        private BoundingBox _worldBox;

        // TODO: this is not a good representation for boxes with pitch and roll
        // but it works well for buildings (only have yaw) and updating the terrain passability
        public TransformedRectangle BoundingArea { get; protected set; }

        public BoxCollider(GeometryShape geometry, Transform transform)
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
            _worldBox = new BoundingBox(rect, height);
            WorldAABox = new AxisAlignedBoundingBox(new Vector3(rect.X, rect.Y, 0), new Vector3(rect.X + rect.Width, rect.Y + rect.Height, height));
            BoundingArea = TransformedRectangle.FromRectangle(rect);
            AxisAlignedBoundingArea = rect;
        }

        public sealed override void Update(Transform transform)
        {
            base.Update(transform);
            _worldBox = new BoundingBox(_AABox, transform.Matrix);
            WorldAABox = new AxisAlignedBoundingBox(_worldBox);

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

        public override bool Intersects(RectangleF bounds) => base.Intersects(bounds) && AxisAlignedBoundingArea.Intersects(bounds) && WorldAABox.Intersects(bounds);

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

            return twoDimensional || (WorldAABox.Intersects(other.WorldAABox) && _worldBox.Intersects(other._worldBox));
        }

        public override bool Contains(Vector2 point)
        {
            return base.Contains(point) && AxisAlignedBoundingArea.Contains(point) && BoundingArea.Contains(point);
        }

        public override bool Contains(Vector3 point)
        {
            return base.Contains(point) && AxisAlignedBoundingArea.Contains(point.X, point.Y) && _worldBox.Contains(point);
        }
    }

    public class CylinderCollider : BoxCollider
    {
        public float LowerRadius { get; private set; }
        public float UpperRadius { get; private set; }

        public CylinderCollider(GeometryShape geometry, Transform transform)
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

        public override bool Contains(Vector3 point)
        {
            if (!base.Contains(point))
            {
                return false;
            }
            return true; // TODO
        }
    }
}
