using System.Numerics;

namespace OpenSage.Mathematics
{
    internal static class Matrix4x4Utility
    {
        public static Matrix4x4 Invert(in Matrix4x4 value)
        {
            if (!Matrix4x4.Invert(value, out var result))
            {
                throw new System.InvalidOperationException();
            }
            return result;
        }

        public static Vector3 GetTranslation(in Matrix4x4 value)
        {
            return new Vector3(value.M41, value.M42, value.M43);
        }

        /// <summary>
        /// The forward vector formed from the third row -M31, -M32, -M33 elements.
        /// </summary>
        public static Vector3 Forward(in this Matrix4x4 value)
        {
            return new Vector3(-value.M31, -value.M32, -value.M33);
        }

        /// <summary>
        /// The backward vector formed from the third row M31, M32, M33 elements.
        /// </summary>
        public static Vector3 Backward(in this Matrix4x4 value)
        {
            return new Vector3(value.M31, value.M32, value.M33);
        }

        /// <summary>
        /// The left vector formed from the first row -M11, -M12, -M13 elements.
        /// </summary>
        public static Vector3 Left(in this Matrix4x4 value)
        {
            return new Vector3(-value.M11, -value.M12, -value.M13);
        }

        /// <summary>
        /// The right vector formed from the first row M11, M12, M13 elements.
        /// </summary>
        public static Vector3 Right(in this Matrix4x4 value)
        {
            return new Vector3(value.M11, value.M12, value.M13);
        }

        /// <summary>
        /// The up vector formed from the second row M21, M22, M23 elements.
        /// </summary>
        public static Vector3 Up(in this Matrix4x4 value)
        {
            return new Vector3(value.M21, value.M22, value.M23);
        }

        public static void ToMatrix4x3(in this Matrix4x4 value, out Matrix4x3 output)
        {
            output = new Matrix4x3(
                value.M11,
                value.M12,
                value.M13,

                value.M21,
                value.M22,
                value.M23,

                value.M31,
                value.M32,
                value.M33,

                value.M41,
                value.M42,
                value.M43);
        }
    }
}
