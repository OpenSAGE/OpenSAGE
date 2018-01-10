using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    /// <summary>
    /// Used for BFME onwards?
    /// </summary>
    public sealed class BuildListItem2
    {
        public ushort Unknown1 { get; private set; }
        public string BuildingName { get; private set; }
        public byte[] Unknown2 { get; private set; }

        internal static BuildListItem2 Parse(BinaryReader reader, ushort version)
        {
            var unknown = reader.ReadUInt16();
            if (unknown != 0)
            {
                throw new InvalidDataException();
            }

            var buildingName = reader.ReadUInt16PrefixedAsciiString();

            var unknown2 = reader.ReadBytes(version >= 1 ? 31 : 30); // TODO

            return new BuildListItem2
            {
                Unknown1 = unknown,
                BuildingName = buildingName,
                Unknown2 = unknown2
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(Unknown1);
            writer.WriteUInt16PrefixedAsciiString(BuildingName);
            writer.Write(Unknown2);
        }
    }
}
