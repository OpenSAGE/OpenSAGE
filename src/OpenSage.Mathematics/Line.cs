using System.Numerics;
using System;

namespace OpenSage.Mathematics
{
    /// <summary>
    /// Represents a 2D line.
    /// </summary>
    public readonly struct Line2D
    {
        public readonly Vector2 V0;
        public readonly Vector2 V1;

        public readonly RectangleF BoundingBox;

        // used to judge the distance to point
        private readonly float A, B, C, S;

        public Line2D(in Vector2 v0, in Vector2 v1)
        {
            V0 = v0;
            V1 = v1;
            var dv = v1 - v0;
            A = dv.Y;
            B = -(dv.X);
            C = (dv.X * v0.Y) - (dv.Y * v0.X);
            S = (float) Math.Sqrt(A * A + B * B);
            BoundingBox = new RectangleF(Math.Min(v0.X, v1.X), Math.Min(v0.Y, v1.Y), Math.Abs(dv.X), Math.Abs(dv.Y));
        }

        public static Line2D Transform(in Line2D line, in Matrix3x2 matrix)
        {
            return new Line2D(
                Vector2.Transform(line.V0, matrix),
                Vector2.Transform(line.V1, matrix));
        }

        /// <summary>
        /// return if point is in the area around the line with radius distance. 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool Contains(Vector2 point, int distance = 1)
        {
            var res = false;
            if (BoundingBox.Contains(point.X, point.Y, distance))
            {
                var line_dis = Math.Abs(A * point.X + B * point.Y + C) / S;
                res = line_dis <= distance;
            }
            return res;
        }
    }
}
