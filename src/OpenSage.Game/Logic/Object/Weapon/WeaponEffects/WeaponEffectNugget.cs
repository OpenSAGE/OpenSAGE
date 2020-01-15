using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public abstract class WeaponEffectNugget
    {
        internal abstract void Activate(TimeSpan currentTime);

        internal abstract void Update(TimeSpan currentTime);
    }

    public abstract class WeaponEffectNuggetData
    {
        private protected static readonly IniParseTable<WeaponEffectNuggetData> FieldParseTable = new IniParseTable<WeaponEffectNuggetData>
        {
            { "SpecialObjectFilter", (parser, x) => x.SpecialObjectFilter = ObjectFilter.Parse(parser) },
            { "ForbiddenUpgradeNames", (parser, x) => x.ForbiddenUpgradeNames = parser.ParseAssetReferenceArray() },
            { "RequiredUpgradeNames", (parser, x) => x.RequiredUpgradeNames = parser.ParseAssetReferenceArray() },
        };

        public ObjectFilter SpecialObjectFilter { get; private set; }
        public string[] ForbiddenUpgradeNames { get; private set; }
        public string[] RequiredUpgradeNames { get; private set; }

        // TODO: Make this abstract.
        internal virtual WeaponEffectNugget CreateNugget(Weapon weapon) => null;
    }
}
