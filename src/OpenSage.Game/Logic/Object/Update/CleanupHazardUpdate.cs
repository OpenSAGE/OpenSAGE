using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class CleanupHazardUpdate : UpdateModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.SkipUnknownBytes(5);
            byte unknown = 0;
            reader.PersistByte(ref unknown); // I have no idea what this is for
            reader.SkipUnknownBytes(23);
        }
    }

    public sealed class CleanupHazardUpdateModuleData : UpdateModuleData
    {
        internal static CleanupHazardUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CleanupHazardUpdateModuleData> FieldParseTable = new IniParseTable<CleanupHazardUpdateModuleData>
        {
            { "WeaponSlot", (parser, x) => x.WeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "ScanRate", (parser, x) => x.ScanRate = parser.ParseInteger() },
            { "ScanRange", (parser, x) => x.ScanRange = parser.ParseFloat() }
        };

        public WeaponSlot WeaponSlot { get; private set; }
        public int ScanRate { get; private set; }
        public float ScanRange { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new CleanupHazardUpdate();
        }
    }
}
