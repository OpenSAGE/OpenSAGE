using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterInfoV2(
    uint BurstSize,
    W3dVolumeRandomizer CreationVolume,
    W3dVolumeRandomizer VelRandom,
    float OutwardVel,
    float VelInherit,
    W3dShader Shader,
    W3dEmitterRenderMode RenderMode,
    W3dEmitterFrameMode FrameMode) : W3dChunk(W3dChunkType.W3D_CHUNK_EMITTER_INFOV2)
{
    internal static W3dEmitterInfoV2 Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var burstSize = reader.ReadUInt32();
            var creationVolume = W3dVolumeRandomizer.Parse(reader);
            var velRandom = W3dVolumeRandomizer.Parse(reader);
            var outwardVel = reader.ReadSingle();
            var velInherit = reader.ReadSingle();
            var shader = W3dShader.Parse(reader);
            var renderMode = reader.ReadUInt32AsEnum<W3dEmitterRenderMode>();
            var frameMode = reader.ReadUInt32AsEnum<W3dEmitterFrameMode>();

            reader.ReadBytes(6 * sizeof(uint)); // Pad

            return new W3dEmitterInfoV2(burstSize, creationVolume, velRandom, outwardVel, velInherit, shader, renderMode, frameMode);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(BurstSize);
        CreationVolume.WriteTo(writer);
        VelRandom.WriteTo(writer);
        writer.Write(OutwardVel);
        writer.Write(VelInherit);
        Shader.WriteTo(writer);
        writer.Write((uint)RenderMode);
        writer.Write((uint)FrameMode);

        // Pad
        for (var i = 0; i < 6; i++)
        {
            writer.Write((uint)0);
        }
    }
}
