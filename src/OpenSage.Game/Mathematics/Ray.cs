using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public struct Ray
    {
        public Vector3 Position;
        public Vector3 Direction;

        public Ray(Vector3 position, Vector3 direction)
        {
            Position = position;
            Direction = direction;
        }

        public bool Intersects(in BoundingBox box, out float t)
        {
            // Based on https://tavianator.com/fast-branchless-raybounding-box-intersections/

            var tMin = float.MinValue;
            var tMax = float.MaxValue;

            var v1 = (box.Min - Position) / Direction;
            var v2 = (box.Max - Position) / Direction;

            var vMin = Vector3.Min(v1, v2);
            var vMax = Vector3.Max(v1, v2);

            tMin = Math.Max(tMin, vMin.X);
            tMin = Math.Max(tMin, vMin.Y);
            tMin = Math.Max(tMin, vMin.Z);

            tMax = Math.Min(tMax, vMax.X);
            tMax = Math.Min(tMax, vMax.Y);
            tMax = Math.Min(tMax, vMax.Z);

            t = tMin;

            return tMax >= tMin;
        }

        public bool Intersects(in BoundingSphere sphere, out float t)
        {
            // Based on https://gamedev.stackexchange.com/a/96469

            var p = Position - sphere.Center;

            var rSquared = sphere.Radius * sphere.Radius;
            var pDot = Vector3.Dot(p, Direction);

            if (pDot > 0 || Vector3.Dot(p, p) < rSquared)
            {
                t = 0;
                return false;
            }

            var a = p - pDot * Direction;

            var aSquared = Vector3.Dot(a, a);

            if (aSquared > rSquared)
            {
                t = 0;
                return false;
            }

            var h = (float) Math.Sqrt(rSquared - aSquared);
            var i = a - h * Direction;

            // Intersection point. Could be useful?
            // var intersection = Position + i;

            t = i.Length();
            return true;
        }

        public float? Intersects(ref Plane plane)
        {
            var den = Vector3.Dot(Direction, plane.Normal);
            if (Math.Abs(den) < 0.00001f)
            {
                return null;
            }

            var result = (-plane.D - Vector3.Dot(plane.Normal, Position)) / den;

            if (result < 0.0f)
            {
                if (result < -0.00001f)
                {
                    return null;
                }

                result = 0.0f;
            }

            return result;
        }

        public Ray Transform(in Matrix4x4 world)
        {
            return new Ray(
                Vector3.Transform(Position, world),
                Vector3.TransformNormal(Direction, world));
        }

        /// <summary>
        /// From XNA "Picking With Triangle Accuracy" sample.
        /// http://xbox.create.msdn.com/en-US/education/catalog/sample/picking_triangle
        /// 
        /// Checks whether a ray intersects a triangle. This uses the algorithm
        /// developed by Tomas Moller and Ben Trumbore, which was published in the
        /// Journal of Graphics Tools, volume 2, "Fast, Minimum Storage Ray-Triangle
        /// Intersection".
        public bool Intersects(
            in Triangle triangle,
            out float? result)
        {
            // Compute vectors along two edges of the triangle.
            var edge1 = Vector3.Subtract(triangle.V1, triangle.V0);
            var edge2 = Vector3.Subtract(triangle.V2, triangle.V0);

            // Compute the determinant.
            var directionCrossEdge2 = Vector3.Cross(Direction, edge2);

            var determinant = Vector3.Dot(edge1, directionCrossEdge2);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                result = null;
                return false;
            }

            float inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            var distanceVector = Vector3.Subtract(Position, triangle.V0);

            var triangleU = Vector3.Dot(distanceVector, directionCrossEdge2);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                result = null;
                return false;
            }

            // Calculate the V parameter of the intersection point.
            var distanceCrossEdge1 = Vector3.Cross(distanceVector, edge1);

            var triangleV = Vector3.Dot(Direction, distanceCrossEdge1);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                result = null;
                return false;
            }

            // Compute the distance along the ray to the triangle.
            var rayDistance = Vector3.Dot(edge2, distanceCrossEdge1);
            rayDistance *= inverseDeterminant;

            // Is the triangle behind the ray origin?
            if (rayDistance < 0)
            {
                result = null;
                return false;
            }

            result = rayDistance;
            return true;
        }

        public override string ToString()
        {
            return $"{nameof(Position)}: {Position}, {nameof(Direction)}: {Direction}";
        }
    }
}
