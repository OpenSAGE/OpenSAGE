using System.IO;
using System.Numerics;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class BuildListItem
    {
        private Vector3 _position;

        public string BuildingName { get; private set; }
        public string Name { get; private set; }
        public Vector3 Position => _position;
        public float Angle { get; private set; }
        public bool StructureAlreadyBuilt { get; private set; }
        public uint Rebuilds { get; private set; }
        public string Script { get; private set; }
        public bool Unknown1 { get; private set; }
        public uint StartingHealth { get; private set; }
        public bool Unknown2 { get; private set; }
        public bool Unknown3 { get; private set; }
        public bool Unknown4 { get; private set; }

        internal void Load(SaveFileReader reader)
        {
            reader.ReadVersion(2);

            BuildingName = reader.ReadAsciiString();
            Name = reader.ReadAsciiString();
            reader.ReadVector3(ref _position);

            reader.SkipUnknownBytes(8);

            Angle = reader.ReadSingle();

            var unknown3 = reader.ReadBoolean();

            Rebuilds = reader.ReadUInt32();

            reader.SkipUnknownBytes(1);

            StartingHealth = reader.ReadUInt32();

            var unknown6 = reader.ReadBoolean();
            var unknown7 = reader.ReadBoolean();
            var unknown8 = reader.ReadBoolean();
            var unknown9 = reader.ReadBoolean();
            var unknown10 = reader.ReadUInt32();
            var unknown11 = reader.ReadUInt32();
            var unknown12 = reader.ReadBoolean();

            reader.SkipUnknownBytes(40);

            var unknown13 = reader.ReadBoolean();

            var unknown14 = reader.ReadInt32();

            var unknown15 = reader.ReadBoolean();

            var unknown16 = reader.ReadInt32();
        }

        internal static BuildListItem Parse(BinaryReader reader, ushort version, ushort versionThatHasUnknownBoolean, bool mapHasAssetList)
        {
            var result = new BuildListItem
            {
                BuildingName = reader.ReadUInt16PrefixedAsciiString(),

                Name = reader.ReadUInt16PrefixedAsciiString(),

                _position = reader.ReadVector3(),
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
