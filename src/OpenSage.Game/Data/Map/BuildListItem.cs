using System.IO;
using System.Numerics;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class BuildListItem : IPersistableObject
    {
        private string _buildingName;
        private string _name;
        private Vector3 _position;
        private float _angle;
        private bool _unknownBool1;
        private uint _rebuilds;
        private uint _startingHealth;
        private bool _unknownBool2;
        private bool _unknownBool3;
        private bool _unknownBool4;
        private bool _unknownBool5;
        private uint _unknownInt1;
        private uint _unknownInt2;
        private bool _unknownBool6;
        private bool _unknownBool7;
        private int _unknownInt3;
        private bool _unknownBool8;
        private int _unknownInt4;

        public string BuildingName { get; private set; }
        public string Name { get; private set; }
        public Vector3 Position => _position;
        public float Angle => _angle;
        public bool StructureAlreadyBuilt { get; private set; }
        public uint Rebuilds => _rebuilds;
        public string Script { get; private set; }
        public bool Unknown1 { get; private set; }
        public uint StartingHealth => _startingHealth;
        public bool Unknown2 { get; private set; }
        public bool Unknown3 { get; private set; }
        public bool Unknown4 { get; private set; }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.PersistAsciiString("BuildingName", ref _buildingName);
            reader.PersistAsciiString("Name", ref _name);
            reader.PersistVector3("Position", ref _position);

            reader.SkipUnknownBytes(8);

            reader.PersistSingle("Angle", ref _angle);
            reader.PersistBoolean("UnknownBool1", ref _unknownBool1);
            reader.PersistUInt32("Rebuilds", ref _rebuilds);

            reader.SkipUnknownBytes(1);

            reader.PersistUInt32("StartingHealth", ref _startingHealth);
            reader.PersistBoolean("UnknownBool2", ref _unknownBool2);
            reader.PersistBoolean("UnknownBool3", ref _unknownBool3);
            reader.PersistBoolean("UnknownBool4", ref _unknownBool4);
            reader.PersistBoolean("UnknownBool5", ref _unknownBool5);
            reader.PersistUInt32("UnknownInt1", ref _unknownInt1);
            reader.PersistUInt32("UnknownInt2", ref _unknownInt2);
            reader.PersistBoolean("UnknownBool6", ref _unknownBool6);

            reader.SkipUnknownBytes(40);

            reader.PersistBoolean("UnknownBool7", ref _unknownBool7);
            reader.PersistInt32("UnknownInt3", ref _unknownInt3);
            reader.PersistBoolean("UnknownBool8", ref _unknownBool8);
            reader.PersistInt32("UnknownInt4", ref _unknownInt4);
        }

        internal static BuildListItem Parse(BinaryReader reader, ushort version, ushort versionThatHasUnknownBoolean, bool mapHasAssetList)
        {
            var result = new BuildListItem
            {
                BuildingName = reader.ReadUInt16PrefixedAsciiString(),

                Name = reader.ReadUInt16PrefixedAsciiString(),

                _position = reader.ReadVector3(),
                _angle = reader.ReadSingle(),

                StructureAlreadyBuilt = reader.ReadBooleanChecked()
            };

            // BFME and C&C3 both used v1 for this chunk, but C&C3 has an extra boolean here.
            // If the map file has an AssetList chunk, we assume it's C&C3.
            if (mapHasAssetList && version >= versionThatHasUnknownBoolean)
            {
                result.Unknown1 = reader.ReadBooleanChecked();
            }

            result._rebuilds = reader.ReadUInt32();

            result.Script = reader.ReadUInt16PrefixedAsciiString();

            result._startingHealth = reader.ReadUInt32();

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
