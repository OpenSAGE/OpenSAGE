using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dRing(
    W3dRingHeader Header,
    W3dRingPlaceholder Placeholder,
    W3dRingColorInfo ColorInfo,
    W3dRingOpacityInfo OpacityInfo,
    W3dRingScaleKeyFrames InnerScaleKeyFrames,
    W3dRingScaleKeyFrames OuterScaleKeyFrames,
    W3dRingShaderFunc Shader,
    uint UnknownFlag,
    uint TextureTiling) : W3dChunk(W3dChunkType.W3D_CHUNK_RING)
{
    internal static W3dRing Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var resultHeader = W3dRingHeader.Parse(reader);

            var shader = (W3dRingShaderFunc)(reader.ReadUInt32() >> 24);
            var unknownFlag = (reader.ReadUInt32() >> 24);  // TODO: Determine What this flag is/does.

            var placeholder = W3dRingPlaceholder.Parse(reader);
            var textureTiling = reader.ReadUInt32();
            var colorInfo = W3dRingColorInfo.Parse(reader);
            var opacityInfo = W3dRingOpacityInfo.Parse(reader);
            var innerScaleKeyFrames = W3dRingScaleKeyFrames.Parse(reader);
            var outerScaleKeyFrames = W3dRingScaleKeyFrames.Parse(reader);

            return new W3dRing(resultHeader, placeholder, colorInfo, opacityInfo, innerScaleKeyFrames,
                outerScaleKeyFrames, shader, unknownFlag, textureTiling);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        Header.Write(writer);

        writer.Write((byte)Shader);
        writer.Write(UnknownFlag);

        Placeholder.Write(writer);

        writer.Write(TextureTiling);

        ColorInfo.Write(writer);
        OpacityInfo.Write(writer);
        InnerScaleKeyFrames.Write(writer);
        OuterScaleKeyFrames.Write(writer);
    }
}
