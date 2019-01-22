using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHLod : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_HLOD;

        public W3dHLodHeader Header { get; private set; }

        public List<W3dHLodArray> Lods { get; } = new List<W3dHLodArray>();

        public W3dHLodAggregateArray Aggregate { get; private set; }

        public W3dHLodProxyArray Proxy { get; private set; }

        internal static W3dHLod Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dHLod();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_HLOD_HEADER:
                            result.Header = W3dHLodHeader.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_HLOD_LOD_ARRAY:
                            result.Lods.Add(W3dHLodArray.Parse(reader, context));
                            break;

                        case W3dChunkType.W3D_CHUNK_HLOD_AGGREGATE_ARRAY:
                            result.Aggregate = W3dHLodAggregateArray.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_HLOD_PROXY_ARRAY:
                            result.Proxy = W3dHLodProxyArray.Parse(reader, context);
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

    public sealed class W3dHLodAggregateArray : W3dHLodArrayBase<W3dHLodAggregateArray>
    {
        
    }

    public sealed class W3dHLodProxyArray : W3dHLodArrayBase<W3dHLodProxyArray>
    {

    }
}
