using System;
using System.Numerics;

namespace OpenSage.Mathematics
{
    public struct Point3D
    {
        public static readonly Point3D Zero = new Point3D(0, 0, 0);

        public float X;
        public float Y;
        public float Z;

        public Point3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"<{X}, {Y}, {Z}>";
        }

        public static Point3D operator+(Point3D a, Point3D b)
        {
            return new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Point3D operator-(Point3D a, Point3D b)
        {
            return new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }
    }
}
