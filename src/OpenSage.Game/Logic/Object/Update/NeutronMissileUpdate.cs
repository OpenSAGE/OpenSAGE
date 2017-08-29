using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class NeutronMissileUpdateModuleData : UpdateModuleData
    {
        internal static NeutronMissileUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<NeutronMissileUpdateModuleData> FieldParseTable = new IniParseTable<NeutronMissileUpdateModuleData>
        {
            { "DistanceToTravelBeforeTurning", (parser, x) => x.DistanceToTravelBeforeTurning = parser.ParseInteger() },
            { "MaxTurnRate", (parser, x) => x.MaxTurnRate = parser.ParseInteger() },
            { "ForwardDamping", (parser, x) => x.ForwardDamping = parser.ParseFloat() },
            { "RelativeSpeed", (parser, x) => x.RelativeSpeed = parser.ParseFloat() },
            { "LaunchFX", (parser, x) => x.LaunchFX = parser.ParseAssetReference() },
            { "IgnitionFX", (parser, x) => x.IgnitionFX = parser.ParseAssetReference() },
            { "TargetFromDirectlyAbove", (parser, x) => x.TargetFromDirectlyAbove = parser.ParseInteger() },
            { "SpecialAccelFactor", (parser, x) => x.SpecialAccelFactor = parser.ParseInteger() },
            { "SpecialSpeedTime", (parser, x) => x.SpecialSpeedTime = parser.ParseInteger() },
            { "SpecialSpeedHeight", (parser, x) => x.SpecialSpeedHeight = parser.ParseInteger() },
            { "SpecialJitterDistance", (parser, x) => x.SpecialJitterDistance = parser.ParseFloat() },
            { "DeliveryDecalRadius", (parser, x) => x.DeliveryDecalRadius = parser.ParseInteger() },
            { "DeliveryDecal", (parser, x) => x.DeliveryDecal = RadiusDecalTemplate.Parse(parser) },
        };

        public int DistanceToTravelBeforeTurning { get; private set; }
        public int MaxTurnRate { get; private set; }
        public float ForwardDamping { get; private set; }
        public float RelativeSpeed { get; private set; }
        public string LaunchFX { get; private set; }
        public string IgnitionFX { get; private set; }
        public int TargetFromDirectlyAbove { get; private set; }
        public int SpecialAccelFactor { get; private set; }
        public int SpecialSpeedTime { get; private set; }
        public int SpecialSpeedHeight { get; private set; }
        public float SpecialJitterDistance { get; private set; }
        public int DeliveryDecalRadius { get; private set; }
        public RadiusDecalTemplate DeliveryDecal { get; private set; }
    }
}
