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
            { "RequiredConditions", (parser, x) => x.RequiredConditions = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "HeroAttributeModifier", (parser, x) => x.HeroAttributeModifier = parser.ParseIdentifier() },
            { "TriggerSound", (parser, x) => x.TriggerSound = parser.ParseAssetReference() },
            { "UnpackingVariation", (parser, x) => x.UnpackingVariation = parser.ParseInteger() },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
            { "AwardXPForTriggering", (parser, x) => x.AwardXPForTriggering = parser.ParseInteger() },
            { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
            { "StopUnitBeforeActivating", (parser, x) => x.StopUnitBeforeActivating = parser.ParseBoolean() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public int UnpackTime { get; private set; }
        public int HeroEffectDuration { get; private set; }
        public BitArray<ModelConditionFlag> RequiredConditions { get; private set; }
        public string HeroAttributeModifier { get; private set; }
        public string TriggerSound { get; private set; }
        public int UnpackingVariation { get; private set; }
        public int PackTime { get; private set; }
        public int AwardXPForTriggering { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int PreparationTime { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool StopUnitBeforeActivating { get; private set; }
    }
}
