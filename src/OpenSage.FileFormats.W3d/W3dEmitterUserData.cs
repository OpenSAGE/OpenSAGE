using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dEmitterUserData(
    W3dEmitterUserDataType Type,
    string Value,
    uint NumPadBytes) : W3dChunk(W3dChunkType.W3D_CHUNK_EMITTER_USER_DATA)
{
    internal static W3dEmitterUserData Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var type = reader.ReadUInt32AsEnum<W3dEmitterUserDataType>();
            var value = reader.ReadFixedLengthString((int)reader.ReadUInt32());

            var numPadBytes = (uint)(context.CurrentEndPosition - reader.BaseStream.Position);
            reader.ReadBytes((int)numPadBytes);

            return new W3dEmitterUserData(type, value, numPadBytes);
        });
    }

    protected override void WriteToOverride(BinaryWriter writer)
    {
        writer.Write((uint)Type);
        if (Value.Length > 0)
        {
            writer.Write(Value.Length + 1);
            writer.WriteFixedLengthString(Value, Value.Length + 1);
        }
        else
        {
            writer.Write(0u);
        }

        for (var i = 0; i < NumPadBytes; i++)
        {
            writer.Write((byte)0);
        }
    }
}
