using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ToggleHiddenSpecialAbilityUpdateModuleData : UpdateModuleData
    {
        internal static ToggleHiddenSpecialAbilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ToggleHiddenSpecialAbilityUpdateModuleData> FieldParseTable = new IniParseTable<ToggleHiddenSpecialAbilityUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseString() },
            { "UnpackingVariation", (parser, x) => x.UnpackingVariation = parser.ParseInteger() },
            { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseInteger() },
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
            { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
            { "PersistentPrepTime", (parser, x) => x.PersistentPrepTime = parser.ParseInteger() },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
            { "AwardXPForTriggering", (parser, x) => x.AwardXPForTriggering = parser.ParseInteger() },
            { "EffectDuration", (parser, x) => x.EffectDuration = parser.ParseInteger() },
            { "ShowPalantirTimer", (parser, x) => x.ShowPalantirTimer = parser.ParseBoolean() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public int UnpackingVariation { get; private set; }
        public double StartAbilityRange { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int PersistentPrepTime { get; private set; }
        public int PackTime { get; private set; }
        public int AwardXPForTriggering { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int EffectDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ShowPalantirTimer { get; private set; }
    }
}
