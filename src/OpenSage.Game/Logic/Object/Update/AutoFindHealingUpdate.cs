using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Searches for a nearby healing station. AI only.
    /// </summary>
    public sealed class AutoFindHealingUpdate : ObjectBehavior
    {
        internal static AutoFindHealingUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AutoFindHealingUpdate> FieldParseTable = new IniParseTable<AutoFindHealingUpdate>
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
    }
}
