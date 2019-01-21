using System.Numerics;

namespace OpenSage.Mathematics
{
    /// <summary>
    /// Represents a 2D line.
    /// </summary>
    public readonly struct Line2D
    {
        public readonly Vector2 V0;
        public readonly Vector2 V1;

        public Line2D(in Vector2 v0, in Vector2 v1)
        {
            V0 = v0;
            V1 = v1;
        }

        public static Line2D Transform(in Line2D line, in Matrix3x2 matrix)
        {
            return new Line2D(
                Vector2.Transform(line.V0, matrix),
                Vector2.Transform(line.V1, matrix));
        }
    }
}
