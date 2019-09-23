using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class LaserUpdateModuleData : ClientUpdateModuleData
    {
        internal static LaserUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LaserUpdateModuleData> FieldParseTable = new IniParseTable<LaserUpdateModuleData>
        {
            { "MuzzleParticleSystem", (parser, x) => x.MuzzleParticleSystem = parser.ParseAssetReference() },
            { "ParentFireBoneName", (parser, x) => x.ParentFireBoneName = parser.ParseBoneName() },
            { "ParentFireBoneOnTurret", (parser, x) => x.ParentFireBoneOnTurret = parser.ParseBoolean() },
            { "TargetParticleSystem", (parser, x) => x.TargetParticleSystem = parser.ParseAssetReference() },
            { "PunchThroughScalar", (parser, x) => x.PunchThroughScalar = parser.ParseFloat() },
            { "LaserLifetime", (parser, x) => x.LaserLifetime = parser.ParseInteger() }
        };

        public string MuzzleParticleSystem { get; private set; }
        public string ParentFireBoneName { get; private set; }
        public bool ParentFireBoneOnTurret { get; private set; }
        public string TargetParticleSystem { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float PunchThroughScalar { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int LaserLifetime { get; private set; }
    }
}
