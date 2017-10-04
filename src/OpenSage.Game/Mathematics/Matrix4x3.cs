using System.Runtime.InteropServices;

namespace OpenSage.Mathematics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4x3
    {
        public float M11;
        public float M21;
        public float M31;
        public float M41;

        public float M12;
        public float M22;
        public float M32;
        public float M42;

        public float M13;
        public float M23;
        public float M33;
        public float M43;
    }
}
