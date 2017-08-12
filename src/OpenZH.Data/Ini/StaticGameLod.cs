using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class StaticGameLod
    {
        internal static StaticGameLod Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<StaticGameLod> FieldParseTable = new IniParseTable<StaticGameLod>
        {
            { "MinimumFPS", (parser, x) => x.MinimumFps = parser.ParseInteger() },
            { "MinimumProcessorFps", (parser, x) => x.MinimumProcessorFps = parser.ParseInteger() },
            { "SampleCount2D", (parser, x) => x.SampleCount2D = parser.ParseInteger() },
            { "SampleCount3D", (parser, x) => x.SampleCount3D = parser.ParseInteger() },
            { "StreamCount", (parser, x) => x.StreamCount = parser.ParseInteger() },
            { "MaxParticleCount", (parser, x) => x.MaxParticleCount = parser.ParseInteger() },
            { "UseShadowVolumes", (parser, x) => x.UseShadowVolumes = parser.ParseBoolean() },
            { "UseShadowDecals", (parser, x) => x.UseShadowDecals = parser.ParseBoolean() },
            { "UseCloudMap", (parser, x) => x.UseCloudMap = parser.ParseBoolean() },
            { "UseLightMap", (parser, x) => x.UseLightMap = parser.ParseBoolean() },
            { "ShowSoftWaterEdge", (parser, x) => x.ShowSoftWaterEdge = parser.ParseBoolean() },
            { "MaxTankTrackEdges", (parser, x) => x.MaxTankTrackEdges = parser.ParseInteger() },
            { "MaxTankTrackOpaqueEdges", (parser, x) => x.MaxTankTrackOpaqueEdges = parser.ParseInteger() },
            { "MaxTankTrackFadeDelay", (parser, x) => x.MaxTankTrackFadeDelay = parser.ParseInteger() },
            { "UseBuildupScaffolds", (parser, x) => x.UseBuildupScaffolds = parser.ParseBoolean() },
            { "UseTreeSway", (parser, x) => x.UseTreeSway = parser.ParseBoolean() },
            { "UseEmissiveNightMaterials", (parser, x) => x.UseEmissiveNightMaterials = parser.ParseBoolean() },
            { "TextureReductionFactor", (parser, x) => x.TextureReductionFactor = parser.ParseInteger() },
        };

        public string Name { get; private set; }

        public int MinimumFps { get; private set; }
        public int MinimumProcessorFps { get; private set; }
        public int SampleCount2D { get; private set; }
        public int SampleCount3D { get; private set; }
        public int StreamCount { get; private set; }
        public int MaxParticleCount { get; private set; }
        public bool UseShadowVolumes { get; private set; }
        public bool UseShadowDecals { get; private set; }
        public bool UseCloudMap { get; private set; }
        public bool UseLightMap { get; private set; }
        public bool ShowSoftWaterEdge { get; private set; }
        public int MaxTankTrackEdges { get; private set; }
        public int MaxTankTrackOpaqueEdges { get; private set; }
        public int MaxTankTrackFadeDelay { get; private set; }
        public bool UseBuildupScaffolds { get; private set; }
        public bool UseTreeSway { get; private set; }
        public bool UseEmissiveNightMaterials { get; private set; }
        public int TextureReductionFactor { get; private set; }
    }
}
