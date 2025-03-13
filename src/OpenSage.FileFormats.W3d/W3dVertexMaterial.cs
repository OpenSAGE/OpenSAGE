using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dVertexMaterial(
    W3dVertexMaterialName Name,
    W3dVertexMaterialInfo Info,
    W3dVertexMapperArgs? MapperArgs0,
    W3dVertexMapperArgs? MapperArgs1) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL)
{
    internal static W3dVertexMaterial Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dVertexMaterialName? name = null;
            W3dVertexMaterialInfo? info = null;
            W3dVertexMapperArgs? mapperArgs0 = null;
            W3dVertexMapperArgs? mapperArgs1 = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL_NAME:
                        name = W3dVertexMaterialName.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_MAPPER_ARGS0:
                        mapperArgs0 = W3dVertexMapperArgs.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_MAPPER_ARGS1:
                        mapperArgs1 = W3dVertexMapperArgs.Parse(reader, context, chunkType);
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL_INFO:
                        info = W3dVertexMaterialInfo.Parse(reader, context);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (name is null || info is null)
            {
                throw new InvalidDataException();
            }

            return new W3dVertexMaterial(name, info, mapperArgs0, mapperArgs1);
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
