using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.W3d
{
    public sealed class W3dMaterial
    {
        public string Name { get; private set; }

        public W3dVertexMaterial VertexMaterialInfo { get; private set; }

        public string MapperArgs0 { get; private set; }
        public string MapperArgs1 { get; private set; }

        public float UPerSec { get; private set; }
        public float VPerSec { get; private set; }

        public float CurrentU { get; private set; }
        public float CurrentV { get; private set; }

        public static W3dMaterial Parse(BinaryReader reader, uint chunkSize)
        {
            var result = new W3dMaterial();

            uint loadedSize = 0;

            do
            {
                loadedSize += W3dChunkHeader.SizeInBytes;
                var currentChunk = W3dChunkHeader.Parse(reader);

                loadedSize += currentChunk.ChunkSize;

                switch (currentChunk.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL_NAME:
                        result.Name = reader.ReadFixedLengthString((int) currentChunk.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_MAPPER_ARGS0:
                        result.MapperArgs0 = reader.ReadFixedLengthString((int) currentChunk.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_MAPPER_ARGS1:
                        result.MapperArgs1 = reader.ReadFixedLengthString((int) currentChunk.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL_INFO:
                        result.VertexMaterialInfo = W3dVertexMaterial.Parse(reader);
                        break;

                    default:
                        reader.ReadBytes((int) currentChunk.ChunkSize);
                        break;
                }
            } while (loadedSize < chunkSize);

            return result;
        }
    }
}
