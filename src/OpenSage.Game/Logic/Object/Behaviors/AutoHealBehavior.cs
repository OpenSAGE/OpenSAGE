using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class AutoHealBehaviorModuleData : UpgradeModuleData
    {
        internal static AutoHealBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<AutoHealBehaviorModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<AutoHealBehaviorModuleData>
            {
                { "HealingAmount", (parser, x) => x.HealingAmount = parser.ParseInteger() },
                { "HealingDelay", (parser, x) => x.HealingDelay = parser.ParseInteger() },
                { "AffectsWholePlayer", (parser, x) => x.AffectsWholePlayer = parser.ParseBoolean() },
                { "KindOf", (parser, x) => x.KindOf = parser.ParseEnum<ObjectKinds>() },
                { "ForbiddenKindOf", (parser, x) => x.ForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
                { "StartHealingDelay", (parser, x) => x.StartHealingDelay = parser.ParseInteger() },
                { "Radius", (parser, x) => x.Radius = parser.ParseFloat() },
                { "SingleBurst", (parser, x) => x.SingleBurst = parser.ParseBoolean() },
                { "SkipSelfForHealing", (parser, x) => x.SkipSelfForHealing = parser.ParseBoolean() },
            });

        public int HealingAmount { get; private set; }
        public int HealingDelay { get; private set; }
        public bool AffectsWholePlayer { get; private set; }
        public ObjectKinds KindOf { get; private set; }
        public ObjectKinds ForbiddenKindOf { get; private set; }
        public int StartHealingDelay { get; private set; }
        public float Radius { get; private set; }
        public bool SingleBurst { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool SkipSelfForHealing { get; private set; }
    }
}
