using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterUserData
    {
        public W3dEmitterUserDataType Type { get; private set; }

        public string Value { get; private set; }

        public static W3dEmitterUserData Parse(BinaryReader reader, uint chunkSize)
        {
            var startPosition = reader.BaseStream.Position;

            var result = new W3dEmitterUserData
            {
                Type = reader.ReadUInt32AsEnum<W3dEmitterUserDataType>(),
                Value = reader.ReadFixedLengthString((int) reader.ReadUInt32())
            };

            var endPosition = startPosition + chunkSize;
            reader.ReadBytes((int) (endPosition - reader.BaseStream.Position));

            return result;
        }
    }
}
