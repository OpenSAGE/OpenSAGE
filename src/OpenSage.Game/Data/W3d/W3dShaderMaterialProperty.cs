using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dShaderMaterialProperty : W3dChunk
    {
        public W3dShaderMaterialPropertyType PropertyType { get; private set; }
        public string PropertyName { get; private set; }

        public string StringValue { get; private set; }
        public W3dShaderMaterialPropertyValue Value { get; private set; }

        public static W3dShaderMaterialProperty Parse(BinaryReader reader, uint chunkSize)
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
                    value.Float = reader.ReadSingle();
                    break;

                case W3dShaderMaterialPropertyType.Vector2:
                    value.Vector2 = reader.ReadVector2();
                    break;

                case W3dShaderMaterialPropertyType.Vector3:
                    value.Vector3 = reader.ReadVector3();
                    break;

                case W3dShaderMaterialPropertyType.Color:
                    value.Color = reader.ReadColorRgbaF();
                    break;

                case W3dShaderMaterialPropertyType.Int:
                    value.Int = reader.ReadInt32();
                    break;

                case W3dShaderMaterialPropertyType.Bool:
                    value.Bool = reader.ReadBooleanChecked();
                    break;

                default:
                    throw new InvalidDataException();
            }

            result.Value = value;

            return result;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct W3dShaderMaterialPropertyValue
    {
        [FieldOffset(0)]
        public bool Bool;

        [FieldOffset(0)]
        public float Float;

        [FieldOffset(0)]
        public Vector2 Vector2;

        [FieldOffset(0)]
        public Vector3 Vector3;

        [FieldOffset(0)]
        public int Int;

        [FieldOffset(0)]
        public ColorRgbaF Color;
    }
}
