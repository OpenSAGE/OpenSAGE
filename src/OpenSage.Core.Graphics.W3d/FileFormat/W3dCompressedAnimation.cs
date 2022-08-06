using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dCompressedAnimation : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION;

        public W3dCompressedAnimationHeader Header { get; private set; }

        public List<W3dTimeCodedAnimationChannel> TimeCodedChannels { get; } = new List<W3dTimeCodedAnimationChannel>();

        public List<W3dAdaptiveDeltaAnimationChannel> AdaptiveDeltaChannels { get; } = new List<W3dAdaptiveDeltaAnimationChannel>();

        public List<W3dTimeCodedBitChannel> TimeCodedBitChannels { get; } = new List<W3dTimeCodedBitChannel>();

        public List<W3dMotionChannel> MotionChannels { get; } = new List<W3dMotionChannel>();

        internal static W3dCompressedAnimation Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dCompressedAnimation();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_HEADER:
                            result.Header = W3dCompressedAnimationHeader.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_CHANNEL:
                            switch (result.Header.Flavor)
                            {
                                case W3dCompressedAnimationFlavor.TimeCoded:
                                    result.TimeCodedChannels.Add(W3dTimeCodedAnimationChannel.Parse(reader, context));
                                    break;

                                case W3dCompressedAnimationFlavor.AdaptiveDelta4:
                                    result.AdaptiveDeltaChannels.Add(W3dAdaptiveDeltaAnimationChannel.Parse(reader, context, W3dAdaptiveDeltaBitCount.FourBits));
                                    break;

                                default:
                                    throw new InvalidDataException();
                            }
                            break;

                        case W3dChunkType.W3D_CHUNK_COMPRESSED_BIT_CHANNEL:
                            switch (result.Header.Flavor)
                            {
                                case W3dCompressedAnimationFlavor.TimeCoded:
                                    result.TimeCodedBitChannels.Add(W3dTimeCodedBitChannel.Parse(reader, context));
                                    break;

                                default:
                                    throw new InvalidDataException();
                            }
                            break;

                        case W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_MOTION_CHANNEL:
                            switch (result.Header.Flavor)
                            {
                                case W3dCompressedAnimationFlavor.TimeCoded:
                                    result.MotionChannels.Add(W3dMotionChannel.Parse(reader, context));
                                    break;

                                default:
                                    throw new InvalidDataException();
                            }
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
}
