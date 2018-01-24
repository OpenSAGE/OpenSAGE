using System.Numerics;

namespace OpenSage.Mathematics
{
    /// <summary>
    /// Represents a 2D line.
    /// </summary>
    public struct Line2D
    {
        public Vector2 V0;
        public Vector2 V1;

        public static Line2D Transform(in Line2D line, in Matrix3x2 matrix)
        {
            return new Line2D
            {
                V0 = Vector2.Transform(line.V0, matrix),
                V1 = Vector2.Transform(line.V1, matrix)
            };
        }
    }
}
