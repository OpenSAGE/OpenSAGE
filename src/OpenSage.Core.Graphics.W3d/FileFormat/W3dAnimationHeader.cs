using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dAnimationHeader : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_ANIMATION_HEADER;

        public uint Version { get; private set; }

        public string Name { get; private set; }

        public string HierarchyName { get; private set; }

        public uint NumFrames { get; private set; }

        public uint FrameRate { get; private set; }

        internal static W3dAnimationHeader Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                return new W3dAnimationHeader
                {
                    Version = reader.ReadUInt32(),
                    Name = reader.ReadFixedLengthString(W3dConstants.NameLength),
                    HierarchyName = reader.ReadFixedLengthString(W3dConstants.NameLength),
                    NumFrames = reader.ReadUInt32(),
                    FrameRate = reader.ReadUInt32()
                };
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
            writer.WriteFixedLengthString(HierarchyName, W3dConstants.NameLength);
            writer.Write(NumFrames);
            writer.Write(FrameRate);
        }
    }
}
