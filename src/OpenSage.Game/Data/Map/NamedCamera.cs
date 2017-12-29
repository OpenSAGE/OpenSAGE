using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class NamedCamera
    {
        public Vector3 LookAtPoint { get; private set; }
        public string Name { get; private set; }

        public Vector3 Unknown1 { get; private set; }
        public Vector3 Unknown2 { get; private set; }

        internal static NamedCamera Parse(BinaryReader reader)
        {
            return new NamedCamera
            {
                LookAtPoint = reader.ReadVector3(),
                Name = reader.ReadUInt16PrefixedAsciiString(),

                Unknown1 = reader.ReadVector3(),
                Unknown2 = reader.ReadVector3()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(LookAtPoint);
            writer.WriteUInt16PrefixedAsciiString(Name);

            writer.Write(Unknown1);
            writer.Write(Unknown2);
        }
    }
}
