using System.IO;
using System.Numerics;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;
using OpenSage.FileFormats.W3d;
using OpenSage.Graphics;
using Veldrid;

namespace OpenSage.Data.W3x
{
    public sealed class FXShaderConstant
    {
        public FXShaderConstantType Type { get; private set; }
        public string Name { get; private set; }

        public Texture TextureValue { get; private set; }
        public uint FloatValueCount { get; private set; }
        public W3dShaderMaterialPropertyValue Value { get; private set; }

        internal static FXShaderConstant Parse(BinaryReader reader, AssetImportCollection imports)
        {
            var result = new FXShaderConstant
            {
                Type = reader.ReadUInt32AsEnum<FXShaderConstantType>(),
                Name = reader.ReadUInt32PrefixedAsciiStringAtOffset()
            };

            var value = new W3dShaderMaterialPropertyValue();

            switch (result.Type)
            {
                case FXShaderConstantType.Float:
                    var values = reader.ReadArrayAtOffset(() => reader.ReadSingle());
                    result.FloatValueCount = (uint) values.Length;
                    switch (values.Length)
                    {
                        case 1:
                            value.Float = values[0];
                            break;

                        case 2:
                            value.Vector2 = new Vector2(values[0], values[1]);
                            break;

                        case 3:
                            value.Vector3 = new Vector3(values[0], values[1], values[2]);
                            break;

                        case 4:
                            value.Vector4 = new Vector4(values[0], values[1], values[2], values[3]);
                            break;

                        default:
                            throw new InvalidDataException();
                    }
                    break;

                case FXShaderConstantType.Bool:
                    value.Bool = reader.ReadBooleanChecked();
                    break;

                case FXShaderConstantType.Int:
                    value.Int = reader.ReadInt32();
                    break;

                case FXShaderConstantType.Texture:
                    result.TextureValue = imports.GetImportedData<TextureAsset>(reader);
                    break;

                default:
                    throw new InvalidDataException();
            }

            result.Value = value;

            return result;
        }
    }

    public enum FXShaderConstantType : uint
    {
        Float = 0xDF08DF25,
        Bool = 0xA3F84C3D,
        Int = 0x89181982,
        Texture = 0xA59096A6
    }
}
