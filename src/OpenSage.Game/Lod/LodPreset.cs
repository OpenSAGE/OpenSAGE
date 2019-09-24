using OpenSage.Data.Ini;

namespace OpenSage.Lod
{
    public sealed class LodPreset : BaseAsset
    {
        internal static LodPreset Parse(IniParser parser)
        {
            var result = new LodPreset
            {
                Level = parser.ParseEnum<LodType>(),
                CpuType = parser.ParseEnum<CpuType>(),
                MHz = parser.ParseInteger(),
                GpuType = parser.ParseEnum<GpuType>(),
                GpuMemory = parser.ParseInteger()
            };

            result.SetNameAndInstanceId("LODPreset", result.Level.ToString());

            if (parser.SageGame >= SageGame.Bfme)
            {
                result.Unknown = parser.ParseInteger();
                result.ResolutionWidth = parser.ParseInteger();
                result.ResolutionHeight = parser.ParseInteger();
            }
            return result;
        }

        public LodType Level { get; private set; }
        public CpuType CpuType { get; private set; }
        public int MHz { get; private set; }
        public GpuType GpuType { get; private set; }
        public int GpuMemory { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int Unknown {get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ResolutionWidth { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int ResolutionHeight { get; private set; }
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
        R300,

        [IniEnum("RADEON_9800"), AddedIn(SageGame.Bfme2)]
        Radeon9800,

        [IniEnum("_MINIMUM_FOR_LOW_LOD"), AddedIn(SageGame.Bfme2)]
        MinimumForLowLod,

        [IniEnum("_MINIMUM_FOR_MEDIUM_LOD"), AddedIn(SageGame.Bfme2)]
        MinimumForMediumLod,

        [IniEnum("_MINIMUM_FOR_HIGH_LOD"), AddedIn(SageGame.Bfme2)]
        MinimumForHighLod,

        [IniEnum("_MINIMUM_FOR_ULTRA_HIGH_LOD"), AddedIn(SageGame.Bfme2)]
        MinimumForUltraHighLod,
    }
}
