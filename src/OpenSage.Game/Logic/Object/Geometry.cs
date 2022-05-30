using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Diagnostics.Util;
using OpenSage.Graphics.Cameras;
using OpenSage.Gui;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class Geometry : IPersistableObject
    {
        private float _boundingCircleRadius;
        private float _boundingSphereRadius;

        public bool IsSmall;

        public readonly List<GeometryShape> Shapes;

        public float BoundingCircleRadius => _boundingCircleRadius;
        public float BoundingSphereRadius => _boundingSphereRadius;

        public Geometry()
        {
            Shapes = new List<GeometryShape>
            {
                new GeometryShape(this)
            };
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            if (Shapes.Count != 1)
            {
                throw new InvalidStateException();
            }

            var shape = Shapes[0];

            var type = shape.Type;
            reader.PersistEnum(ref type);
            shape.Type = type;

            reader.PersistBoolean(ref IsSmall);

            var height = shape.Height;
            reader.PersistSingle(ref height);
            shape.Height = height;

            var majorRadius = shape.MajorRadius;
            reader.PersistSingle(ref majorRadius);
            shape.MajorRadius = majorRadius;

            var minorRadius = shape.MinorRadius;
            reader.PersistSingle(ref minorRadius);
            shape.MinorRadius = minorRadius;

            reader.PersistSingle(ref _boundingCircleRadius);
            reader.PersistSingle(ref _boundingSphereRadius);
        }

        public Geometry Clone()
        {
            var result = new Geometry();

            result._boundingCircleRadius = _boundingCircleRadius;
            result._boundingSphereRadius = _boundingSphereRadius;
            result.IsSmall = IsSmall;

            result.Shapes.Clear();

            foreach (var geometryShape in Shapes)
            {
                result.Shapes.Add(geometryShape.Clone());
            }

            return result;
        }

        internal void OnShapeChanged()
        {
            _boundingCircleRadius = _boundingSphereRadius = 0.0f;

            foreach (var shape in Shapes)
            {
                float boundingCircleRadius, boundingSphereRadius;

                switch (shape.Type)
                {
                    case GeometryType.Sphere:
                        boundingCircleRadius = boundingSphereRadius = shape.MajorRadius;
                        break;

                    case GeometryType.Cylinder:
                        boundingCircleRadius = shape.MajorRadius;
                        boundingSphereRadius = Math.Max(shape.MajorRadius, shape.Height / 2.0f);
                        break;

                    case GeometryType.Box:
                        var radiusSquared2D = shape.MajorRadius * shape.MajorRadius + shape.MinorRadius * shape.MinorRadius;
                        boundingCircleRadius = MathF.Sqrt(radiusSquared2D);
                        var halfHeight = shape.Height / 2.0f;
                        boundingSphereRadius = MathF.Sqrt(radiusSquared2D + halfHeight * halfHeight);
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                _boundingCircleRadius = Math.Max(_boundingCircleRadius, boundingCircleRadius);
                _boundingSphereRadius = Math.Max(_boundingSphereRadius, boundingSphereRadius);
            }
        }

        internal void DebugDraw(DrawingContext2D drawingContext, Camera camera, GameObject parent)
        {
            var collideInfo = GeometryCollisionDetectionUtility.CreateCollideInfo(parent);

            foreach (var shape in Shapes)
            {
                var shapeCollideInfo = GeometryCollisionDetectionUtility.CreateShapeInfo(shape, collideInfo);

                switch (shape.Type)
                {
                    case GeometryType.Sphere:
                        drawingContext.DrawSphere(camera, shapeCollideInfo.CreateSphere());
                        break;

                    case GeometryType.Cylinder:
                        drawingContext.DrawCylinder(camera, shapeCollideInfo.CreateCylinder());
                        break;

                    case GeometryType.Box:
                        drawingContext.DrawBox(camera, shapeCollideInfo.CreateBox());
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }

    public sealed class GeometryShape
    {
        private readonly Geometry _geometry;

        private GeometryType _type;
        private float _height;
        private float _majorRadius;
        private float _minorRadius;

        public GeometryType Type
        {
            get => _type;
            set
            {
                _type = value;
                OnChanged();
            }
        }

        public float Height
        {
            get => _height;
            set
            {
                _height = value;
                OnChanged();
            }
        }

        public float MajorRadius
        {
            get => _majorRadius;
            set
            {
                _majorRadius = value;
                OnChanged();
            }
        }

        public float MinorRadius
        {
            get => _minorRadius;
            set
            {
                _minorRadius = value;
                OnChanged();
            }
        }

        public Vector3 Offset;
        public bool IsActive;
        public float FrontAngle;
        public string Name;

        internal GeometryShape(Geometry geometry)
        {
            _geometry = geometry;

            _type = GeometryType.Sphere;
            _majorRadius = 1.0f;
            _minorRadius = 1.0f;
            _height = 1.0f;
            IsActive = true;
        }

        private void OnChanged()
        {
            _geometry.OnShapeChanged();
        }

        public GeometryShape Clone() => (GeometryShape)MemberwiseClone();
    }

    internal static class GeometryCollisionDetectionUtility
    {
        public static GeometryCollideInfo CreateCollideInfo(GameObject gameObject)
        {
            return new GeometryCollideInfo(gameObject.Geometry, gameObject.Translation, gameObject.Yaw);
        }

        public static GeometryShapeCollideInfo CreateShapeInfo(GeometryShape shape, GeometryCollideInfo collideInfo)
        {
            return new GeometryShapeCollideInfo(
                shape,
                collideInfo.Position + shape.Offset, // TODO: Not right, needs to use object's orientation
                collideInfo.Angle);
        }

        public static bool Intersects(in GeometryCollideInfo left, in GeometryCollideInfo right)
        {
            foreach (var leftShape in left.Geometry.Shapes)
            {
                var leftShapeInfo = CreateShapeInfo(leftShape, left);

                foreach (var rightShape in right.Geometry.Shapes)
                {
                    var rightShapeInfo = CreateShapeInfo(rightShape, right);

                    if (Intersects(leftShapeInfo, rightShapeInfo))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool Intersects(in GeometryShapeCollideInfo shape1, in GeometryShapeCollideInfo shape2)
        {
            var intersectionDelegate = IntersectionDelegates[(shape1.Shape.Type, shape2.Shape.Type)];
            return intersectionDelegate(shape1, shape2);
        }

        internal readonly record struct GeometryShapeCollideInfo(GeometryShape Shape, Vector3 Position, float Angle)
        {
            public SphereShape CreateSphere()
            {
                return new SphereShape(Position, Shape.MajorRadius);
            }

            public CylinderShape CreateCylinder()
            {
                return new CylinderShape(Position, Shape.Height, Shape.MajorRadius);
            }

            public BoxShape CreateBox()
            {
                return new BoxShape(Position, Angle, Shape.MajorRadius, Shape.MinorRadius, Shape.Height);
            }
        }

        private delegate bool IntersectionDelegate(in GeometryShapeCollideInfo shape1, in GeometryShapeCollideInfo shape2);

        private static readonly Dictionary<(GeometryType, GeometryType), IntersectionDelegate> IntersectionDelegates = new Dictionary<(GeometryType, GeometryType), IntersectionDelegate>
        {
            { (GeometryType.Sphere, GeometryType.Sphere), IntersectsSphereSphere },
            { (GeometryType.Sphere, GeometryType.Cylinder), IntersectsSphereCylinder },
            { (GeometryType.Sphere, GeometryType.Box), IntersectsSphereBox },
            { (GeometryType.Cylinder, GeometryType.Sphere), IntersectsCylinderSphere },
            { (GeometryType.Cylinder, GeometryType.Cylinder), IntersectsCylinderCylinder },
            { (GeometryType.Cylinder, GeometryType.Box), IntersectsCylinderBox },
            { (GeometryType.Box, GeometryType.Sphere), IntersectsBoxSphere },
            { (GeometryType.Box, GeometryType.Cylinder), IntersectsBoxCylinder },
            { (GeometryType.Box, GeometryType.Box), IntersectsBoxBox },
        };

        private static bool IntersectsSphereSphere(in GeometryShapeCollideInfo shape1, in GeometryShapeCollideInfo shape2)
        {
            return CollisionDetection.IntersectSphereSphere(shape1.CreateSphere(), shape2.CreateSphere());
        }

        private static bool IntersectsSphereCylinder(in GeometryShapeCollideInfo shape1, in GeometryShapeCollideInfo shape2)
        {
            return CollisionDetection.IntersectSphereCylinder(shape1.CreateSphere(), shape2.CreateCylinder());
        }

        private static bool IntersectsSphereBox(in GeometryShapeCollideInfo shape1, in GeometryShapeCollideInfo shape2)
        {
            return CollisionDetection.IntersectSphereBox(shape1.CreateSphere(), shape2.CreateBox());
        }

        private static bool IntersectsCylinderSphere(in GeometryShapeCollideInfo shape1, in GeometryShapeCollideInfo shape2)
        {
            return CollisionDetection.IntersectSphereCylinder(shape2.CreateSphere(), shape1.CreateCylinder());
        }

        private static bool IntersectsCylinderCylinder(in GeometryShapeCollideInfo shape1, in GeometryShapeCollideInfo shape2)
        {
            return CollisionDetection.IntersectCylinderCylinder(shape1.CreateCylinder(), shape2.CreateCylinder());
        }

        private static bool IntersectsCylinderBox(in GeometryShapeCollideInfo shape1, in GeometryShapeCollideInfo shape2)
        {
            return CollisionDetection.IntersectCylinderBox(shape1.CreateCylinder(), shape2.CreateBox());
        }

        private static bool IntersectsBoxSphere(in GeometryShapeCollideInfo shape1, in GeometryShapeCollideInfo shape2)
        {
            return CollisionDetection.IntersectSphereBox(shape2.CreateSphere(), shape1.CreateBox());
        }

        private static bool IntersectsBoxCylinder(in GeometryShapeCollideInfo shape1, in GeometryShapeCollideInfo shape2)
        {
            return CollisionDetection.IntersectCylinderBox(shape2.CreateCylinder(), shape1.CreateBox());
        }

        private static bool IntersectsBoxBox(in GeometryShapeCollideInfo shape1, in GeometryShapeCollideInfo shape2)
        {
            return CollisionDetection.IntersectBoxBox(shape1.CreateBox(), shape2.CreateBox());
        }
    }

    internal readonly record struct GeometryCollideInfo(Geometry Geometry, Vector3 Position, float Angle);
}
