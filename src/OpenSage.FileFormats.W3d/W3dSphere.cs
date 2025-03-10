using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSphere(
    W3dSphereHeader Header,
    W3dSpherePlaceholder Placeholder,
    W3dSphereColors Colors,
    W3dSphereOpacityInfo OpacityInfo,
    W3dSphereScaleKeyFrames ScaleKeyFrames,
    W3dSphereAlphaVectors AlphaVectors,
    W3dRingShaderFunc Shader,
    uint UnknownFlag) : W3dChunk(W3dChunkType.W3D_CHUNK_SPHERE)
{
    internal static W3dSphere Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var resultHeader = W3dSphereHeader.Parse(reader);

            var shader = (W3dRingShaderFunc)(reader.ReadUInt32() >> 24);
            var unknownFlag = (reader.ReadUInt32() >> 24);  // TODO: Determine What this Flag is/does.

            var placeholder = W3dSpherePlaceholder.Parse(reader);
            var colors = W3dSphereColors.Parse(reader);
            var opacityInfo = W3dSphereOpacityInfo.Parse(reader);
            var scaleKeyFrames = W3dSphereScaleKeyFrames.Parse(reader);
            var alphaVectors = W3dSphereAlphaVectors.Parse(reader);

            return new W3dSphere(resultHeader, placeholder, colors, opacityInfo, scaleKeyFrames, alphaVectors, shader,
                unknownFlag);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        Header.Write(writer);

        writer.Write((byte)Shader);
        writer.Write(UnknownFlag);

        Placeholder.Write(writer);
        Colors.Write(writer);
        OpacityInfo.Write(writer);
        ScaleKeyFrames.Write(writer);
        AlphaVectors.Write(writer);
    }
}
