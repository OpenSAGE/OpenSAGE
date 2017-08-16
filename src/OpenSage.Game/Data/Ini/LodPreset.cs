using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class LodPreset
    {
        internal static LodPreset Parse(IniParser parser)
        {
            parser.NextToken(IniTokenType.Identifier);
            parser.NextToken(IniTokenType.Equals);

            var result = new LodPreset
            {
                Level = parser.ParseEnum<StaticGameLodLevel>(),
                CpuType = parser.ParseEnum<CpuType>(),
                MHz = parser.ParseInteger(),
                GpuType = parser.ParseEnum<GpuType>(),
                GpuMemory = parser.ParseInteger()
            };

            parser.NextTokenIf(IniTokenType.EndOfLine);

            return result;
        }

        public StaticGameLodLevel Level { get; private set; }
        public CpuType CpuType { get; private set; }
        public int MHz { get; private set; }
        public GpuType GpuType { get; private set; }
        public int GpuMemory { get; private set; }
    }

    public enum GpuType
    {
        [IniEnum("V2")]
        V2,

        [IniEnum("V3")]
        V3,

        [IniEnum("V4")]
        V4,

        [IniEnum("V5")]
        V5,

        [IniEnum("TNT")]
        TNT,

        [IniEnum("TNT2")]
        TNT2,

        [IniEnum("GF2")]
        GF2,

        [IniEnum("R100")]
        R100,

        [IniEnum("PS11")]
        PS11,

        [IniEnum("GF3")]
        GF3,

        [IniEnum("GF4")]
        GF4,

        [IniEnum("PS14")]
        PS14,

        [IniEnum("R200")]
        R200,

        [IniEnum("PS20")]
        PS20,

        [IniEnum("R300")]
        R300
    }
}
