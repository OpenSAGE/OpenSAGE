using System.IO;
using System.Collections.Generic;

namespace OpenSage.Data.W3d
{
    public sealed class W3dShaderMaterials : W3dChunk
    {
        public IReadOnlyList<W3dShaderMaterial> Materials { get; private set; }

        public static W3dShaderMaterials Parse(BinaryReader reader, uint chunkSize)
        {
            var normalMaps = new List<W3dShaderMaterial>();

            var finalResult = ParseChunk<W3dShaderMaterials>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_SHADER_MATERIAL:
                        normalMaps.Add(W3dShaderMaterial.Parse(reader, header.ChunkSize));
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });

            finalResult.Materials = normalMaps;

            return finalResult;
        }
    }
}
