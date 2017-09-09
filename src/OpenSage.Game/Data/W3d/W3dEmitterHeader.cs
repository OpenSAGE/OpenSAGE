using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterHeader
    {
        public uint Version { get; private set; }

        public string Name { get; private set; }

        public static W3dEmitterHeader Parse(BinaryReader reader)
        {
            return new W3dEmitterHeader
            {
                Version = reader.ReadUInt32(),
                Name = reader.ReadFixedLengthString(W3dConstants.NameLength)
            };
        }
    }
}
