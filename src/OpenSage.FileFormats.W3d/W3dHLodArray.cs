using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public abstract class W3dHLodArrayBase<TDerived> : W3dContainerChunk
        where TDerived : W3dHLodArrayBase<TDerived>, new()
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_HLOD_LOD_ARRAY;

        public W3dHLodArrayHeader Header { get; private set; }

        public List<W3dHLodSubObject> SubObjects { get; } = new List<W3dHLodSubObject>();

        internal static TDerived Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new TDerived();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_HLOD_SUB_OBJECT_ARRAY_HEADER:
                            result.Header = W3dHLodArrayHeader.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_HLOD_SUB_OBJECT:
                            result.SubObjects.Add(W3dHLodSubObject.Parse(reader, context));
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

            foreach (var subObject in SubObjects)
            {
                yield return subObject;
            }
        }
    }

    public sealed class W3dHLodArray : W3dHLodArrayBase<W3dHLodArray>
    {
        
    }
}
