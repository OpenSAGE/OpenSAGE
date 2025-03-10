using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Graphics.Mathematics;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dShaderMaterialProperty(
    W3dShaderMaterialPropertyType PropertyType,
    string PropertyName,
    string StringValue,
    W3dShaderMaterialPropertyValue Value) : W3dChunk(W3dChunkType.W3D_CHUNK_SHADER_MATERIAL_PROPERTY)
{
    internal static W3dShaderMaterialProperty Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var propertyType = reader.ReadUInt32AsEnum<W3dShaderMaterialPropertyType>();
            var propertyName = reader.ReadFixedLengthString((int)reader.ReadUInt32());

            var value = new W3dShaderMaterialPropertyValue();
            var stringValue = string.Empty;

            switch (propertyType)
            {
                case W3dShaderMaterialPropertyType.Texture:
                    stringValue = reader.ReadFixedLengthString((int)reader.ReadUInt32());
                    break;

                case W3dShaderMaterialPropertyType.Float:
                case W3dShaderMaterialPropertyType.Vector2:
                case W3dShaderMaterialPropertyType.Vector3:
                case W3dShaderMaterialPropertyType.Vector4:
                case W3dShaderMaterialPropertyType.Int:
                case W3dShaderMaterialPropertyType.Bool:
                    value = W3dShaderMaterialPropertyValue.Parse(reader, propertyType);
                    break;

                default:
                    throw new InvalidDataException();
            }

            return new W3dShaderMaterialProperty(propertyType, propertyName, stringValue, value);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write((uint)PropertyType);
        writer.Write((uint)PropertyName.Length + 1);
        writer.WriteFixedLengthString(PropertyName, PropertyName.Length + 1);

        switch (PropertyType)
        {
            case W3dShaderMaterialPropertyType.Texture:
                writer.Write(StringValue.Length + 1);
                writer.WriteFixedLengthString(StringValue, StringValue.Length + 1);
                break;

            case W3dShaderMaterialPropertyType.Float:
            case W3dShaderMaterialPropertyType.Vector2:
            case W3dShaderMaterialPropertyType.Vector3:
            case W3dShaderMaterialPropertyType.Vector4:
            case W3dShaderMaterialPropertyType.Int:
            case W3dShaderMaterialPropertyType.Bool:
                Value.WriteTo(writer, PropertyType);
                break;

            default:
                throw new InvalidDataException();
        }
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct W3dShaderMaterialPropertyValue
{
    [FieldOffset(0)]
    public Bool32 Bool;

    [FieldOffset(0)]
    public float Float;

    [FieldOffset(0)]
    public Vector2 Vector2;

    [FieldOffset(0)]
    public Vector3 Vector3;

    [FieldOffset(0)]
    public Vector4 Vector4;

    [FieldOffset(0)]
    public int Int;

    internal static W3dShaderMaterialPropertyValue Parse(BinaryReader reader, W3dShaderMaterialPropertyType propertyType)
    {
        switch (propertyType)
        {
            case W3dShaderMaterialPropertyType.Float:
                return new W3dShaderMaterialPropertyValue
                {
                    Float = reader.ReadSingle()
                };

            case W3dShaderMaterialPropertyType.Vector2:
                return new W3dShaderMaterialPropertyValue
                {
                    Vector2 = reader.ReadVector2()
                };

            case W3dShaderMaterialPropertyType.Vector3:
                return new W3dShaderMaterialPropertyValue
                {
                    Vector3 = reader.ReadVector3()
                };

            case W3dShaderMaterialPropertyType.Vector4:
                return new W3dShaderMaterialPropertyValue
                {
                    Vector4 = reader.ReadVector4()
                };

            case W3dShaderMaterialPropertyType.Int:
                return new W3dShaderMaterialPropertyValue
                {
                    Int = reader.ReadInt32()
                };

            case W3dShaderMaterialPropertyType.Bool:
                return new W3dShaderMaterialPropertyValue
                {
                    Bool = reader.ReadBooleanChecked()
                };

            default:
                throw new InvalidDataException();
        }
    }

    internal void WriteTo(BinaryWriter writer, W3dShaderMaterialPropertyType propertyType)
    {
        switch (propertyType)
        {
            case W3dShaderMaterialPropertyType.Float:
                writer.Write(Float);
                break;

            case W3dShaderMaterialPropertyType.Vector2:
                writer.Write(Vector2);
                break;

            case W3dShaderMaterialPropertyType.Vector3:
                writer.Write(Vector3);
                break;

            case W3dShaderMaterialPropertyType.Vector4:
                writer.Write(Vector4);
                break;

            case W3dShaderMaterialPropertyType.Int:
                writer.Write(Int);
                break;

            case W3dShaderMaterialPropertyType.Bool:
                writer.Write(Bool);
                break;

            default:
                throw new InvalidDataException();
        }
    }
}
