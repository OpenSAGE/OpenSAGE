using System.Numerics;

namespace OpenSage.Graphics.Effects
{
    internal static class PointerUtil
    {
        public static unsafe void CopyToMatrix4x3(ref Matrix4x4 matrix, float* destPtr)
        {
            *(destPtr + 0) = matrix.M11;
            *(destPtr + 1) = matrix.M21;
            *(destPtr + 2) = matrix.M31;
            *(destPtr + 3) = matrix.M41;

            *(destPtr + 4) = matrix.M12;
            *(destPtr + 5) = matrix.M22;
            *(destPtr + 6) = matrix.M32;
            *(destPtr + 7) = matrix.M42;

            *(destPtr + 8) = matrix.M13;
            *(destPtr + 9) = matrix.M23;
            *(destPtr + 10) = matrix.M33;
            *(destPtr + 11) = matrix.M43;
        }
    }
}
