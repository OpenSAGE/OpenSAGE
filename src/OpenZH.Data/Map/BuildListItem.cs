using System.IO;
using System.Linq;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class BuildListItem
    {
        public string BuildingName { get; private set; }
        public string Name { get; private set; }
        public MapVector3 Position { get; private set; }
        public float Angle { get; private set; }
        public bool StructureAlreadyBuilt { get; private set; }
        public uint Rebuilds { get; private set; }
        public string Script { get; private set; }
        public uint StartingHealth { get; private set; }
        public bool Unknown1 { get; private set; }
        public bool Unknown2 { get; private set; }
        public bool Unknown3 { get; private set; }

        internal static BuildListItem Parse(BinaryReader reader)
        {
            var buildingName = reader.ReadUInt16PrefixedAsciiString();

            var name = reader.ReadUInt16PrefixedAsciiString();

            var position = MapVector3.Parse(reader);
            var angle = reader.ReadSingle();

            var structureAlreadyBuilt = reader.ReadBooleanChecked();

            var rebuilds = reader.ReadUInt32();

            var script = reader.ReadUInt16PrefixedAsciiString();

            var startingHealth = reader.ReadUInt32();

            // One of these unknown booleans is the "Unsellable" checkbox in Building Properties.
            var unknown1 = reader.ReadBooleanChecked();
            var unknown2 = reader.ReadBooleanChecked();

            var unknown3 = reader.ReadBooleanChecked();
            if (!unknown3)
            {
                throw new InvalidDataException();
            }

            return new BuildListItem
            {
                BuildingName = buildingName,
                Name = name,
                Position = position,
                Angle = angle,
                StructureAlreadyBuilt = structureAlreadyBuilt,
                Rebuilds = rebuilds,
                Script = script,
                StartingHealth = startingHealth,
                Unknown1 = unknown1,
                Unknown2 = unknown2,
                Unknown3 = unknown3
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.WriteUInt16PrefixedAsciiString(BuildingName);

            writer.WriteUInt16PrefixedAsciiString(Name);

            Position.WriteTo(writer);
            writer.Write(Angle);

            writer.Write(StructureAlreadyBuilt);

            writer.Write(Rebuilds);

            writer.WriteUInt16PrefixedAsciiString(Script);

            writer.Write(StartingHealth);

            writer.Write(Unknown1);
            writer.Write(Unknown2);
            writer.Write(Unknown3);
        }
    }
}
