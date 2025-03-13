using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dSoundRObjDefinition(byte[] UnknownBytes) : W3dChunk(W3dChunkType.W3D_CHUNK_SOUNDROBJ_DEFINITION)
{
    internal static W3dSoundRObjDefinition Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var unknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position);

            // TODO: Determine W3dSoundRObjDefinition Chunk Structure (Currently Unknown)
            /*
            var chunkA = reader.ReadUInt32() >> 8;
            var chunkASize = reader.ReadUInt32();
            var chunkAArray = reader.ReadBytes((int) chunkASize);

            var Flag2 = reader.ReadUInt32() >> 8;
            var tmp = reader.ReadBytes(4); // unknown

            var chunkB = reader.ReadUInt32() >> 8;
            var chunkBSize = reader.ReadUInt32();
            var chunkBArray = reader.ReadBytes((int) chunkBSize);
            */

            return new W3dSoundRObjDefinition(unknownBytes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write(UnknownBytes);
    }
}
