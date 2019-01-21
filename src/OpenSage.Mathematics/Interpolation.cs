using System.Numerics;

namespace OpenSage.Mathematics
{
    public static class Interpolation
    {
        // From http://paulbourke.net/miscellaneous/interpolation/
        public static float CatmullRom(float a, float b, float c, float d, float t)
        {
            var t2 = t * t;
            var a0 = -0.5f * a + 1.5f * b - 1.5f * c + 0.5f * d;
            var a1 = a - 2.5f * b + 2.0f * c - 0.5f * d;
            var a2 = -0.5f * a + 0.5f * c;
            var a3 = b;

            return a0 * t * t2 + a1 * t2 + a2 * t + a3;
        }

        public static Vector3 CatmullRom(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d, float t)
        {
            return new Vector3(
                CatmullRom(a.X, b.X, c.X, d.X, t),
                CatmullRom(a.Y, b.Y, c.Y, d.Y, t),
                CatmullRom(a.Z, b.Z, c.Z, d.Z, t)
            );
        }
    }
}
