using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dCollectionObjName : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_COLLECTION_OBJ_NAME;
        public string[] Names { get; private set; }

        internal static W3dCollectionObjName Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var nameCount = header.ChunkSize / W3dConstants.NameLength;
                var result = new W3dCollectionObjName{};

                result.Names = new string[nameCount];
                for (int i = 0; i < nameCount; i++)
                {
                    result.Names[i] = reader.ReadFixedLengthString(W3dConstants.NameLength);
                }

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            for (var i = 0; i < Names.Length; i++)
            {
                writer.WriteFixedLengthString(Names[i], W3dConstants.NameLength);
            }
        }
    }
}
