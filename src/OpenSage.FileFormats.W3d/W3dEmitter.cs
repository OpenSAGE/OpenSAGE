using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitter(
    W3dEmitterHeader Header,
    W3dEmitterUserData? UserData,
    W3dEmitterInfo? Info,
    W3dEmitterInfoV2? InfoV2,
    W3dEmitterProperties? Properties,
    W3dEmitterRotationKeyframes? RotationKeyframes,
    W3dEmitterFrameKeyframes? FrameKeyframes,
    W3dEmitterLineProperties? LineProperties,
    W3dEmitterBlurTimeKeyframes? BlurTimeKeyframes,
    W3dEmitterExtraInfo? ExtraInfo) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_EMITTER)
{
    internal static W3dEmitter Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dEmitterHeader? resultHeader = null;
            W3dEmitterUserData? userData = null;
            W3dEmitterInfo? info = null;
            W3dEmitterInfoV2? infoV2 = null;
            W3dEmitterProperties? properties = null;
            W3dEmitterRotationKeyframes? rotationKeyframes = null;
            W3dEmitterFrameKeyframes? frameKeyframes = null;
            W3dEmitterLineProperties? lineProperties = null;
            W3dEmitterBlurTimeKeyframes? blurTimeKeyframes = null;
            W3dEmitterExtraInfo? extraInfo = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_EMITTER_HEADER:
                        resultHeader = W3dEmitterHeader.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_USER_DATA:
                        userData = W3dEmitterUserData.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_INFO:
                        info = W3dEmitterInfo.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_INFOV2:
                        infoV2 = W3dEmitterInfoV2.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_PROPS:
                        properties = W3dEmitterProperties.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_ROTATION_KEYFRAMES:
                        rotationKeyframes = W3dEmitterRotationKeyframes.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_FRAME_KEYFRAMES:
                        frameKeyframes = W3dEmitterFrameKeyframes.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_LINE_PROPERTIES:
                        lineProperties = W3dEmitterLineProperties.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_BLUR_TIME_KEYFRAMES:
                        blurTimeKeyframes = W3dEmitterBlurTimeKeyframes.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER_EXTRA_INFO:
                        extraInfo = W3dEmitterExtraInfo.Parse(reader, context);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null)
            {
                throw new InvalidDataException("header should never be null");
            }

            return new W3dEmitter(resultHeader, userData, info, infoV2, properties, rotationKeyframes, frameKeyframes,
                lineProperties, blurTimeKeyframes, extraInfo);
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

public sealed record W3dEmitterExtraInfo(byte[] Unknown) : W3dChunk(W3dChunkType.W3D_CHUNK_EMITTER_EXTRA_INFO)
{
    internal static W3dEmitterExtraInfo Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var unknown = reader.ReadBytes((int)header.ChunkSize);

            return new W3dEmitterExtraInfo(unknown);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(Unknown);
    }
}
