using System.IO;
using OpenSage.Data.Utilities.Extensions;
using LLGfx;

namespace OpenSage.Data.W3d
{
    public sealed class W3dShaderMaterialProperty : W3dChunk
    {
        public W3dNormalMapPropertyType PropertyType { get; private set; }
        public string TypeName { get; private set; }

        public string ItemName { get; private set; }
        public float? ItemScalar1 { get; private set; }
        public float? ItemScalar2 { get; private set; }
        public ColorRgbaF? ItemColor { get; private set; }
        public byte? ItemAlpha { get; private set; }

        public static W3dShaderMaterialProperty Parse(BinaryReader reader, uint chunkSize)
        {
            var result = new W3dShaderMaterialProperty
            {
                PropertyType = reader.ReadUInt32AsEnum<W3dNormalMapPropertyType>(),
                TypeName = reader.ReadFixedLengthString((int) reader.ReadUInt32())
            };

            switch (result.PropertyType)
            {
                case W3dNormalMapPropertyType.Texture:
                    result.ItemName = reader.ReadFixedLengthString((int) reader.ReadUInt32());
                    break;

                case W3dNormalMapPropertyType.Bump:
                    result.ItemScalar1 = reader.ReadSingle();
                    break;

                case W3dNormalMapPropertyType.Unknown:
                    result.ItemScalar1 = reader.ReadSingle();
                    result.ItemScalar2 = reader.ReadSingle();
                    break;

                case W3dNormalMapPropertyType.Colors:
                    result.ItemColor = reader.ReadColorRgbaF();
                    break;

                case W3dNormalMapPropertyType.Alpha:
                    result.ItemAlpha = reader.ReadByte();
                    break;

                default:
                    throw new InvalidDataException();
            }

            return result;
        }
    }
}
