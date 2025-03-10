using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterLineProperties(
    W3dEmitterLineFlags Flags,
    uint SubdivisionLevel,
    float NoiseAmplitude,
    float MergeAbortFactor,
    float TextureTileFactor,
    float UPerSec,
    float VPerSec) : W3dChunk(W3dChunkType.W3D_CHUNK_EMITTER_LINE_PROPERTIES)
{
    internal static W3dEmitterLineProperties Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var flags = reader.ReadUInt32AsEnumFlags<W3dEmitterLineFlags>();
            var subdivisionLevel = reader.ReadUInt32();
            var noiseAmplitude = reader.ReadSingle();
            var mergeAbortFactor = reader.ReadSingle();
            var textureTileFactor = reader.ReadSingle();
            var uPerSec = reader.ReadSingle();
            var vPerSec = reader.ReadSingle();

            reader.ReadBytes(sizeof(uint) * 9); // Padding

            return new W3dEmitterLineProperties(flags, subdivisionLevel, noiseAmplitude, mergeAbortFactor, textureTileFactor, uPerSec, vPerSec);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write((uint)Flags);
        writer.Write(SubdivisionLevel);
        writer.Write(NoiseAmplitude);
        writer.Write(MergeAbortFactor);
        writer.Write(TextureTileFactor);
        writer.Write(UPerSec);
        writer.Write(VPerSec);

        for (var i = 0; i < 9; i++) // Padding
        {
            writer.Write(0u);
        }
    }
}
