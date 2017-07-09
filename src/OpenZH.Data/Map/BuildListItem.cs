using System.IO;
using System.Linq;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class BuildListItem
    {
        public string Name { get; private set; }
        public MapVector3 Position { get; private set; }
        public float Angle { get; private set; }
        public bool StructureAlreadyBuilt { get; private set; }
        public uint Rebuilds { get; private set; }

        public static BuildListItem Parse(BinaryReader reader)
        {
            var unknown1 = reader.ReadUInt16();
            if (unknown1 != 0)
            {
                throw new InvalidDataException();
            }

            var name = reader.ReadUInt16PrefixedAsciiString();

            var position = MapVector3.Parse(reader);
            var angle = reader.ReadSingle();

            var structureAlreadyBuilt = reader.ReadBoolean();

            var rebuilds = reader.ReadUInt32();

            var unknown2 = reader.ReadBytes(9);
            if (!unknown2.SequenceEqual(new byte[] { 0, 0, 100, 0, 0, 0, 1, 0, 1 }))
            {
                throw new InvalidDataException();
            }

            return new BuildListItem
            {
                Name = name,
                Position = position,
                Angle = angle,
                StructureAlreadyBuilt = structureAlreadyBuilt,
                Rebuilds = rebuilds
            };
        }
    }
}
