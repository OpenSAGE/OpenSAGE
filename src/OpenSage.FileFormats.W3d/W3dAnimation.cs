using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dAnimation(W3dAnimationHeader Header, IReadOnlyList<W3dAnimationChannelBase> Channels)
    : W3dContainerChunk(W3dChunkType.W3D_CHUNK_ANIMATION)
{
    internal static W3dAnimation Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dAnimationHeader? resultHeader = null;
            List<W3dAnimationChannelBase> channels = [];

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_ANIMATION_HEADER:
                        resultHeader = W3dAnimationHeader.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_ANIMATION_CHANNEL:
                        channels.Add(W3dAnimationChannel.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_BIT_CHANNEL:
                        channels.Add(W3dBitChannel.Parse(reader, context));
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null)
            {
                throw new InvalidDataException("header should never be null");
            }

            return new W3dAnimation(resultHeader, channels);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Header;

        foreach (var channel in Channels)
        {
            yield return channel;
        }
    }
}
