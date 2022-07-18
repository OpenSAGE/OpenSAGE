using System.IO;
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dEmitterInfo : W3dChunk
    {
        private const int TextureFileNameLength = 260;

        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_EMITTER_INFO;

        public string TextureFileName { get; private set; }

        public float StartSize { get; private set; }
        public float EndSize { get; private set; }
        public float Lifetime { get; private set; }
        public float EmissionRate { get; private set; }
        public float MaxEmissions { get; private set; }
        public float VelocityRandom { get; private set; }
        public float PositionRandom { get; private set; }
        public float FadeTime { get; private set; }
        public float Gravity { get; private set; }
        public float Elasticity { get; private set; }

        public Vector3 Velocity { get; private set; }
        public Vector3 Acceleration { get; private set; }

        public ColorRgba StartColor { get; private set; }
        public ColorRgba EndColor { get; private set; }

        internal static W3dEmitterInfo Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                return new W3dEmitterInfo
                {
                    TextureFileName = reader.ReadFixedLengthString(TextureFileNameLength),
                    StartSize = reader.ReadSingle(),
                    EndSize = reader.ReadSingle(),
                    Lifetime = reader.ReadSingle(),
                    EmissionRate = reader.ReadSingle(),
                    MaxEmissions = reader.ReadSingle(),
                    VelocityRandom = reader.ReadSingle(),
                    PositionRandom = reader.ReadSingle(),
                    FadeTime = reader.ReadSingle(),
                    Gravity = reader.ReadSingle(),
                    Elasticity = reader.ReadSingle(),

                    Velocity = reader.ReadVector3(),
                    Acceleration = reader.ReadVector3(),

                    StartColor = reader.ReadColorRgba(),
                    EndColor = reader.ReadColorRgba()
                };
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
}
