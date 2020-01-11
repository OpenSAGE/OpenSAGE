using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class ReplaceObjectUpdateModuleData : UpdateModuleData
    {
        internal static ReplaceObjectUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ReplaceObjectUpdateModuleData> FieldParseTable = new IniParseTable<ReplaceObjectUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "SkipContinue", (parser, x) => x.SkipContinue = parser.ParseBoolean() },
            { "UnpackingVariation", (parser, x) => x.UnpackingVariation = parser.ParseInteger() },
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
            { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
            { "PersistentPrepTime", (parser, x) => x.PersistentPrepTime = parser.ParseInteger() },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
            { "AwardXPForTriggering", (parser, x) => x.AwardXPForTriggering = parser.ParseInteger() },
            { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseFloat() },
            { "MustFinishAbility", (parser, x) => x.MustFinishAbility = parser.ParseBoolean() },
            { "ReplaceObject", (parser, x) => x.ReplaceObject = ReplaceObject.Parse(parser) },
            { "ReplaceRadius", (parser, x) => x.ReplaceRadius = parser.ParseFloat() },
            { "ReplaceFX", (parser, x) => x.ReplaceFX = parser.ParseAssetReference() },
            { "Scatter", (parser, x) => x.Scatter = parser.ParseBoolean() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public bool SkipContinue { get; private set; }
        public int UnpackingVariation { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int PersistentPrepTime { get; private set; }
        public int PackTime { get; private set; }
        public int AwardXPForTriggering { get; private set; }
        public float StartAbilityRange { get; private set; }
        public bool MustFinishAbility { get; private set; }
        public ReplaceObject ReplaceObject { get; private set; }
        public float ReplaceRadius { get; private set; }
        public string ReplaceFX { get; private set; }
        public bool Scatter { get; private set; }
    }

    public sealed class ReplaceObject
    {
        internal static ReplaceObject Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ReplaceObject> FieldParseTable = new IniParseTable<ReplaceObject>
        {
            { "TargetObjectFilter", (parser, x) => x.TargetObjectFilter = ObjectFilter.Parse(parser) },
            { "ReplacementObjectName", (parser, x) => x.ReplacementObjectName = parser.ParseAssetReferenceArray() }
        };

        public ObjectFilter TargetObjectFilter { get; private set; }
        public string[] ReplacementObjectName { get; private set; }
    }
}
