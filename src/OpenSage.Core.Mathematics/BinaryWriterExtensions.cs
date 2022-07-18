using System;
using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Mathematics;

public static class BinaryWriterExtensions
{


    public static void Write(this BinaryWriter writer, in Point2D value)
    {
        writer.Write(value.X);
        writer.Write(value.Y);
    }

    public static void Write(this BinaryWriter writer, in Point3D value)
    {
        writer.Write(value.X);
        writer.Write(value.Y);
        writer.Write(value.Z);
    }

    public static void Write(this BinaryWriter writer, in RectangleF value)
    {
        writer.Write(value.X);
        writer.Write(value.Y);
        writer.Write(value.Width);
        writer.Write(value.Height);
    }

    public static void Write(this BinaryWriter writer, in Matrix2x2 value)
    {
        writer.Write(value.M11);
        writer.Write(value.M12);
        writer.Write(value.M21);
        writer.Write(value.M22);
    }

    public static void Write(this BinaryWriter writer, in Matrix4x3 value)
    {
        writer.Write(value.M11);
        writer.Write(value.M12);
        writer.Write(value.M13);
        writer.Write(value.M21);
        writer.Write(value.M22);
        writer.Write(value.M23);
        writer.Write(value.M31);
        writer.Write(value.M32);
        writer.Write(value.M33);
        writer.Write(value.M41);
        writer.Write(value.M42);
        writer.Write(value.M43);
    }

    public static void WriteMatrix4x3Transposed(this BinaryWriter writer, in Matrix4x3 value)
    {
        writer.Write(value.M11);
        writer.Write(value.M21);
        writer.Write(value.M31);
        writer.Write(value.M41);
        writer.Write(value.M12);
        writer.Write(value.M22);
        writer.Write(value.M32);
        writer.Write(value.M42);
        writer.Write(value.M13);
        writer.Write(value.M23);
        writer.Write(value.M33);
        writer.Write(value.M43);
    }

    public static void Write(this BinaryWriter writer, in ColorRgb value, bool extraBytePadding = false)
    {
        writer.Write(value.R);
        writer.Write(value.G);
        writer.Write(value.B);

        if (extraBytePadding)
        {
            writer.Write((byte)0);
        }
    }

    public static void Write(this BinaryWriter writer, in ColorRgba value, ColorRgbaPixelOrder pixelOrder = ColorRgbaPixelOrder.Rgba)
    {
        switch (pixelOrder)
        {
            case ColorRgbaPixelOrder.Rgba:
                writer.Write(value.R);
                writer.Write(value.G);
                writer.Write(value.B);
                writer.Write(value.A);
                break;

            case ColorRgbaPixelOrder.Bgra:
                writer.Write(value.B);
                writer.Write(value.G);
                writer.Write(value.R);
                writer.Write(value.A);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(pixelOrder));
        }
    }

    public static void WriteColorRgbaInt(this BinaryWriter writer, in ColorRgba value)
    {
        writer.Write((uint)value.R);
        writer.Write((uint)value.G);
        writer.Write((uint)value.B);
        writer.Write((uint)value.A);
    }

    public static void Write(this BinaryWriter writer, in ColorRgbF value)
    {
        writer.Write(value.R);
        writer.Write(value.G);
        writer.Write(value.B);
    }

    public static void Write(this BinaryWriter writer, in ColorRgbaF value)
    {
        writer.Write(value.R);
        writer.Write(value.G);
        writer.Write(value.B);
        writer.Write(value.A);
    }

    public static void Write(this BinaryWriter writer, in RandomVariable value)
    {
        writer.WriteEnumAsUInt32(value.DistributionType);
        writer.Write(value.Low);
        writer.Write(value.High);
    }

    public static void Write(this BinaryWriter writer, in Line2D line)
    {
        writer.Write(line.V0);
        writer.Write(line.V1);
    }
}
