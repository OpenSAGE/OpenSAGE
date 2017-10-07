using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class Vector3Utility
    {
        public static Vector3 Lerp(ref Vector3 x, ref Vector3 y, float s)
        {
            return x + s * (y - x);
        }

        //  From https://keithmaggio.wordpress.com/2011/02/15/math-magician-lerp-slerp-and-nlerp/
        public static Vector3 Slerp(ref Vector3 start, ref Vector3 end, float percent)
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
    }
}
