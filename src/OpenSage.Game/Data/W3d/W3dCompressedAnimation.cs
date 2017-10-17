using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dCompressedAnimation : W3dChunk
    {
        public W3dCompressedAnimationHeader Header { get; private set; }

        public IReadOnlyList<W3dTimeCodedAnimationChannel> TimeCodedChannels { get; private set; }

        public IReadOnlyList<W3dTimeCodedBitChannel> TimeCodedBitChannels { get; private set; }

        public static W3dCompressedAnimation Parse(BinaryReader reader, uint chunkSize)
        {
            var timeCodedChannels = new List<W3dTimeCodedAnimationChannel>();
            var timeCodedBitChannels = new List<W3dTimeCodedBitChannel>();

            var finalResult = ParseChunk<W3dCompressedAnimation>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_HEADER:
                        result.Header = W3dCompressedAnimationHeader.Parse(reader);
                        break;

                    case W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_CHANNEL:
                        switch (result.Header.Flavor)
                        {
                            case W3dCompressedAnimationFlavor.TimeCoded:
                                timeCodedChannels.Add(W3dTimeCodedAnimationChannel.Parse(reader, header.ChunkSize));
                                break;

                            default:
                                throw new InvalidDataException();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_COMPRESSED_BIT_CHANNEL:
                        switch (result.Header.Flavor)
                        {
                            case W3dCompressedAnimationFlavor.TimeCoded:
                                timeCodedBitChannels.Add(W3dTimeCodedBitChannel.Parse(reader, header.ChunkSize));
                                break;

                            default:
                                throw new InvalidDataException();
                        }
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });

            finalResult.TimeCodedChannels = timeCodedChannels;
            finalResult.TimeCodedBitChannels = timeCodedBitChannels;

            return finalResult;
        }
    }
}
