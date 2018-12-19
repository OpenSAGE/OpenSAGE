using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dVertexMaterial : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL;

        public W3dVertexMaterialName Name { get; private set; }

        public W3dVertexMaterialInfo Info { get; private set; }

        public W3dVertexMapperArgs MapperArgs0 { get; private set; }
        public W3dVertexMapperArgs MapperArgs1 { get; private set; }

        internal static W3dVertexMaterial Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dVertexMaterial();
                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL_NAME:
                            result.Name = W3dVertexMaterialName.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_VERTEX_MAPPER_ARGS0:
                            result.MapperArgs0 = W3dVertexMapperArgs.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_VERTEX_MAPPER_ARGS1:
                            result.MapperArgs1 = W3dVertexMapperArgs.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL_INFO:
                            result.Info = W3dVertexMaterialInfo.Parse(reader, context);
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
            yield return Name;
            yield return Info;

            if (MapperArgs0 != null)
            {
                yield return MapperArgs0;
            }

            if (MapperArgs1 != null)
            {
                yield return MapperArgs1;
            }
        }
    }
}
