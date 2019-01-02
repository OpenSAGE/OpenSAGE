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

        public W3dAggregateUnknown UnknownChunkData { get; private set; }

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

                        // TODO: Determine if this is a parsing error? Why is this neccessary?
                        case W3dChunkType.W3D_CHUNK_AGGREGATE_UNKNOWN:      // See enb w3d "npc14b.w3d"
                            result.UnknownChunkData = W3dAggregateUnknown.Parse(reader, context);
                            isUnknownChunk = true;
                            break;

                        default:
                            throw CreateUnknownChunkException(chunkType);
                    }
                });

                // TODO: figure out why this is neccessary (see enb w3d "npc14b.w3d"
                // Neccessary because parseChunk callback Position check expects 1 byte less.  (Hack only for Unknown Data Chunk)
                if (result.UnknownChunkData != null)
                {
                    reader.BaseStream.Position = context.CurrentEndPosition;
                }

                return result;
            });

            // TODO: figure out why this is neccessary (see enb w3d "npc14b.w3d"
            // Neccessary because the parseChunk callback expects Position to be at end of stream. (Hack only for Unknown Data Chunk)
            if (isUnknownChunk)
            {
                reader.BaseStream.Position = reader.BaseStream.Length;
            }

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

            if (UnknownChunkData != null)
            {
                yield return UnknownChunkData;
            }
        }
    }
}
