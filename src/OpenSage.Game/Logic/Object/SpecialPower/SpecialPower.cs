using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public class SpecialPowerModuleData : BehaviorModuleData
    {
        internal static SpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<SpecialPowerModuleData> FieldParseTable = new IniParseTable<SpecialPowerModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "StartsPaused", (parser, x) => x.StartsPaused = parser.ParseBoolean() },
            { "UpdateModuleStartsAttack", (parser, x) => x.UpdateModuleStartsAttack = parser.ParseBoolean() },
            { "InitiateSound", (parser, x) => x.InitiateSound = parser.ParseAssetReference() },
            { "InitiateSound2", (parser, x) => x.InitiateSound2 = parser.ParseAssetReference() },
            { "AttributeModifier", (parser, x) => x.AttributeModifier = parser.ParseAssetReference() },
            { "AttributeModifierAffectsSelf", (parser, x) => x.AttributeModifierAffectsSelf = parser.ParseBoolean() },
            { "InitiateFX", (parser, x) => x.InitiateFX = parser.ParseAssetReference() },
            { "AntiCategory", (parser, x) => x.AntiCategory = parser.ParseEnum<ModifierCategory>() },
            { "AttributeModifierRange", (parser, x) => x.AttributeModifierRange = parser.ParseFloat() },
            { "AttributeModifierFX", (parser, x) => x.AttributeModifierFX = parser.ParseAssetReference() },
            { "TriggerFX", (parser, x) => x.TriggerFX = parser.ParseAssetReference() },
            { "SetModelCondition", (parser, x) => x.SetModelCondition = parser.ParseAttributeEnum<ModelConditionFlag>("ModelConditionState") },
            { "SetModelConditionTime", (parser, x) => x.SetModelConditionTime = parser.ParseFloat() },
            { "AttributeModifierAffects", (parser, x) => x.AttributeModifierAffects = ObjectFilter.Parse(parser) },
            { "AvailableAtStart", (parser, x) => x.AvailableAtStart = parser.ParseBoolean() },
            { "TargetAllSides", (parser, x) => x.TargetAllSides = parser.ParseBoolean() },
            { "AffectAllies", (parser, x) => x.AffectAllies = parser.ParseBoolean() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public bool StartsPaused { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool UpdateModuleStartsAttack { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string InitiateSound { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string AttributeModifier { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AttributeModifierAffectsSelf { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string InitiateFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ModifierCategory AntiCategory { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float AttributeModifierRange { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string AttributeModifierFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string InitiateSound2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string TriggerFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ModelConditionFlag SetModelCondition { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float SetModelConditionTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectFilter AttributeModifierAffects { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool TargetAllSides { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AvailableAtStart { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AffectAllies { get; private set; }
    }
}
