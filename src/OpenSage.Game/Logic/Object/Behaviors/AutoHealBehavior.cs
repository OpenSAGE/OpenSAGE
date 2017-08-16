using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class AutoHealBehavior : ObjectBehavior
    {
        internal static AutoHealBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AutoHealBehavior> FieldParseTable = new IniParseTable<AutoHealBehavior>
        {
            { "HealingAmount", (parser, x) => x.HealingAmount = parser.ParseInteger() },
            { "HealingDelay", (parser, x) => x.HealingDelay = parser.ParseInteger() },
            { "AffectsWholePlayer", (parser, x) => x.AffectsWholePlayer = parser.ParseBoolean() },
            { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
            { "KindOf", (parser, x) => x.KindOf = parser.ParseEnum<ObjectKinds>() },
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() },
            { "StartHealingDelay", (parser, x) => x.StartHealingDelay = parser.ParseInteger() },
        };

        public int HealingAmount { get; private set; }
        public int HealingDelay { get; private set; }
        public bool AffectsWholePlayer { get; private set; }
        public bool StartsActive { get; private set; }
        public ObjectKinds KindOf { get; private set; }
        public string[] TriggeredBy { get; private set; }
        public int StartHealingDelay { get; private set; }
    }
}
