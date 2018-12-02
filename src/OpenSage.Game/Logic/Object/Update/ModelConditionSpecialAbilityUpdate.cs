using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ModelConditionSpecialAbilityUpdateModuleData : UpdateModuleData
    {
        internal static ModelConditionSpecialAbilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ModelConditionSpecialAbilityUpdateModuleData> FieldParseTable = new IniParseTable<ModelConditionSpecialAbilityUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseString() },
            { "UnpackingVariation", (parser, x) => x.UnpackingVariation = parser.ParseInteger() },
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
            { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
            { "PersistentPrepTime", (parser, x) => x.PersistentPrepTime = parser.ParseInteger() },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
            { "AwardXPForTriggering", (parser, x) => x.AwardXPForTriggering = parser.ParseInteger() },
            { "GenerateTerror", (parser, x) => x.GenerateTerror = parser.ParseBoolean() },
            { "EmotionPulseRadius", (parser, x) => x.EmotionPulseRadius = parser.ParseInteger() },
            { "DisableWhenWearingTheRing", (parser, x) => x.DisableWhenWearingTheRing = parser.ParseBoolean() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public int UnpackingVariation { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int PersistentPrepTime { get; private set; }
        public int PackTime { get; private set; }
        public int AwardXPForTriggering { get; private set; }
        public bool GenerateTerror { get; private set; }
        public int EmotionPulseRadius { get; private set; }
        public bool DisableWhenWearingTheRing { get; private set; }
    }
}
