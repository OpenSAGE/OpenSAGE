using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    /// <summary>
    /// An 'aggregate' is simply a 'shell' that contains references to a hierarchy model and subobjects to attach to its bones.
    /// </summary>
    public sealed class W3dAggregate : W3dContainerChunk
    {

        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_AGGREGATE;

        public W3dAggregateHeader Header { get; private set; }

        public W3dAggregateInfo Info { get; private set; }

        public W3dAggregateClassInfo ClassInfo { get; private set; }

        public W3dTextureReplacerInfo TextureReplacerInfo { get; private set; }

        internal static W3dAggregate Parse(BinaryReader reader, W3dParseContext context)
        {
            var isUnknownChunk = false;
            var parsedChunk = ParseChunk(reader, context, header =>
            {
                var result = new W3dAggregate();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_AGGREGATE_CLASS_INFO:
                            result.ClassInfo = W3dAggregateClassInfo.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_AGGREGATE_HEADER:
                            result.Header = W3dAggregateHeader.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_AGGREGATE_INFO:
                            result.Info = W3dAggregateInfo.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_TEXTURE_REPLACER_INFO:
                            result.TextureReplacerInfo = W3dTextureReplacerInfo.Parse(reader, context);
                            break;

                        default:
                            throw CreateUnknownChunkException(chunkType);
                    }
                });

                return result;
            });

            return parsedChunk;
        }

        protected override IEnumerable<W3dChunk> GetSubChunksOverride()
        {
            yield return Header;

            if (Info != null)
            {
                yield return Info;
            }

            if (ClassInfo != null)
            {
                yield return ClassInfo;
            }

            if (TextureReplacerInfo != null)
            {
                yield return TextureReplacerInfo;
            }
        }
    }
}
