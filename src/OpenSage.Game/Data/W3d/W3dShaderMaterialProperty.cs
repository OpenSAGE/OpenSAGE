using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Graphics.Shaders;

namespace OpenSage.Data.W3d
{
    public sealed class W3dShaderMaterialProperty : W3dChunk
    {
        public W3dShaderMaterialPropertyType PropertyType { get; private set; }
        public string PropertyName { get; private set; }

        public string StringValue { get; private set; }
        public W3dShaderMaterialPropertyValue Value { get; private set; }

        internal static W3dShaderMaterialProperty Parse(BinaryReader reader, uint chunkSize)
        {
            var result = new W3dShaderMaterialProperty
            {
                PropertyType = reader.ReadUInt32AsEnum<W3dShaderMaterialPropertyType>(),
                PropertyName = reader.ReadFixedLengthString((int) reader.ReadUInt32())
            };

            var value = new W3dShaderMaterialPropertyValue();

            switch (result.PropertyType)
            {
                case W3dShaderMaterialPropertyType.Texture:
                    result.StringValue = reader.ReadFixedLengthString((int) reader.ReadUInt32());
                    break;

                case W3dShaderMaterialPropertyType.Float:
                case W3dShaderMaterialPropertyType.Vector2:
                case W3dShaderMaterialPropertyType.Vector3:
                case W3dShaderMaterialPropertyType.Vector4:
                case W3dShaderMaterialPropertyType.Int:
                case W3dShaderMaterialPropertyType.Bool:
                    value = W3dShaderMaterialPropertyValue.Parse(reader, result.PropertyType);
                    break;

                default:
                    throw new InvalidDataException();
            }

            result.Value = value;

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write((uint) PropertyType);
            writer.Write((uint) PropertyName.Length + 1);
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
}
