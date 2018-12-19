using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitter : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_EMITTER;

        public W3dEmitterHeader Header { get; private set; }

        public W3dEmitterUserData UserData { get; private set; }

        public W3dEmitterInfo Info { get; private set; }

        public W3dEmitterInfoV2 InfoV2 { get; private set; }

        public W3dEmitterProperties Properties { get; private set; }

        public W3dEmitterRotationKeyframes RotationKeyframes { get; private set; }

        public W3dEmitterFrameKeyframes FrameKeyframes { get; private set; }

        public W3dEmitterLineProperties LineProperties { get; private set; }

        public W3dEmitterBlurTimeKeyframes BlurTimeKeyframes { get; private set; }

        public W3dEmitterExtraInfo ExtraInfo { get; private set; }

        internal static W3dEmitter Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dEmitter();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_EMITTER_HEADER:
                            result.Header = W3dEmitterHeader.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_EMITTER_USER_DATA:
                            result.UserData = W3dEmitterUserData.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_EMITTER_INFO:
                            result.Info = W3dEmitterInfo.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_EMITTER_INFOV2:
                            result.InfoV2 = W3dEmitterInfoV2.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_EMITTER_PROPS:
                            result.Properties = W3dEmitterProperties.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_EMITTER_ROTATION_KEYFRAMES:
                            result.RotationKeyframes = W3dEmitterRotationKeyframes.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_EMITTER_FRAME_KEYFRAMES:
                            result.FrameKeyframes = W3dEmitterFrameKeyframes.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_EMITTER_LINE_PROPERTIES:
                            result.LineProperties = W3dEmitterLineProperties.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_EMITTER_BLUR_TIME_KEYFRAMES:
                            result.BlurTimeKeyframes = W3dEmitterBlurTimeKeyframes.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_EMITTER_EXTRA_INFO:
                            result.ExtraInfo = W3dEmitterExtraInfo.Parse(reader, context);
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

            if (UserData != null)
            {
                yield return UserData;
            }

            if (Info != null)
            {
                yield return Info;
            }

            if (InfoV2 != null)
            {
                yield return InfoV2;
            }

            if (Properties != null)
            {
                yield return Properties;
            }

            if (LineProperties != null)
            {
                yield return LineProperties;
            }

            if (RotationKeyframes != null)
            {
                yield return RotationKeyframes;
            }

            if (FrameKeyframes != null)
            {
                yield return FrameKeyframes;
            }

            if (BlurTimeKeyframes != null)
            {
                yield return BlurTimeKeyframes;
            }

            if (ExtraInfo != null)
            {
                yield return ExtraInfo;
            }
        }
    }

    public sealed class W3dEmitterExtraInfo : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_EMITTER_EXTRA_INFO;

        public byte[] Unknown { get; private set; }

        internal static W3dEmitterExtraInfo Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                return new W3dEmitterExtraInfo
                {
                    Unknown = reader.ReadBytes((int) header.ChunkSize)
                };
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(Unknown);
        }
    }
}
