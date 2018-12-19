using System.IO;
using OpenSage.Data.Utilities;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dVertexMaterialInfo : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_MATERIAL_INFO;

        public W3dVertexMaterialFlags Attributes { get; private set; }

        public W3dVertexMappingType Stage0Mapping { get; private set; }
        public W3dVertexMappingType Stage1Mapping { get; private set; }

        public W3dRgb Ambient { get; private set; }
        public W3dRgb Diffuse { get; private set; }
        public W3dRgb Specular { get; private set; }
        public W3dRgb Emissive { get; private set; }

        /// <summary>
        /// how tight the specular highlight will be, 1 - 1000 (default = 1)
        /// </summary>
        public float Shininess { get; private set; }

        /// <summary>
        /// how opaque the material is, 0.0 = invisible, 1.0 = fully opaque (default = 1)
        /// </summary>
        public float Opacity { get; private set; }

        /// <summary>
        /// how much light passes through the material. (default = 0)
        /// </summary>
        public float Translucency { get; private set; }

        internal static W3dVertexMaterialInfo Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var rawAttributes = reader.ReadUInt32();

                return new W3dVertexMaterialInfo
                {
                    Attributes = (W3dVertexMaterialFlags) (rawAttributes & 0xF),

                    Stage0Mapping = ConvertStageMapping(rawAttributes, 0x00FF0000, 16),
                    Stage1Mapping = ConvertStageMapping(rawAttributes, 0x0000FF00, 8),

                    Ambient = W3dRgb.Parse(reader),
                    Diffuse = W3dRgb.Parse(reader),
                    Specular = W3dRgb.Parse(reader),
                    Emissive = W3dRgb.Parse(reader),

                    Shininess = reader.ReadSingle(),
                    Opacity = reader.ReadSingle(),
                    Translucency = reader.ReadSingle()
                };
            });
        }

        private static W3dVertexMappingType ConvertStageMapping(uint attributes, uint mask, int shift)
        {
            return EnumUtility.CastValueAsEnum<uint, W3dVertexMappingType>((attributes & mask) >> shift);
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            var rawAttributes = (uint) Attributes;
            rawAttributes |= (uint) Stage0Mapping << 16;
            rawAttributes |= (uint) Stage1Mapping << 8;
            writer.Write(rawAttributes);

            writer.Write(Ambient);
            writer.Write(Diffuse);
            writer.Write(Specular);
            writer.Write(Emissive);

            writer.Write(Shininess);
            writer.Write(Opacity);
            writer.Write(Translucency);
        }
    }
}
