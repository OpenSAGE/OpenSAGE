using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class BunkerBusterBehaviorModuleData : BehaviorModuleData
    {
        internal static BunkerBusterBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BunkerBusterBehaviorModuleData> FieldParseTable = new IniParseTable<BunkerBusterBehaviorModuleData>
        {
            { "UpgradeRequired", (parser, x) => x.UpgradeRequired = parser.ParseAssetReference() },
            { "DetonationFX", (parser, x) => x.DetonationFX = parser.ParseAssetReference() },
            { "CrashThroughBunkerFX", (parser, x) => x.CrashThroughBunkerFX = parser.ParseAssetReference() },
            { "CrashThroughBunkerFXFrequency", (parser, x) => x.CrashThroughBunkerFXFrequency = parser.ParseInteger() },

            { "SeismicEffectRadius", (parser, x) => x.SeismicEffectRadius = parser.ParseInteger() },
            { "SeismicEffectMagnitude", (parser, x) => x.SeismicEffectMagnitude = parser.ParseInteger() },

            { "ShockwaveWeaponTemplate", (parser, x) => x.ShockwaveWeaponTemplate = parser.ParseAssetReference() },
            { "OccupantDamageWeaponTemplate", (parser, x) => x.OccupantDamageWeaponTemplate = parser.ParseAssetReference() },
        };

        public string UpgradeRequired { get; private set; }
        public string DetonationFX { get; private set; }
        public string CrashThroughBunkerFX { get; private set; }
        public int CrashThroughBunkerFXFrequency { get; private set; }

        public int SeismicEffectRadius { get; private set; }
        public int SeismicEffectMagnitude { get; private set; }

        public string ShockwaveWeaponTemplate { get; private set; }
        public string OccupantDamageWeaponTemplate { get; private set; }
    }
}
