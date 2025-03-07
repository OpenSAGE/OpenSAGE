using System;
using System.Numerics;

namespace OpenSage.Mathematics;

public static class Matrix4x4Utility
{
    public static Matrix4x4 Invert(in Matrix4x4 value)
    {
        if (!Matrix4x4.Invert(value, out var result))
        {
            throw new System.InvalidOperationException();
        }
        return result;
    }

    public static float GetXTranslation(in this Matrix4x4 value) => value.M41;

    public static float GetYTranslation(in this Matrix4x4 value) => value.M42;

    public static float GetZTranslation(in this Matrix4x4 value) => value.M43;

    public static float GetZRotation(in this Matrix4x4 matrix) => MathF.Atan2(matrix.M12, matrix.M11);

    public static void SetXTranslation(ref this Matrix4x4 matrix, float value) => matrix.M41 = value;

    public static void SetYTranslation(ref this Matrix4x4 matrix, float value) => matrix.M42 = value;

    public static void SetZTranslation(ref this Matrix4x4 matrix, float value) => matrix.M43 = value;

    public static Vector3 GetXVector(in this Matrix4x4 value)
    {
        return new Vector3(value.M11, value.M12, value.M13);
    }

    public static Vector3 GetYVector(in this Matrix4x4 value)
    {
        return new Vector3(value.M21, value.M22, value.M32);
    }

    public static Vector3 GetZVector(in this Matrix4x4 value)
    {
        return new Vector3(value.M31, value.M32, value.M33);
    }

    /// <summary>
    /// The backward vector formed from the third row M31, M32, M33 elements.
    /// </summary>
    public static Vector3 Backward(in this Matrix4x4 value)
    {
        return new Vector3(value.M31, value.M32, value.M33);
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

    public static void RotateX(ref this Matrix4x4 value, float theta)
    {
        float tmp1, tmp2;

        var s = MathF.Sin(theta);
        var c = MathF.Cos(theta);

        tmp1 = value.M21;
        tmp2 = value.M31;

        value.M21 = c * tmp1 + s * tmp2;
        value.M31 = -s * tmp1 + c * tmp2;

        tmp1 = value.M22;
        tmp2 = value.M32;

        value.M22 = c * tmp1 + s * tmp2;
        value.M32 = -s * tmp1 + c * tmp2;

        tmp1 = value.M23;
        tmp2 = value.M33;

        value.M23 = c * tmp1 + s * tmp2;
        value.M33 = -s * tmp1 + c * tmp2;
    }

    public static void RotateY(ref this Matrix4x4 value, float theta)
    {
        float tmp1, tmp2;

        var s = MathF.Sin(theta);
        var c = MathF.Cos(theta);

        tmp1 = value.M11;
        tmp2 = value.M31;

        value.M11 = c * tmp1 - s * tmp2;
        value.M31 = s * tmp1 + c * tmp2;

        tmp1 = value.M12;
        tmp2 = value.M32;

        value.M12 = c * tmp1 - s * tmp2;
        value.M32 = s * tmp1 + c * tmp2;

        tmp1 = value.M13;
        tmp2 = value.M33;

        value.M13 = c * tmp1 - s * tmp2;
        value.M33 = s * tmp1 + c * tmp2;
    }

    public static void RotateZ(ref this Matrix4x4 value, float theta)
    {
        float tmp1, tmp2;

        var s = MathF.Sin(theta);
        var c = MathF.Cos(theta);

        tmp1 = value.M11;
        tmp2 = value.M21;

        value.M11 = c * tmp1 + s * tmp2;
        value.M21 = -s * tmp1 + c * tmp2;

        tmp1 = value.M12;
        tmp2 = value.M22;

        value.M12 = c * tmp1 + s * tmp2;
        value.M32 = -s * tmp1 + c * tmp2;

        tmp1 = value.M13;
        tmp2 = value.M23;

        value.M13 = c * tmp1 + s * tmp2;
        value.M23 = -s * tmp1 + c * tmp2;
    }
}
