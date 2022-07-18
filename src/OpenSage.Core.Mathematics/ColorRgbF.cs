using System.Numerics;

namespace OpenSage.Mathematics
{
    public readonly struct ColorRgbF
    {
        public readonly float R;
        public readonly float G;
        public readonly float B;

        public ColorRgbF(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(R, G, B);
        }
    }
}
