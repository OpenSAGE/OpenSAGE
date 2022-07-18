using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dCollisionNode : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_COLLISION_NODE;

        public byte[] UnknownBytes { get; private set; }

        internal static W3dCollisionNode Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dCollisionNode
                {
                    UnknownBytes = reader.ReadBytes((int) context.CurrentEndPosition - (int) reader.BaseStream.Position)
                };

                // TODO: Determine W3dCollisionNode UnknownBytes

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(UnknownBytes);
        }
    }
}
