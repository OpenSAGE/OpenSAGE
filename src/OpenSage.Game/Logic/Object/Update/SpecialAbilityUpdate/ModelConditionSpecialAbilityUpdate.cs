using OpenSage.Data.Ini;

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
            { "DisableWhenWearingTheRing", (parser, x) => x.DisableWhenWearingTheRing = parser.ParseBoolean() },
            { "WhichSpecialPower", (parser, x) => x.WhichSpecialPower = parser.ParseInteger() },
            { "ObjectFilter", (parser, x) => x.ObjectFilter = ObjectFilter.Parse(parser) },
            { "TriggerSound", (parser, x) => x.TriggerSound = parser.ParseAssetReference() },
            { "MustFinishAbility", (parser, x) => x.MustFinishAbility = parser.ParseBoolean() },
            { "LoseStealthOnTrigger", (parser, x) => x.LoseStealthOnTrigger = parser.ParseBoolean() },
            { "PreTriggerUnstealthTime", (parser, x) => x.PreTriggerUnstealthTime = parser.ParseInteger() },
            { "GenerateUncontrollableFear", (parser, x) => x.GenerateUncontrollableFear = parser.ParseBoolean() }
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
        public int WhichSpecialPower { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectFilter ObjectFilter { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string TriggerSound { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool MustFinishAbility { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool LoseStealthOnTrigger { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int PreTriggerUnstealthTime { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool GenerateUncontrollableFear { get; private set; }
    }
}
