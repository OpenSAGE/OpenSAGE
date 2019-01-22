using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dEmitterLineProperties : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_EMITTER_LINE_PROPERTIES;

        public W3dEmitterLineFlags Flags { get; private set; }
        public uint SubdivisionLevel { get; private set; }
        public float NoiseAmplitude { get; private set; }
        public float MergeAbortFactor { get; private set; }
        public float TextureTileFactor { get; private set; }
        public float UPerSec { get; private set; }
        public float VPerSec { get; private set; }

        internal static W3dEmitterLineProperties Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dEmitterLineProperties
                {
                    Flags = reader.ReadUInt32AsEnumFlags<W3dEmitterLineFlags>(),
                    SubdivisionLevel = reader.ReadUInt32(),
                    NoiseAmplitude = reader.ReadSingle(),
                    MergeAbortFactor = reader.ReadSingle(),
                    TextureTileFactor = reader.ReadSingle(),
                    UPerSec = reader.ReadSingle(),
                    VPerSec = reader.ReadSingle()
                };

                reader.ReadBytes(sizeof(uint) * 9); // Padding

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write((uint) Flags);
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
}
