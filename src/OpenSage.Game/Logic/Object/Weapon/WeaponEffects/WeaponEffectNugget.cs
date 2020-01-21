using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public abstract class WeaponEffectNugget
    {
        private protected static readonly IniParseTable<WeaponEffectNugget> FieldParseTable = new IniParseTable<WeaponEffectNugget>
        {
            { "SpecialObjectFilter", (parser, x) => x.SpecialObjectFilter = ObjectFilter.Parse(parser) },
            { "ForbiddenUpgradeNames", (parser, x) => x.ForbiddenUpgradeNames = parser.ParseAssetReferenceArray() },
            { "RequiredUpgradeNames", (parser, x) => x.RequiredUpgradeNames = parser.ParseAssetReferenceArray() },
        };

        public ObjectFilter SpecialObjectFilter { get; private set; }
        public string[] ForbiddenUpgradeNames { get; private set; }
        public string[] RequiredUpgradeNames { get; private set; }

        internal virtual void Execute(WeaponEffectExecutionContext context) { } // TODO: This should be abstract.
    }
}
