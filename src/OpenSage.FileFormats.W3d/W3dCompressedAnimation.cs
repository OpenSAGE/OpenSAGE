using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dCompressedAnimation(
    W3dCompressedAnimationHeader Header,
    List<W3dTimeCodedAnimationChannel> TimeCodedChannels,
    List<W3dAdaptiveDeltaAnimationChannel> AdaptiveDeltaChannels,
    List<W3dTimeCodedBitChannel> TimeCodedBitChannels,
    List<W3dMotionChannel> MotionChannels) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION)
{
    internal static W3dCompressedAnimation Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dCompressedAnimationHeader? resultHeader = null;
            List<W3dTimeCodedAnimationChannel> timeCodedChannels = [];
            List<W3dAdaptiveDeltaAnimationChannel> adaptiveDeltaChannels = [];
            List<W3dTimeCodedBitChannel> timeCodedBitChannels = [];
            List<W3dMotionChannel> motionChannels = [];

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_HEADER:
                        resultHeader = W3dCompressedAnimationHeader.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_CHANNEL:
                        if (resultHeader is null)
                        {
                            throw new InvalidDataException();
                        }

                        switch (resultHeader.Flavor)
                        {
                            case W3dCompressedAnimationFlavor.TimeCoded:
                                timeCodedChannels.Add(W3dTimeCodedAnimationChannel.Parse(reader, context));
                                break;

                            case W3dCompressedAnimationFlavor.AdaptiveDelta4:
                                adaptiveDeltaChannels.Add(W3dAdaptiveDeltaAnimationChannel.Parse(reader, context, W3dAdaptiveDeltaBitCount.FourBits));
                                break;

                            default:
                                throw new InvalidDataException();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_COMPRESSED_BIT_CHANNEL:
                        if (resultHeader is null)
                        {
                            throw new InvalidDataException();
                        }

                        switch (resultHeader.Flavor)
                        {
                            case W3dCompressedAnimationFlavor.TimeCoded:
                                timeCodedBitChannels.Add(W3dTimeCodedBitChannel.Parse(reader, context));
                                break;

                            default:
                                throw new InvalidDataException();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_MOTION_CHANNEL:
                        if (resultHeader is null)
                        {
                            throw new InvalidDataException();
                        }

                        switch (resultHeader.Flavor)
                        {
                            case W3dCompressedAnimationFlavor.TimeCoded:
                                motionChannels.Add(W3dMotionChannel.Parse(reader, context));
                                break;

                            default:
                                throw new InvalidDataException();
                        }
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null)
            {
                throw new InvalidDataException("header should never be null");
            }

            return new W3dCompressedAnimation(resultHeader, timeCodedChannels, adaptiveDeltaChannels,
                timeCodedBitChannels, motionChannels);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Header;

        foreach (var channel in TimeCodedChannels)
        {
            yield return channel;
        }

        foreach (var channel in TimeCodedBitChannels)
        {
            yield return channel;
        }

        foreach (var channel in AdaptiveDeltaChannels)
        {
            yield return channel;
        }

        foreach (var channel in MotionChannels)
        {
            yield return channel;
        }
    }
}
