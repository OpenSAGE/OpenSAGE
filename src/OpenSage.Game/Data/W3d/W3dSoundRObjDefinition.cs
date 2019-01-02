using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dSoundRObjDefinition : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_SOUNDROBJ_DEFINITION;

        public byte[] UnknownBytes { get; private set; }

        internal static W3dSoundRObjDefinition Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dSoundRObjDefinition
                {
                    UnknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position)
                };

                // TODO: Determine W3dSoundRObjDefinition Chunk Structure (Currently Unknown)
                /*
                var chunkA = reader.ReadUInt32() >> 8;
                var chunkASize = reader.ReadUInt32();
                var chunkAArray = reader.ReadBytes((int)chunkASize);

                var Flag2 = reader.ReadUInt32() >> 8;
                var tmp = reader.ReadBytes(4); // unknown

                var chunkB = reader.ReadUInt32() >> 8;
                var chunkBSize = reader.ReadUInt32();
                var chunkBArray = reader.ReadBytes((int)chunkBSize);
                */

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(UnknownBytes);
        }
    }
}
