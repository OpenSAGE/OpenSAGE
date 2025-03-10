using System.IO;

namespace OpenSage.FileFormats.W3d;

/// <param name="Attributes">Flags for this texture</param>
/// <param name="AnimationType">animation logic</param>
/// <param name="FrameCount">Number of frames (1 if not animated)</param>
/// <param name="FrameRate">Frame rate, frames per second in floating point</param>
public sealed record W3dTextureInfo(
    W3dTextureFlags Attributes,
    W3dTextureAnimation AnimationType,
    uint FrameCount,
    float FrameRate) : W3dChunk(W3dChunkType.W3D_CHUNK_TEXTURE_INFO)
{
    internal static W3dTextureInfo Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var attributes = reader.ReadUInt16AsEnumFlags<W3dTextureFlags>();
            var animationType = reader.ReadUInt16AsEnum<W3dTextureAnimation>();
            var frameCount = reader.ReadUInt32();
            var frameRate = reader.ReadSingle();

            return new W3dTextureInfo(attributes, animationType, frameCount, frameRate);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write((ushort)Attributes);
        writer.Write((ushort)AnimationType);
        writer.Write(FrameCount);
        writer.Write(FrameRate);
    }
}
