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
            { "AttributeModifier", (parser, x) => x.AttributeModifier = parser.ParseAssetReference() },
            { "AttributeModifierAffectsSelf", (parser, x) => x.AttributeModifierAffectsSelf = parser.ParseBoolean() },
            { "InitiateFX", (parser, x) => x.InitiateFX = parser.ParseAssetReference() },
            { "AntiCategory", (parser, x) => x.AntiCategory = parser.ParseEnum<ModifierCategory>() },
            { "AttributeModifierRange", (parser, x) => x.AttributeModifierRange = parser.ParseFloat() }
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
    }
}
