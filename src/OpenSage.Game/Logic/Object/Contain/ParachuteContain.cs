using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to utilize PARA_MAN, PARA_ATTACH and PARA_COG bones on contained object.
    /// </summary>
    public sealed class ParachuteContainModuleData : ContainModuleData
    {
        internal static ParachuteContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ParachuteContainModuleData> FieldParseTable = new IniParseTable<ParachuteContainModuleData>
        {
            { "PitchRateMax", (parser, x) => x.PitchRateMax = parser.ParseInteger() },
            { "RollRateMax", (parser, x) => x.RollRateMax = parser.ParseInteger() },
            { "LowAltitudeDamping", (parser, x) => x.LowAltitudeDamping = parser.ParseFloat() },
            { "ParachuteOpenDist", (parser, x) => x.ParachuteOpenDist = parser.ParseFloat() },
            { "AllowInsideKindOf", (parser, x) => x.AllowInsideKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "ParachuteOpenSound", (parser, x) => x.ParachuteOpenSound = parser.ParseAssetReference() }
        };

        public int PitchRateMax { get; private set; }
        public int RollRateMax { get; private set; }
        public float LowAltitudeDamping { get; private set; }
        public float ParachuteOpenDist { get; private set; }
        public BitArray<ObjectKinds> AllowInsideKindOf { get; private set; }
        public string ParachuteOpenSound { get; private set; }
    }
}
