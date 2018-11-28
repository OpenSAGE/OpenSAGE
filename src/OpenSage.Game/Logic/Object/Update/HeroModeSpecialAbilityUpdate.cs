using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class HeroModeSpecialAbilityUpdateModuleData : UpdateModuleData
    {
        internal static HeroModeSpecialAbilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<HeroModeSpecialAbilityUpdateModuleData> FieldParseTable = new IniParseTable<HeroModeSpecialAbilityUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseString() },
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
            { "HeroEffectDuration", (parser, x) => x.HeroEffectDuration = parser.ParseInteger() },
            { "RequiredConditions", (parser, x) => x.RequiredConditions = parser.ParseEnumBitArray<ModelConditionFlag>() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public int UnpackTime { get; private set; }
        public int HeroEffectDuration { get; private set; }
        public BitArray<ModelConditionFlag> RequiredConditions { get; private set; }
    }
}
