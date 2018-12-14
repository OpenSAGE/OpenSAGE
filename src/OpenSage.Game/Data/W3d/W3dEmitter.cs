using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitter : W3dChunk
    {
        public W3dEmitterHeader Header { get; private set; }

        public W3dEmitterUserData UserData { get; private set; }

        public W3dEmitterInfo Info { get; private set; }

        public W3dEmitterInfoV2 InfoV2 { get; private set; }

        public W3dEmitterProperties Properties { get; private set; }

        public W3dEmitterRotationKeyframes RotationKeyframes { get; private set; }

        public W3dEmitterFrameKeyframes FrameKeyframes { get; private set; }

        public W3dEmitterLineProperties LineProperties { get; private set; }

        public W3dEmitterBlurTimeKeyframes BlurTimeKeyframes { get; private set; }

        internal static W3dEmitter Parse(BinaryReader reader, uint chunkSize)
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

                    case W3dChunkType.W3D_CHUNK_EMITTER_LINE_PROPERTIES:
                        result.LineProperties = W3dEmitterLineProperties.Parse(reader);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_BLUR_TIME_KEYFRAMES:
                        result.BlurTimeKeyframes = W3dEmitterBlurTimeKeyframes.Parse(reader);
                        break;

                    case (W3dChunkType) 1293:
                        // TODO: What is this?
                        reader.ReadBytes((int)header.ChunkSize);
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_EMITTER_HEADER, false, () =>
            {
                Header.WriteTo(writer);
            });

            if (UserData != null)
            {
                WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_EMITTER_USER_DATA, false, () =>
                {
                    UserData.WriteTo(writer);
                });
            }

            if (Info != null)
            {
                WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_EMITTER_INFO, false, () =>
                {
                    Info.WriteTo(writer);
                });
            }

            if (InfoV2 != null)
            {
                WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_EMITTER_INFOV2, false, () =>
                {
                    InfoV2.WriteTo(writer);
                });
            }

            if (Properties != null)
            {
                WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_EMITTER_PROPS, false, () =>
                {
                    Properties.WriteTo(writer);
                });
            }

            if (RotationKeyframes != null)
            {
                WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_EMITTER_ROTATION_KEYFRAMES, false, () =>
                {
                    RotationKeyframes.WriteTo(writer);
                });
            }

            if (FrameKeyframes != null)
            {
                WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_EMITTER_FRAME_KEYFRAMES, false, () =>
                {
                    FrameKeyframes.WriteTo(writer);
                });
            }
        }
    }
}
