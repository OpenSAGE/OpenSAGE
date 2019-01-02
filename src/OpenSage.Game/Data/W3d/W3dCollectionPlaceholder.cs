using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dCollectionPlaceholder : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_PLACEHOLDER;

        public uint Version { get; private set; }
        public Vector3 TransformA { get; private set; }
        public Vector3 TransformB { get; private set; }
        public Vector3 TransformC { get; private set; }
        public Vector3 TransformD { get; private set; }
        public string Name { get; private set; }

        public byte[] UnknownBytes { get; private set; }

        internal static W3dCollectionPlaceholder Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dCollectionPlaceholder
                {
                    TransformA = reader.ReadVector3(),
                    TransformB = reader.ReadVector3(),
                    TransformC = reader.ReadVector3(),
                    TransformD = reader.ReadVector3(),
                    Name = reader.ReadFixedLengthString(W3dConstants.NameLength),
                    UnknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position)
                };

                // TODO: Determine W3dCollectionPlaceholder UnknownBytes

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(TransformA);
            writer.Write(TransformB);
            writer.Write(TransformC);
            writer.Write(TransformD);
            writer.Write(Name);
            writer.Write(UnknownBytes);
        }
    }
}
