using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dHLod(W3dHLodHeader Header,
    List<W3dHLodArray> Lods,
    W3dHLodArray? Aggregate,
    W3dHLodArray? Proxy) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_HLOD)
{
    internal static W3dHLod Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dHLodHeader? resultHeader = null;
            List<W3dHLodArray> lods = [];
            W3dHLodArray? aggregate = null;
            W3dHLodArray? proxy = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_HLOD_HEADER:
                        resultHeader = W3dHLodHeader.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_HLOD_LOD_ARRAY:
                        lods.Add(W3dHLodArray.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_HLOD_AGGREGATE_ARRAY:
                        aggregate = W3dHLodArray.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_HLOD_PROXY_ARRAY:
                        proxy = W3dHLodArray.Parse(reader, context);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null)
            {
                throw new InvalidDataException("header should never be null");
            }

            return new W3dHLod(resultHeader, lods, aggregate, proxy);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Header;

        foreach (var lod in Lods)
        {
            yield return lod;
        }

        if (Aggregate != null)
        {
            yield return Aggregate;
        }
    }
}
