using System.IO;
using System.Collections.Generic;

namespace OpenSage.Data.W3d
{
    public sealed class W3dShaderMaterial : W3dChunk
    {
        public W3dShaderMaterialHeader Header { get; private set; }

        public IReadOnlyList<W3dShaderMaterialProperty> Properties { get; private set; }

        public static W3dShaderMaterial Parse(BinaryReader reader, uint chunkSize)
        {
            var properties = new List<W3dShaderMaterialProperty>();

            var finalResult = ParseChunk<W3dShaderMaterial>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_SHADER_MATERIAL_HEADER:
                        result.Header = W3dShaderMaterialHeader.Parse(reader, header.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_SHADER_MATERIAL_PROPERTY:
                        properties.Add(W3dShaderMaterialProperty.Parse(reader, header.ChunkSize));
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });

            finalResult.Properties = properties;

            return finalResult;
        }
    }
}
