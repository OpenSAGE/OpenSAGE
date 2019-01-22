using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHLodHeader : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_HLOD_HEADER;

        public uint Version { get; private set; }
        public uint LodCount { get; private set; }
        public string Name { get; private set; }

        /// <summary>
        /// Name of the hierarchy tree to use.
        /// </summary>
        public string HierarchyName { get; private set; }

        internal static W3dHLodHeader Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                return new W3dHLodHeader
                {
                    Version = reader.ReadUInt32(),
                    LodCount = reader.ReadUInt32(),
                    Name = reader.ReadFixedLengthString(W3dConstants.NameLength),
                    HierarchyName = reader.ReadFixedLengthString(W3dConstants.NameLength)
                };
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(LodCount);
            writer.WriteFixedLengthString(Name, W3dConstants.NameLength);
            writer.WriteFixedLengthString(HierarchyName, W3dConstants.NameLength);
        }
    }
}
