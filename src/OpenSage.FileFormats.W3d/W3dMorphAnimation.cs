using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dMorphAnimation(
    W3dMorphAnimHeader Header,
    W3dMorphAnimChannel? AnimChannel,
    W3dMorphAnimPivotChannelData? PivotChannelData) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_MORPH_ANIMATION)
{
    internal static W3dMorphAnimation Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dMorphAnimHeader? resultHeader = null;
            W3dMorphAnimChannel? animChannel = null;
            W3dMorphAnimPivotChannelData? pivotChannelData = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_MORPHANIM_HEADER:
                        resultHeader = W3dMorphAnimHeader.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_MORPHANIM_CHANNEL:
                        animChannel = W3dMorphAnimChannel.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_MORPHANIM_PIVOTCHANNELDATA:
                        pivotChannelData = W3dMorphAnimPivotChannelData.Parse(reader, context);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null)
            {
                throw new InvalidDataException("header should never be null");
            }

            return new W3dMorphAnimation(resultHeader, animChannel, pivotChannelData);
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
