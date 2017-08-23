using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class LaserUpdate : ClientUpdate
    {
        internal static LaserUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LaserUpdate> FieldParseTable = new IniParseTable<LaserUpdate>
        {
            { "MuzzleParticleSystem", (parser, x) => x.MuzzleParticleSystem = parser.ParseAssetReference() },
            { "ParentFireBoneName", (parser, x) => x.ParentFireBoneName = parser.ParseBoneName() },
            { "ParentFireBoneOnTurret", (parser, x) => x.ParentFireBoneOnTurret = parser.ParseBoolean() },
            { "TargetParticleSystem", (parser, x) => x.TargetParticleSystem = parser.ParseAssetReference() },
        };

        public string MuzzleParticleSystem { get; private set; }
        public string ParentFireBoneName { get; private set; }
        public bool ParentFireBoneOnTurret { get; private set; }
        public string TargetParticleSystem { get; private set; }
    }
}
