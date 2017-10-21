using System.IO;
using OpenSage.Data.Utilities.Extensions;
using LLGfx;
using OpenSage.Mathematics;

namespace OpenSage.Data.W3d
{
    public sealed class W3dShaderMaterialProperty : W3dChunk
    {
        public W3dShaderMaterialPropertyType PropertyType { get; private set; }
        public string TypeName { get; private set; }

        public string ItemName { get; private set; }
        public float? ItemScalar1 { get; private set; }
        public float? ItemScalar2 { get; private set; }
        public uint? ItemUint { get; private set; }
        public ColorRgbF? ItemColorRgb { get; private set; }
        public ColorRgbaF? ItemColorRgba { get; private set; }
        public byte? ItemAlpha { get; private set; }

        public static W3dShaderMaterialProperty Parse(BinaryReader reader, uint chunkSize)
        {
            var result = new W3dShaderMaterialProperty
            {
                PropertyType = reader.ReadUInt32AsEnum<W3dShaderMaterialPropertyType>(),
                TypeName = reader.ReadFixedLengthString((int) reader.ReadUInt32())
            };

            switch (result.PropertyType)
            {
                case W3dShaderMaterialPropertyType.Texture:
                    result.ItemName = reader.ReadFixedLengthString((int) reader.ReadUInt32());
                    break;

                case W3dShaderMaterialPropertyType.Bump:
                    result.ItemScalar1 = reader.ReadSingle();
                    break;

                case W3dShaderMaterialPropertyType.Unknown1:
                    result.ItemScalar1 = reader.ReadSingle();
                    result.ItemScalar2 = reader.ReadSingle();
                    break;

                case W3dShaderMaterialPropertyType.Unknown2:
                    result.ItemColorRgb = reader.ReadColorRgbF();
                    break;

                case W3dShaderMaterialPropertyType.Colors:
                    result.ItemColorRgba = reader.ReadColorRgbaF();
                    break;

                case W3dShaderMaterialPropertyType.Unknown3:
                    result.ItemUint = reader.ReadUInt32();
                    break;

                case W3dShaderMaterialPropertyType.Alpha:
                    result.ItemAlpha = reader.ReadByte();
                    break;

                default:
                    throw new InvalidDataException();
            }

            return result;
        }
    }
}
