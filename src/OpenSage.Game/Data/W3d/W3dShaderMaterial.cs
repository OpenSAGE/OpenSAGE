using System.IO;
using System.Collections.Generic;

namespace OpenSage.Data.W3d
{
    public sealed class W3dShaderMaterial : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_SHADER_MATERIAL;

        public W3dShaderMaterialHeader Header { get; private set; }

        public List<W3dShaderMaterialProperty> Properties { get; } = new List<W3dShaderMaterialProperty>();

        internal static W3dShaderMaterial Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dShaderMaterial();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_SHADER_MATERIAL_HEADER:
                            result.Header = W3dShaderMaterialHeader.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_SHADER_MATERIAL_PROPERTY:
                            result.Properties.Add(W3dShaderMaterialProperty.Parse(reader, context));
                            break;

                        default:
                            throw CreateUnknownChunkException(chunkType);
                    }
                });

                return result;
            });
        }

        protected override IEnumerable<W3dChunk> GetSubChunksOverride()
        {
            yield return Header;

            foreach (var property in Properties)
            {
                yield return property;
            }
        }
    }
}
