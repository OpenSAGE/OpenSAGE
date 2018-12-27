using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class HeroModeSpecialAbilityUpdateModuleData : SpecialAbilityUpdateModuleData
    {
        internal new static HeroModeSpecialAbilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<HeroModeSpecialAbilityUpdateModuleData> FieldParseTable = SpecialAbilityUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<HeroModeSpecialAbilityUpdateModuleData>
        {
            { "HeroEffectDuration", (parser, x) => x.HeroEffectDuration = parser.ParseInteger() },
            { "HeroAttributeModifier", (parser, x) => x.HeroAttributeModifier = parser.ParseIdentifier() },
            { "StopUnitBeforeActivating", (parser, x) => x.StopUnitBeforeActivating = parser.ParseBoolean() },
        });

        public int HeroEffectDuration { get; private set; }
        public string HeroAttributeModifier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool StopUnitBeforeActivating { get; private set; }
    }
}
