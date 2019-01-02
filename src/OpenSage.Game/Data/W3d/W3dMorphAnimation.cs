using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMorphAnimation : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_MORPH_ANIMATION;

        public W3dMorphAnimHeader Header { get; private set; }

        public W3dMorphAnimChannel AnimChannel { get; private set; }

        public W3dMorphAnimPivotChannelData PivotChannelData { get; private set; }

        internal static W3dMorphAnimation Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dMorphAnimation();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_MORPHANIM_HEADER:
                            result.Header = W3dMorphAnimHeader.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_MORPHANIM_CHANNEL:
                            result.AnimChannel = W3dMorphAnimChannel.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_MORPHANIM_PIVOTCHANNELDATA:
                            result.PivotChannelData = W3dMorphAnimPivotChannelData.Parse(reader, context);
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

            if (AnimChannel != null)
            {
                yield return AnimChannel;
            }

            if (PivotChannelData != null)
            {
                yield return PivotChannelData;
            }
        }
    }
}
