using System;
using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterHeader
    {
        public uint Version { get; private set; }

        public string Name { get; private set; }

        public static W3dEmitterHeader Parse(BinaryReader reader)
        {
            return new W3dEmitterHeader
            {
                Version = reader.ReadUInt32(),
                Name = reader.ReadFixedLengthString(W3dConstants.NameLength)
            };
        }
    }

    public sealed class W3dEmitterUserData
    {
        public W3dEmitterUserDataType Type { get; private set; }

        public string Value { get; private set; }

        public static W3dEmitterUserData Parse(BinaryReader reader, uint chunkSize)
        {
            var startPosition = reader.BaseStream.Position;

            var result = new W3dEmitterUserData
            {
                Type = reader.ReadUInt32AsEnum<W3dEmitterUserDataType>(),
                Value = reader.ReadFixedLengthString((int) reader.ReadUInt32())
            };

            var endPosition = startPosition + chunkSize;
            reader.ReadBytes((int) (endPosition - reader.BaseStream.Position));

            return result;
        }
    }

    public enum W3dEmitterUserDataType : uint
    {
        Default = 0
    }

    public sealed class W3dEmitterInfo
    {
        public string TextureFileName { get; private set; }

        public float StartSize { get; private set; }
        public float EndSize { get; private set; }
        public float Lifetime { get; private set; }
        public float EmissionRate { get; private set; }
        public float MaxEmissions { get; private set; }
        public float VelocityRandom { get; private set; }
        public float PositionRandom { get; private set; }
        public float FadeTime { get; private set; }
        public float Gravity { get; private set; }
        public float Elasticity { get; private set; }

        public W3dVector Velocity { get; private set; }
        public W3dVector Acceleration { get; private set; }

        public W3dRgba StartColor { get; private set; }
        public W3dRgba EndColor { get; private set; }

        public static W3dEmitterInfo Parse(BinaryReader reader)
        {
            return new W3dEmitterInfo
            {
                TextureFileName = reader.ReadFixedLengthString(260),
                StartSize = reader.ReadSingle(),
                EndSize = reader.ReadSingle(),
                Lifetime = reader.ReadSingle(),
                EmissionRate = reader.ReadSingle(),
                MaxEmissions = reader.ReadSingle(),
                VelocityRandom = reader.ReadSingle(),
                PositionRandom = reader.ReadSingle(),
                FadeTime = reader.ReadSingle(),
                Gravity = reader.ReadSingle(),
                Elasticity = reader.ReadSingle(),

                Velocity = W3dVector.Parse(reader),
                Acceleration = W3dVector.Parse(reader),

                StartColor = W3dRgba.Parse(reader),
                EndColor = W3dRgba.Parse(reader)
            };
        }
    }

    public sealed class W3dVolumeRandomizer
    {
        public uint ClassID { get; private set; }
        public float Value1 { get; private set; }
        public float Value2 { get; private set; }
        public float Value3 { get; private set; }

        public static W3dVolumeRandomizer Parse(BinaryReader reader)
        {
            var result = new W3dVolumeRandomizer
            {
                ClassID = reader.ReadUInt32(),
                Value1 = reader.ReadSingle(),
                Value2 = reader.ReadSingle(),
                Value3 = reader.ReadSingle(),
            };

            reader.ReadBytes(4 * sizeof(uint)); // Pad

            return result;
        }
    }

    public enum W3dEmitterRenderMode : uint
    {
        TriParticles  = 0,
        QuadParticles = 1,
        Line          = 2,
        LineGrpTetra  = 3,
        LineGrpPrism  = 4
    }

    public enum W3dEmitterFrameMode : uint
    {
        Mode1x1   = 0,
        Mode2x2   = 1,
        Mode4x4   = 2,
        Mode8x8   = 3,
        Mode16x16 = 4
    }

    public sealed class W3dEmitterInfoV2
    {
        public uint BurstSize { get; private set; }
        public W3dVolumeRandomizer CreationVolume { get; private set; }
        public W3dVolumeRandomizer VelRandom { get; private set; }
        public float OutwardVel { get; private set; }
        public float VelInherit { get; private set; }
        public W3dShader Shader { get; private set; }
        public W3dEmitterRenderMode RenderMode { get; private set; }
        public W3dEmitterFrameMode FrameMode { get; private set; }

        public static W3dEmitterInfoV2 Parse(BinaryReader reader)
        {
            var result = new W3dEmitterInfoV2
            {
                BurstSize = reader.ReadUInt32(),
                CreationVolume = W3dVolumeRandomizer.Parse(reader),
                VelRandom = W3dVolumeRandomizer.Parse(reader),
                OutwardVel = reader.ReadSingle(),
                VelInherit = reader.ReadSingle(),
                Shader = W3dShader.Parse(reader),
                RenderMode = reader.ReadUInt32AsEnum<W3dEmitterRenderMode>(),
                FrameMode = reader.ReadUInt32AsEnum<W3dEmitterFrameMode>()
            };

            reader.ReadBytes(6 * sizeof(uint)); // Pad

            return result;
        }
    }

    public sealed class W3dEmitterProperties
    {
        public uint ColorKeyframesCount { get; private set; }
        public uint OpacityKeyframesCount { get; private set; }
        public uint SizeKeyframesCount { get; private set; }
        public W3dRgba ColorRandom { get; private set; }
        public float OpacityRandom { get; private set; }
        public float SizeRandom { get; private set; }

        public W3dEmitterColorKeyframe[] ColorKeyframes { get; private set; }
        public W3dEmitterOpacityKeyframe[] OpacityKeyframes { get; private set; }
        public W3dEmitterSizeKeyframe[] SizeKeyframes { get; private set; }

        public static W3dEmitterProperties Parse(BinaryReader reader)
        {
            var result = new W3dEmitterProperties
            {
                ColorKeyframesCount = reader.ReadUInt32(),
                OpacityKeyframesCount = reader.ReadUInt32(),
                SizeKeyframesCount = reader.ReadUInt32(),
                ColorRandom = W3dRgba.Parse(reader),
                OpacityRandom = reader.ReadSingle(),
                SizeRandom = reader.ReadSingle()
            };

            reader.ReadBytes(4 * sizeof(uint)); // Pad

            result.ColorKeyframes = new W3dEmitterColorKeyframe[result.ColorKeyframesCount];
            for (var i = 0; i < result.ColorKeyframesCount; i++)
            {
                result.ColorKeyframes[i] = W3dEmitterColorKeyframe.Parse(reader);
            }

            result.OpacityKeyframes = new W3dEmitterOpacityKeyframe[result.OpacityKeyframesCount];
            for (var i = 0; i < result.OpacityKeyframesCount; i++)
            {
                result.OpacityKeyframes[i] = W3dEmitterOpacityKeyframe.Parse(reader);
            }

            result.SizeKeyframes = new W3dEmitterSizeKeyframe[result.SizeKeyframesCount];
            for (var i = 0; i < result.SizeKeyframesCount; i++)
            {
                result.SizeKeyframes[i] = W3dEmitterSizeKeyframe.Parse(reader);
            }

            return result;
        }
    }

    public sealed class W3dEmitterColorKeyframe
    {
        public float Time { get; private set; }
        public W3dRgba Color { get; private set; }

        internal static W3dEmitterColorKeyframe Parse(BinaryReader reader)
        {
            return new W3dEmitterColorKeyframe
            {
                Time = reader.ReadSingle(),
                Color = W3dRgba.Parse(reader)
            };
        }
    }

    public sealed class W3dEmitterOpacityKeyframe
    {
        public float Time { get; private set; }
        public float Opacity { get; private set; }

        internal static W3dEmitterOpacityKeyframe Parse(BinaryReader reader)
        {
            return new W3dEmitterOpacityKeyframe
            {
                Time = reader.ReadSingle(),
                Opacity = reader.ReadSingle()
            };
        }
    }

    public sealed class W3dEmitterSizeKeyframe
    {
        public float Time { get; private set; }
        public float Size { get; private set; }

        internal static W3dEmitterSizeKeyframe Parse(BinaryReader reader)
        {
            return new W3dEmitterSizeKeyframe
            {
                Time = reader.ReadSingle(),
                Size = reader.ReadSingle()
            };
        }
    }

    public sealed class W3dEmitterRotationHeader
    {
        public uint KeyframeCount { get; private set; }

        /// <summary>
        /// Random initial rotational velocity (rotations/sec)
        /// </summary>
        public float Random { get; private set; }

        /// <summary>
        /// Random initial orientation (rotations 1.0=360deg)
        /// </summary>
        public float OrientationRandom { get; private set; }

        internal static W3dEmitterRotationHeader Parse(BinaryReader reader)
        {
            var result = new W3dEmitterRotationHeader
            {
                KeyframeCount = reader.ReadUInt32(),
                Random = reader.ReadSingle(),
                OrientationRandom = reader.ReadSingle()
            };

            reader.ReadBytes(sizeof(uint)); // Pad

            return result;
        }
    }

    public sealed class W3dEmitterRotationKeyframe
    {
        public float Time { get; private set; }

        /// <summary>
        /// Rotational velocity in rotations/sec
        /// </summary>
        public float Rotation { get; private set; }

        internal static W3dEmitterRotationKeyframe Parse(BinaryReader reader)
        {
            return new W3dEmitterRotationKeyframe
            {
                Time = reader.ReadSingle(),
                Rotation = reader.ReadSingle()
            };
        }
    }

    /// <summary>
    /// Frames keyframes are for sub-texture indexing.
    /// </summary>
    public sealed class W3dEmitterFrameHeader
    {
        public uint KeyframeCount { get; private set; }

        public float Random { get; private set; }

        internal static W3dEmitterFrameHeader Parse(BinaryReader reader)
        {
            var result = new W3dEmitterFrameHeader
            {
                KeyframeCount = reader.ReadUInt32(),
                Random = reader.ReadSingle()
            };

            reader.ReadBytes(2 * sizeof(uint)); // Pad

            return result;
        }
    }

    public sealed class W3dEmitterFrameKeyframe
    {
        public float Time { get; private set; }

        public float Frame { get; private set; }

        internal static W3dEmitterFrameKeyframe Parse(BinaryReader reader)
        {
            return new W3dEmitterFrameKeyframe
            {
                Time = reader.ReadSingle(),
                Frame = reader.ReadSingle()
            };
        }
    }

    public sealed class W3dEmitterRotationKeyframes
    {
        public W3dEmitterRotationHeader Header { get; private set; }

        public W3dEmitterRotationKeyframe[] Keyframes { get; private set; }

        internal static W3dEmitterRotationKeyframes Parse(BinaryReader reader)
        {
            var result = new W3dEmitterRotationKeyframes
            {
                Header = W3dEmitterRotationHeader.Parse(reader)
            };

            result.Keyframes = new W3dEmitterRotationKeyframe[result.Header.KeyframeCount + 1];
            for (var i = 0; i < result.Keyframes.Length; i++)
            {
                result.Keyframes[i] = W3dEmitterRotationKeyframe.Parse(reader);
            }

            return result;
        }
    }

    public sealed class W3dEmitterFrameKeyframes
    {
        public W3dEmitterFrameHeader Header { get; private set; }

        public W3dEmitterFrameKeyframe[] Keyframes { get; private set; }

        internal static W3dEmitterFrameKeyframes Parse(BinaryReader reader)
        {
            var result = new W3dEmitterFrameKeyframes
            {
                Header = W3dEmitterFrameHeader.Parse(reader)
            };

            result.Keyframes = new W3dEmitterFrameKeyframe[result.Header.KeyframeCount + 1];
            for (var i = 0; i < result.Keyframes.Length; i++)
            {
                result.Keyframes[i] = W3dEmitterFrameKeyframe.Parse(reader);
            }

            return result;
        }
    }

    public sealed class W3dEmitter : W3dChunk
    {
        public W3dEmitterHeader Header { get; private set; }

        public W3dEmitterUserData UserData { get; private set; }

        public W3dEmitterInfo Info { get; private set; }

        public W3dEmitterInfoV2 InfoV2 { get; private set; }

        public W3dEmitterProperties Properties { get; private set; }

        public W3dEmitterRotationKeyframes RotationKeyframes { get; private set; }

        public W3dEmitterFrameKeyframes FrameKeyframes { get; private set; }

        public static W3dEmitter Parse(BinaryReader reader, uint chunkSize)
        {
            return ParseChunk<W3dEmitter>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_EMITTER_HEADER:
                        result.Header = W3dEmitterHeader.Parse(reader);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_USER_DATA:
                        result.UserData = W3dEmitterUserData.Parse(reader, header.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_INFO:
                        result.Info = W3dEmitterInfo.Parse(reader);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_INFOV2:
                        result.InfoV2 = W3dEmitterInfoV2.Parse(reader);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_PROPS:
                        result.Properties = W3dEmitterProperties.Parse(reader);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_ROTATION_KEYFRAMES:
                        result.RotationKeyframes = W3dEmitterRotationKeyframes.Parse(reader);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_FRAME_KEYFRAMES:
                        result.FrameKeyframes = W3dEmitterFrameKeyframes.Parse(reader);
                        break;

                    default:
                        throw new InvalidDataException();
                }
            });
        }
    }
}
