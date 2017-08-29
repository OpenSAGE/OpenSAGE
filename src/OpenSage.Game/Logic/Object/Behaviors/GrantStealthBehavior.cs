using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class GrantStealthBehaviorModuleData : BehaviorModuleData
    {
        internal static GrantStealthBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<GrantStealthBehaviorModuleData> FieldParseTable = new IniParseTable<GrantStealthBehaviorModuleData>
        {
            { "StartRadius", (parser, x) => x.StartRadius = parser.ParseFloat() },
            { "FinalRadius", (parser, x) => x.FinalRadius = parser.ParseFloat() },
            { "RadiusGrowRate", (parser, x) => x.RadiusGrowRate = parser.ParseFloat() },
            { "RadiusParticleSystemName", (parser, x) => x.RadiusParticleSystemName = parser.ParseAssetReference() },
            { "KindOf", (parser, x) => x.KindOf = parser.ParseEnumBitArray<ObjectKinds>() },
        };

        public float StartRadius { get; private set; }
        public float FinalRadius { get; private set; }
        public float RadiusGrowRate { get; private set; }
        public string RadiusParticleSystemName { get; private set; }
        public BitArray<ObjectKinds> KindOf { get; private set; }
    }
}
