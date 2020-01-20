using System.Numerics;

namespace OpenSage.Mathematics
{
    public sealed class ClippingPlane
    {
        private Plane _plane;
        public Vector3 Normal
        {
            get { return _plane.Normal; }
            set
            {
                _plane = new Plane(value, D);
            }
        }
        public float D
        {
            get { return _plane.D; }
            set
            {
                _plane = new Plane(Normal, value);
            }
        }

        public ClippingPlane(in float n1, in float n2, in float n3, in float d)
        {
            _plane = new Plane(n1, n2, n3, d);
        }

        public ClippingPlane(in Vector3 normal, in float d)
        {
            _plane = new Plane(normal, d);
        }

        public ClippingPlane(in Vector4 normalWithD)
        {
            _plane = new Plane(normalWithD); ;
        }


        public ClippingPlane(in Plane plane)
        {
            _plane = plane;
        }

        public ClippingPlane CreateFromVertices(in Vector3 vt1, in Vector3 vt2, in Vector3 vt3)
        {
            _plane = Plane.CreateFromVertices(vt1, vt2, vt3);
            return this;
        }

        public Vector4 ConvertToVector4()
        {
            return new Vector4(Normal, D);
        }

        public float Dot(in Vector4 vector)
        {
            return Plane.Dot(_plane, vector);
        }
        public float DotCoordinate(in Vector3 vector)
        {
            return Plane.DotCoordinate(_plane, vector);
        }
        public float DotNormal(in Vector3 vector)
        {
            return Plane.DotNormal(_plane, vector);
        }

        public bool Equals(ClippingPlane other)
        {
            return (this == other);
        }
        public override bool Equals(object obj)
        {
            return (obj is ClippingPlane) && this == ((ClippingPlane) obj);
        }

        public override int GetHashCode() => _plane.GetHashCode();

        public void Normalize()
        {
            _plane = Plane.Normalize(_plane);
        }

        public bool Contains(in BoundingBox boundingBox)
        {
            var intersection = Intersects(boundingBox);
            if (intersection != PlaneIntersectionType.Back)
                return true;
            return false;
        }
        public bool Contains(in BoundingSphere sphere)
        {
            var intersection = Intersects(sphere);
            if (intersection != PlaneIntersectionType.Back)
                return true;
            return false;
        }

        public PlaneIntersectionType Intersects(in BoundingBox boundingBox)
        {
            return boundingBox.Intersects(_plane);
        }
        public PlaneIntersectionType Intersects(in BoundingSphere sphere)
        {
            return sphere.Intersects(_plane);
        }
        public override string ToString() => _plane.ToString();

        public void Transform(in Matrix4x4 transformationMatrix)
        {
            _plane = Plane.Transform(_plane, transformationMatrix);
        }
    }
}
