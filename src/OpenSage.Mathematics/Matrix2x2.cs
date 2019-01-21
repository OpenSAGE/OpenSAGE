using System.Runtime.InteropServices;

namespace OpenSage.Mathematics
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Matrix2x2
    {
        public readonly float M11;
        public readonly float M21;
        public readonly float M12;
        public readonly float M22;

        public Matrix2x2(float m11, float m12, float m21, float m22)
        {
            M11 = m11;
            M21 = m21;
            M12 = m12;
            M22 = m22;
        }
    }
}
