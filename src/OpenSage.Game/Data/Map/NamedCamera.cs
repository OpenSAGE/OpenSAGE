using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class NamedCamera
    {
        public Vector3 LookAtPoint { get; private set; }
        public string Name { get; private set; }

        public Vector3 Position { get; private set; }
        public Vector3 Orientation { get; private set; }

        internal static NamedCamera Parse(BinaryReader reader)
        {
            return new NamedCamera
            {
                LookAtPoint = reader.ReadVector3(),
                Name = reader.ReadUInt16PrefixedAsciiString(),

                Position = reader.ReadVector3(),
                Orientation = reader.ReadVector3()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(LookAtPoint);
            writer.WriteUInt16PrefixedAsciiString(Name);

            writer.Write(Position);
            writer.Write(Orientation);
        }
    }
}
