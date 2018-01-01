using System.Runtime.InteropServices;

namespace OpenSage.LowLevel.Graphics2D
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct RawMatrix3x2
    {

        public float M11;
        public float M12;
        public float M21;
        public float M22;
        public float M31;
        public float M32;

        public RawMatrix3x2(float m11, float m12, float m21, float m22, float m31, float m32)
        {
            M11 = m11;
            M12 = m12;
            M21 = m21;
            M22 = m22;
            M31 = m31;
            M32 = m32;
        }

    }
}
