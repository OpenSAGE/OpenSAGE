using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMaterial : W3dChunk
    {
        public string Name { get; private set; }

        public W3dVertexMaterial VertexMaterialInfo { get; private set; }

        public string MapperArgs0 { get; private set; }
        public string MapperArgs1 { get; private set; }

        public static W3dMaterial Parse(BinaryReader reader, uint chunkSize)
        {
            return ParseChunk<W3dMaterial>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL_NAME:
                        result.Name = reader.ReadFixedLengthString((int) header.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_MAPPER_ARGS0:
                        result.MapperArgs0 = reader.ReadFixedLengthString((int) header.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_MAPPER_ARGS1:
                        result.MapperArgs1 = reader.ReadFixedLengthString((int) header.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL_INFO:
                        result.VertexMaterialInfo = W3dVertexMaterial.Parse(reader);
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });
        }
    }
}
