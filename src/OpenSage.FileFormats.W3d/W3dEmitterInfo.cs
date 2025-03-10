using System.IO;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterInfo(
    string TextureFileName,
    float StartSize,
    float EndSize,
    float Lifetime,
    float EmissionRate,
    float MaxEmissions,
    float VelocityRandom,
    float PositionRandom,
    float FadeTime,
    float Gravity,
    float Elasticity,
    Vector3 Velocity,
    Vector3 Acceleration,
    ColorRgba StartColor,
    ColorRgba EndColor) : W3dChunk(W3dChunkType.W3D_CHUNK_EMITTER_INFO)
{
    private const int TextureFileNameLength = 260;

    internal static W3dEmitterInfo Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var textureFileName = reader.ReadFixedLengthString(TextureFileNameLength);
            var startSize = reader.ReadSingle();
            var endSize = reader.ReadSingle();
            var lifetime = reader.ReadSingle();
            var emissionRate = reader.ReadSingle();
            var maxEmissions = reader.ReadSingle();
            var velocityRandom = reader.ReadSingle();
            var positionRandom = reader.ReadSingle();
            var fadeTime = reader.ReadSingle();
            var gravity = reader.ReadSingle();
            var elasticity = reader.ReadSingle();
            var velocity = reader.ReadVector3();
            var acceleration = reader.ReadVector3();
            var startColor = reader.ReadColorRgba();
            var endColor = reader.ReadColorRgba();

            return new W3dEmitterInfo(textureFileName, startSize, endSize, lifetime, emissionRate, maxEmissions,
                velocityRandom, positionRandom, fadeTime, gravity, elasticity, velocity, acceleration, startColor,
                endColor);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.WriteFixedLengthString(TextureFileName, TextureFileNameLength);
        writer.Write(StartSize);
        writer.Write(EndSize);
        writer.Write(Lifetime);
        writer.Write(EmissionRate);
        writer.Write(MaxEmissions);
        writer.Write(VelocityRandom);
        writer.Write(PositionRandom);
        writer.Write(FadeTime);
        writer.Write(Gravity);
        writer.Write(Elasticity);

        writer.Write(Velocity);
        writer.Write(Acceleration);

        writer.Write(StartColor);
        writer.Write(EndColor);
    }
}
