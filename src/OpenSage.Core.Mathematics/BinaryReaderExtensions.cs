using System;
using System.IO;
using OpenSage.FileFormats;

namespace OpenSage.Mathematics;

public static class BinaryReaderExtensions
{
    public static RectangleF ReadRectangleF(this BinaryReader reader)
    {
        return new RectangleF(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle());
    }

    public static Rectangle ReadRectangle(this BinaryReader reader)
    {
        return new Rectangle(
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32());
    }

    public static Point2D ReadPoint2D(this BinaryReader reader)
    {
        return new Point2D(
            reader.ReadInt32(),
            reader.ReadInt32());
    }

    public static Point3D ReadPoint3D(this BinaryReader reader)
    {
        return new Point3D(
            reader.ReadInt32(),
            reader.ReadInt32(),
            reader.ReadInt32());
    }

    public static IndexedTriangle ReadIndexedTri(this BinaryReader reader)
    {
        return new IndexedTriangle(
            reader.ReadUInt16(),
            reader.ReadUInt16(),
            reader.ReadUInt16());
    }

    public static Matrix4x3 ReadMatrix4x3(this BinaryReader reader)
    {
        return new Matrix4x3(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle());
    }

    public static Matrix4x3 ReadMatrix4x3Transposed(this BinaryReader reader)
    {
        var m11 = reader.ReadSingle();
        var m21 = reader.ReadSingle();
        var m31 = reader.ReadSingle();
        var m41 = reader.ReadSingle();

        var m12 = reader.ReadSingle();
        var m22 = reader.ReadSingle();
        var m32 = reader.ReadSingle();
        var m42 = reader.ReadSingle();

        var m13 = reader.ReadSingle();
        var m23 = reader.ReadSingle();
        var m33 = reader.ReadSingle();
        var m43 = reader.ReadSingle();

        return new Matrix4x3(
            m11, m12, m13,
            m21, m22, m23,
            m31, m32, m33,
            m41, m42, m43);
    }

    public static ColorRgbaF ReadColorRgbaF(this BinaryReader reader)
    {
        return new ColorRgbaF(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle());
    }

    public static ColorRgbF ReadColorRgbF(this BinaryReader reader)
    {
        return new ColorRgbF(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle());
    }

    public static ColorRgb ReadColorRgb(this BinaryReader reader, bool extraBytePadding = false)
    {
        var result = new ColorRgb(
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte());

        if (extraBytePadding)
        {
            reader.ReadByte();
        }

        return result;
    }

    public static ColorRgba ReadColorRgba(this BinaryReader reader, ColorRgbaPixelOrder pixelOrder = ColorRgbaPixelOrder.Rgba)
    {
        byte r, g, b, a;

        switch (pixelOrder)
        {
            case ColorRgbaPixelOrder.Rgba:
                r = reader.ReadByte();
                g = reader.ReadByte();
                b = reader.ReadByte();
                a = reader.ReadByte();
                break;

            case ColorRgbaPixelOrder.Bgra:
                b = reader.ReadByte();
                g = reader.ReadByte();
                r = reader.ReadByte();
                a = reader.ReadByte();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(pixelOrder));
        }

        return new ColorRgba(r, g, b, a);
    }

    public static ColorRgba ReadColorRgbaInt(this BinaryReader reader)
    {
        var r = reader.ReadUInt32();
        var g = reader.ReadUInt32();
        var b = reader.ReadUInt32();
        var a = reader.ReadUInt32();

        if (r > 255 || g > 255 || b > 255 || a > 255)
        {
            throw new InvalidOperationException();
        }

        return new ColorRgba((byte)r, (byte)g, (byte)b, (byte)a);
    }

    public static RandomVariable ReadRandomVariable(this BinaryReader reader)
    {
        var distributionType = reader.ReadUInt32AsEnum<DistributionType>();
        var low = reader.ReadSingle();
        var high = reader.ReadSingle();

        return new RandomVariable(
            low,
            high,
            distributionType);
    }

    public static Matrix2x2 ReadMatrix2x2(this BinaryReader reader)
    {
        return new Matrix2x2(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle());
    }

    public static Line2D ReadLine2D(this BinaryReader reader)
    {
        return new Line2D(
            reader.ReadVector2(),
            reader.ReadVector2());
    }

    public static Percentage ReadPercentage(this BinaryReader reader)
    {
        return new Percentage(reader.ReadSingle());
    }

    public static FloatRange ReadFloatRange(this BinaryReader reader)
    {
        return new FloatRange(reader.ReadSingle(), reader.ReadSingle());
    }

    public static IntRange ReadIntRange(this BinaryReader reader)
    {
        return new IntRange(reader.ReadInt32(), reader.ReadInt32());
    }
}
