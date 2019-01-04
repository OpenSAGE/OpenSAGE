using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dCollectionTransformNode : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_TRANSFORM_NODE;

        public string Name { get; private set; }

        internal static W3dCollectionTransformNode Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dCollectionTransformNode
                {
                    Name = reader.ReadFixedLengthString((int) header.ChunkSize),
                };

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.WriteFixedLengthString(Name, Name.Length + 1);
        }
    }
}
