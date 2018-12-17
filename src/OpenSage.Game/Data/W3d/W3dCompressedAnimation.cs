﻿using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dCompressedAnimation : W3dChunk
    {
        public W3dCompressedAnimationHeader Header { get; private set; }

        public IReadOnlyList<W3dTimeCodedAnimationChannel> TimeCodedChannels { get; private set; }

        public IReadOnlyList<W3dAdaptiveDeltaAnimationChannel> AdaptiveDeltaChannels { get; private set; }

        public IReadOnlyList<W3dTimeCodedBitChannel> TimeCodedBitChannels { get; private set; }

        public IReadOnlyList<W3dMotionChannel> MotionChannels { get; private set; }

        internal static W3dCompressedAnimation Parse(BinaryReader reader, uint chunkSize)
        {
            var timeCodedChannels = new List<W3dTimeCodedAnimationChannel>();
            var adaptiveDeltaChannels = new List<W3dAdaptiveDeltaAnimationChannel>();
            var timeCodedBitChannels = new List<W3dTimeCodedBitChannel>();
            var motionChannels = new List<W3dMotionChannel>();

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
                                timeCodedChannels.Add(W3dTimeCodedAnimationChannel.Parse(reader));
                                break;

                            case W3dCompressedAnimationFlavor.AdaptiveDelta4:
                                adaptiveDeltaChannels.Add(W3dAdaptiveDeltaAnimationChannel.Parse(reader, W3dAdaptiveDeltaBitCount.FourBits));
                                break;

                            default:
                                throw new InvalidDataException();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_COMPRESSED_BIT_CHANNEL:
                        switch (result.Header.Flavor)
                        {
                            case W3dCompressedAnimationFlavor.TimeCoded:
                                timeCodedBitChannels.Add(W3dTimeCodedBitChannel.Parse(reader));
                                break;

                            default:
                                throw new InvalidDataException();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_MOTION_CHANNEL:
                        switch (result.Header.Flavor)
                        {
                            case W3dCompressedAnimationFlavor.TimeCoded:
                                motionChannels.Add(W3dMotionChannel.Parse(reader, header.ChunkSize));
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
            finalResult.AdaptiveDeltaChannels = adaptiveDeltaChannels;
            finalResult.TimeCodedBitChannels = timeCodedBitChannels;
            finalResult.MotionChannels = motionChannels;

            return finalResult;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_HEADER, false, () =>
            {
                Header.WriteTo(writer);
            });

            foreach (var channel in TimeCodedChannels)
            {
                WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_CHANNEL, false, () =>
                {
                    channel.WriteTo(writer);
                });
            }

            foreach (var channel in TimeCodedBitChannels)
            {
                WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_COMPRESSED_BIT_CHANNEL, false, () =>
                {
                    channel.WriteTo(writer);
                });
            }

            foreach (var channel in AdaptiveDeltaChannels)
            {
                WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_CHANNEL, false, () =>
                {
                    channel.WriteTo(writer);
                });
            }

            foreach (var channel in MotionChannels)
            {
                WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION_MOTION_CHANNEL, false, () =>
                {
                    channel.WriteTo(writer);
                });
            }
        }
    }
}
