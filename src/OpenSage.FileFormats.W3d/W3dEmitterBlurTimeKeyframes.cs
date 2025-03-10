using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterBlurTimeKeyframes(
    W3dEmitterBlurTimeHeader Header,
    W3dEmitterBlurTimeKeyframe[] Keyframes) : W3dChunk(W3dChunkType.W3D_CHUNK_EMITTER_BLUR_TIME_KEYFRAMES)
{
    internal static W3dEmitterBlurTimeKeyframes Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var resultHeader = W3dEmitterBlurTimeHeader.Parse(reader);

            var keyframes = new W3dEmitterBlurTimeKeyframe[resultHeader.KeyframeCount + 1];
            for (var i = 0; i < keyframes.Length; i++)
            {
                keyframes[i] = W3dEmitterBlurTimeKeyframe.Parse(reader);
            }

            return new W3dEmitterBlurTimeKeyframes(resultHeader, keyframes);
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
