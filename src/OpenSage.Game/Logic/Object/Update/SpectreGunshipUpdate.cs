using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class SpectreGunshipUpdateModuleData : UpdateModuleData
    {
        internal static SpectreGunshipUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpectreGunshipUpdateModuleData> FieldParseTable = new IniParseTable<SpectreGunshipUpdateModuleData>
        {
            { "GattlingStrafeFXParticleSystem", (parser, x) => x.GattlingStrafeFXParticleSystem = parser.ParseAssetReference() },
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "HowitzerWeaponTemplate", (parser, x) => x.HowitzerWeaponTemplate = parser.ParseAssetReference() },
            { "GattlingTemplateName", (parser, x) => x.GattlingTemplateName = parser.ParseAssetReference() },
            { "RandomOffsetForHowitzer", (parser, x) => x.RandomOffsetForHowitzer = parser.ParseInteger() },
            { "TargetingReticleRadius", (parser, x) => x.TargetingReticleRadius = parser.ParseInteger() },
            { "OrbitInsertionSlope", (parser, x) => x.OrbitInsertionSlope = parser.ParseFloat() },
            { "GunshipOrbitRadius", (parser, x) => x.GunshipOrbitRadius = parser.ParseInteger() },
            { "HowitzerFiringRate", (parser, x) => x.HowitzerFiringRate = parser.ParseInteger() },
            { "HowitzerFollowLag", (parser, x) => x.HowitzerFollowLag = parser.ParseInteger() },
            { "StrafingIncrement", (parser, x) => x.StrafingIncrement = parser.ParseInteger() },
            { "AttackAreaRadius", (parser, x) => x.AttackAreaRadius = parser.ParseInteger() },
            { "OrbitTime", (parser, x) => x.OrbitTime = parser.ParseInteger() },

            { "AttackAreaDecal", (parser, x) => x.AttackAreaDecal = RadiusDecalTemplate.Parse(parser) },
            { "TargetingReticleDecal", (parser, x) => x.TargetingReticleDecal = RadiusDecalTemplate.Parse(parser) },
        };

        public string GattlingStrafeFXParticleSystem { get; private set; }
        public string SpecialPowerTemplate { get; private set; }
        public string HowitzerWeaponTemplate { get; private set; }
        public string GattlingTemplateName { get; private set; }
        public int RandomOffsetForHowitzer { get; private set; }
        public int TargetingReticleRadius { get; private set; }
        public float OrbitInsertionSlope { get; private set; }
        public int GunshipOrbitRadius { get; private set; }
        public int HowitzerFiringRate { get; private set; }
        public int HowitzerFollowLag { get; private set; }
        public int StrafingIncrement { get; private set; }
        public int AttackAreaRadius { get; private set; }
        public int OrbitTime { get; private set; }

        public RadiusDecalTemplate AttackAreaDecal { get; private set; }
        public RadiusDecalTemplate TargetingReticleDecal { get; private set; }
    }
}
