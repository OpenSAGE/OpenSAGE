using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHLodSubObject : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_HLOD_SUB_OBJECT;

        public uint BoneIndex { get; private set; }

        public string Name { get; private set; }

        internal static W3dHLodSubObject Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                return new W3dHLodSubObject
                {
                    BoneIndex = reader.ReadUInt32(),
                    Name = reader.ReadFixedLengthString(W3dConstants.NameLength * 2)
                };
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(BoneIndex);
            writer.WriteFixedLengthString(Name, W3dConstants.NameLength * 2);
        }
    }
}
