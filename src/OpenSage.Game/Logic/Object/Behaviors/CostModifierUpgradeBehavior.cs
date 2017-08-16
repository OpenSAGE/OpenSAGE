using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CostModifierUpgradeBehavior : ObjectBehavior
    {
        internal static CostModifierUpgradeBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CostModifierUpgradeBehavior> FieldParseTable = new IniParseTable<CostModifierUpgradeBehavior>
        {
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReference() },
            { "EffectKindOf", (parser, x) => x.EffectKindOf = parser.ParseEnum<ObjectKinds>() },
            { "Percentage", (parser, x) => x.Percentage = parser.ParsePercentage() }
        };

        public string TriggeredBy { get; private set; }
        public ObjectKinds EffectKindOf { get; private set; }
        public float Percentage { get; private set; }
    }
}
