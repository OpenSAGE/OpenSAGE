using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class PassiveAreaEffectBehaviorModuleData : UpdateModuleData
    {
        internal static PassiveAreaEffectBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PassiveAreaEffectBehaviorModuleData> FieldParseTable = new IniParseTable<PassiveAreaEffectBehaviorModuleData>
        {
            { "EffectRadius", (parser, x) => x.EffectRadius = parser.ParseInteger() },
            { "PingDelay", (parser, x) => x.PingDelay = parser.ParseInteger() },
            { "HealPercentPerSecond", (parser, x) => x.HealPercentPerSecond = parser.ParsePercentage() },
            { "AllowFilter", (parser, x) => x.AllowFilter = ObjectFilter.Parse(parser) }
        };

        public int EffectRadius { get; private set; }
        public int PingDelay { get; private set; }
        public float HealPercentPerSecond { get; private set; }
        public ObjectFilter AllowFilter { get; private set; }
    }
}
