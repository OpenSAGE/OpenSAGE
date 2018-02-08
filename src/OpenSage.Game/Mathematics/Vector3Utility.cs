using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class Vector3Utility
    {
        public static Vector3 Lerp(in Vector3 x, in Vector3 y, float s)
        {
            return x + s * (y - x);
        }

        //  From https://keithmaggio.wordpress.com/2011/02/15/math-magician-lerp-slerp-and-nlerp/
        public static Vector3 Slerp(in Vector3 start, in Vector3 end, float percent)
        {
            // Dot product - the cosine of the angle between 2 vectors.
            float dot = Vector3.Dot(start, end);
            // Clamp it to be in the range of Acos()
            // This may be unnecessary, but floating point
            // precision can be a fickle mistress.
            dot = MathUtility.Clamp(dot, -1.0f, 1.0f);
            // Acos(dot) returns the angle between start and end,
            // And multiplying that by percent returns the angle between
            // start and the final result.
            float theta = MathUtility.Acos(dot) * percent;
            Vector3 RelativeVec = end - start * dot;
            RelativeVec = Vector3.Normalize(RelativeVec);     // Orthonormal basis
                                         // The final result.
            return ((start * MathUtility.Cos(theta)) + (RelativeVec * MathUtility.Sin(theta)));
        }

        public static Vector3 WithX(this Vector3 vector, float x)
        {
            vector.X = x;
            return vector;
        }

        public static Vector3 WithY(this Vector3 vector, float y)
        {
            vector.Y = y;
            return vector;
        }

        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            vector.Z = z;
            return vector;
        }
    }
}
