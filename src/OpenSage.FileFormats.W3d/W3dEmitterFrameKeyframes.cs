using System;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterFrameKeyframes(W3dEmitterFrameHeader Header, W3dEmitterFrameKeyframe[] Keyframes) : W3dChunk(W3dChunkType.W3D_CHUNK_EMITTER_FRAME_KEYFRAMES)
{
    internal static W3dEmitterFrameKeyframes Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var resultHeader = W3dEmitterFrameHeader.Parse(reader);

            W3dEmitterFrameKeyframe[] keyframes = [];

            // Certain enb w3d's break without this check. (example: astro00.w3d)
            if (reader.BaseStream.Position < context.CurrentEndPosition)
            {
                var remaining = (int)context.CurrentEndPosition - (int)reader.BaseStream.Position;
                var keyframeCount = remaining / 8;
                keyframes = new W3dEmitterFrameKeyframe[keyframeCount];
                for (var i = 0; i < keyframes.Length; i++)
                {
                    keyframes[i] = W3dEmitterFrameKeyframe.Parse(reader);
                }
            }

            return new W3dEmitterFrameKeyframes(resultHeader, keyframes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        Header.WriteTo(writer);

        foreach (var keyframe in Keyframes)
        {
            keyframe.WriteTo(writer);
        }
    }
}
