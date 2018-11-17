using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public class ActiveBodyModuleData : BodyModuleData
    {
        internal static ActiveBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<ActiveBodyModuleData> FieldParseTable = new IniParseTable<ActiveBodyModuleData>
        {
            { "MaxHealth", (parser, x) => x.MaxHealth = parser.ParseFloat() },
            { "InitialHealth", (parser, x) => x.InitialHealth = parser.ParseFloat() },
            { "MaxHealthDamaged", (parser, x) => x.MaxHealthDamaged = parser.ParseFloat() },
            { "RecoveryTime", (parser, x) => x.RecoveryTime = parser.ParseFloat() },

            { "SubdualDamageCap", (parser, x) => x.SubdualDamageCap = parser.ParseInteger() },
            { "SubdualDamageHealRate", (parser, x) => x.SubdualDamageHealRate = parser.ParseInteger() },
            { "SubdualDamageHealAmount", (parser, x) => x.SubdualDamageHealAmount = parser.ParseInteger() }
        };

        public float MaxHealth { get; private set; }
        public float InitialHealth { get; private set; }
        [AddedIn(SageGame.Bfme)]
        public float MaxHealthDamaged { get; private set; }
        [AddedIn(SageGame.Bfme)]
        public float RecoveryTime { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int SubdualDamageCap { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int SubdualDamageHealRate { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int SubdualDamageHealAmount { get; private set; }
    }
}
