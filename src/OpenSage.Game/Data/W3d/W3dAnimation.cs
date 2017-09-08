using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dAnimation : W3dChunk
    {
        public W3dAnimationHeader Header { get; private set; }

        public IReadOnlyList<W3dAnimationChannel> Channels { get; private set; }

        public IReadOnlyList<W3dBitChannel> BitChannels { get; private set; }

        public static W3dAnimation Parse(BinaryReader reader, uint chunkSize)
        {
            var channels = new List<W3dAnimationChannel>();
            var bitChannels = new List<W3dBitChannel>();

            var finalResult = ParseChunk<W3dAnimation>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_ANIMATION_HEADER:
                        result.Header = W3dAnimationHeader.Parse(reader);
                        break;

                    case W3dChunkType.W3D_CHUNK_ANIMATION_CHANNEL:
                        channels.Add(W3dAnimationChannel.Parse(reader, header.ChunkSize));
                        break;

                    case W3dChunkType.W3D_CHUNK_BIT_CHANNEL:
                        bitChannels.Add(W3dBitChannel.Parse(reader, header.ChunkSize));
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });

            finalResult.Channels = channels;
            finalResult.BitChannels = bitChannels;

            return finalResult;
        }
    }
}
