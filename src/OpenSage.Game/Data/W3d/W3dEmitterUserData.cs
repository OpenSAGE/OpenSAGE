using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterUserData
    {
        public W3dEmitterUserDataType Type { get; private set; }

        public string Value { get; private set; }

        public uint NumPadBytes { get; private set; }

        internal static W3dEmitterUserData Parse(BinaryReader reader, uint chunkSize)
        {
            var startPosition = reader.BaseStream.Position;

            var result = new W3dEmitterUserData
            {
                Type = reader.ReadUInt32AsEnum<W3dEmitterUserDataType>(),
                Value = reader.ReadFixedLengthString((int) reader.ReadUInt32())
            };

            var endPosition = startPosition + chunkSize;
            result.NumPadBytes = (uint) (endPosition - reader.BaseStream.Position);
            reader.ReadBytes((int) result.NumPadBytes);

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write((uint) Type);
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
                writer.Write((byte) 0);
            }
        }
    }
}
