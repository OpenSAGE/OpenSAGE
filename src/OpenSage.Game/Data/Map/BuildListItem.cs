using System.IO;
using System.Numerics;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class BuildListItem
    {
        public string BuildingName { get; private set; }
        public string Name { get; private set; }
        public Vector3 Position { get; private set; }
        public float Angle { get; private set; }
        public bool StructureAlreadyBuilt { get; private set; }
        public uint Rebuilds { get; private set; }
        public string Script { get; private set; }
        public bool Unknown1 { get; private set; }
        public uint StartingHealth { get; private set; }
        public bool Unknown2 { get; private set; }
        public bool Unknown3 { get; private set; }
        public bool Unknown4 { get; private set; }

        internal void ReadFromSaveFile(BinaryReader reader)
        {
            var version = reader.ReadByte();

            BuildingName = reader.ReadBytePrefixedAsciiString();
            Name = reader.ReadBytePrefixedAsciiString();
            Position = reader.ReadVector3();

            var unknown1 = reader.ReadUInt32();
            if (unknown1 != 0u)
            {
                throw new InvalidDataException();
            }

            var unknown2 = reader.ReadUInt32();
            if (unknown2 != 0u)
            {
                throw new InvalidDataException();
            }

            Angle = reader.ReadSingle();

            var unknown3 = reader.ReadBooleanChecked();

            var unknown4 = reader.ReadByte();
            if (unknown4 != 0)
            {
                throw new InvalidDataException();
            }

            var unknown5 = reader.ReadUInt32();
            if (unknown5 != 0u)
            {
                throw new InvalidDataException();
            }

            StartingHealth = reader.ReadUInt32();

            var unknown6 = reader.ReadBooleanChecked();
            var unknown7 = reader.ReadBooleanChecked();
            var unknown8 = reader.ReadBooleanChecked();
            var unknown9 = reader.ReadBooleanChecked();
            var unknown10 = reader.ReadUInt32();
            var unknown11 = reader.ReadUInt32();

            for (var i = 0; i < 51; i++)
            {
                var unknown12 = reader.ReadByte();
                if (unknown12 != 0)
                {
                    throw new InvalidDataException();
                }
            }
        }

        internal static BuildListItem Parse(BinaryReader reader, ushort version, ushort versionThatHasUnknownBoolean, bool mapHasAssetList)
        {
            var result = new BuildListItem
            {
                BuildingName = reader.ReadUInt16PrefixedAsciiString(),

                Name = reader.ReadUInt16PrefixedAsciiString(),

                Position = reader.ReadVector3(),
                Angle = reader.ReadSingle(),

                StructureAlreadyBuilt = reader.ReadBooleanChecked()
            };

            // BFME and C&C3 both used v1 for this chunk, but C&C3 has an extra boolean here.
            // If the map file has an AssetList chunk, we assume it's C&C3.
            if (mapHasAssetList && version >= versionThatHasUnknownBoolean)
            {
                result.Unknown1 = reader.ReadBooleanChecked();
            }

            result.Rebuilds = reader.ReadUInt32();

            result.Script = reader.ReadUInt16PrefixedAsciiString();

            result.StartingHealth = reader.ReadUInt32();

            // One of these unknown booleans is the "Unsellable" checkbox in Building Properties.
            result.Unknown2 = reader.ReadBooleanChecked();
            result.Unknown3 = reader.ReadBooleanChecked();
            result.Unknown4 = reader.ReadBooleanChecked();

            return result;
        }

        internal void WriteTo(BinaryWriter writer, ushort version, ushort versionThatHasUnknownBoolean, bool mapHasAssetList)
        {
            writer.WriteUInt16PrefixedAsciiString(BuildingName);

            writer.WriteUInt16PrefixedAsciiString(Name);

            writer.Write(Position);
            writer.Write(Angle);

            writer.Write(StructureAlreadyBuilt);

            if (mapHasAssetList && version >= versionThatHasUnknownBoolean)
            {
                writer.Write(Unknown1);
            }

            writer.Write(Rebuilds);

            writer.WriteUInt16PrefixedAsciiString(Script);

            writer.Write(StartingHealth);

            writer.Write(Unknown2);
            writer.Write(Unknown3);
            writer.Write(Unknown4);
        }
    }
}
