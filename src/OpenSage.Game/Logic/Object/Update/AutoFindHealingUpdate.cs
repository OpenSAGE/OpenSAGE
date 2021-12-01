using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class AutoFindHealingUpdate : UpdateModule
    {
        // TODO

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknown = reader.ReadUInt32();
        }
    }

    /// <summary>
    /// Searches for a nearby healing station. AI only.
    /// </summary>
    public sealed class AutoFindHealingUpdateModuleData : UpdateModuleData
    {
        internal static AutoFindHealingUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AutoFindHealingUpdateModuleData> FieldParseTable = new IniParseTable<AutoFindHealingUpdateModuleData>
        {
            { "ScanRate", (parser, x) => x.ScanRate = parser.ParseInteger() },
            { "ScanRange", (parser, x) => x.ScanRange = parser.ParseInteger() },
            { "NeverHeal", (parser, x) => x.NeverHeal = parser.ParseFloat() },
            { "AlwaysHeal", (parser, x) => x.AlwaysHeal = parser.ParseFloat() }
        };

        public int ScanRate { get; private set; }
        public int ScanRange { get; private set; }
        public float NeverHeal { get; private set; }
        public float AlwaysHeal { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new AutoFindHealingUpdate();
        }
    }
}
