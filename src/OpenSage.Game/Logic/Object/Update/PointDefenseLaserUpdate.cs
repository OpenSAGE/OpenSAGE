using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class PointDefenseLaserUpdateModuleData : UpdateModuleData
    {
        internal static PointDefenseLaserUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PointDefenseLaserUpdateModuleData> FieldParseTable = new IniParseTable<PointDefenseLaserUpdateModuleData>
        {
            { "WeaponTemplate", (parser, x) => x.WeaponTemplate = parser.ParseAssetReference() },
            { "PrimaryTargetTypes", (parser, x) => x.PrimaryTargetTypes = parser.ParseEnumBitArray<ObjectKinds>() },
            { "SecondaryTargetTypes", (parser, x) => x.SecondaryTargetTypes = parser.ParseEnumBitArray<ObjectKinds>() },
            { "ScanRate", (parser, x) => x.ScanRate = parser.ParseInteger() },
            { "ScanRange", (parser, x) => x.ScanRange = parser.ParseFloat() },
            { "PredictTargetVelocityFactor", (parser, x) => x.PredictTargetVelocityFactor = parser.ParseFloat() },
        };

        public string WeaponTemplate { get; private set; }
        public BitArray<ObjectKinds> PrimaryTargetTypes { get; private set; }
        public BitArray<ObjectKinds> SecondaryTargetTypes { get; private set; }
        public int ScanRate { get; private set; }
        public float ScanRange { get; private set; }
        public float PredictTargetVelocityFactor { get; private set; }
    }
}
