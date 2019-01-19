using System;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class StaticGameLod
    {
        internal static StaticGameLod Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Level = (LodType) Enum.Parse(typeof(LodType), name),
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
            { "UseAnisotropic", (parser, x) => x.UseAnisotropic = parser.ParseBoolean() },
            { "UsePixelShaders", (parser, x) => x.UsePixelShaders = parser.ParseBoolean() },
            { "UseShadowDecals", (parser, x) => x.UseShadowDecals = parser.ParseBoolean() },
            { "UseCloudMap", (parser, x) => x.UseCloudMap = parser.ParseBoolean() },
            { "UseLightMap", (parser, x) => x.UseLightMap = parser.ParseBoolean() },
            { "ShowSoftWaterEdge", (parser, x) => x.ShowSoftWaterEdge = parser.ParseBoolean() },
            { "MaxTankTrackEdges", (parser, x) => x.MaxTankTrackEdges = parser.ParseInteger() },
            { "MaxTankTrackOpaqueEdges", (parser, x) => x.MaxTankTrackOpaqueEdges = parser.ParseInteger() },
            { "MaxTankTrackFadeDelay", (parser, x) => x.MaxTankTrackFadeDelay = parser.ParseInteger() },
            { "UseBuildupScaffolds", (parser, x) => x.UseBuildupScaffolds = parser.ParseBoolean() },
            { "UseTreeSway", (parser, x) => x.UseTreeSway = parser.ParseBoolean() },
            { "ShowProps", (parser, x) => x.ShowProps = parser.ParseBoolean() },
            { "UseHighQualityVideo", (parser, x) => x.UseHighQualityVideo = parser.ParseBoolean() },
            { "UseEmissiveNightMaterials", (parser, x) => x.UseEmissiveNightMaterials = parser.ParseBoolean() },
            { "TextureReductionFactor", (parser, x) => x.TextureReductionFactor = parser.ParseInteger() },
            { "AnimationDetail", (parser, x) => x.AnimationDetail = parser.ParseEnum<LodType>() },
            { "MinParticlePriority", (parser, x) => x.MinParticlePriority = parser.ParseEnum<ParticleSystemPriority>() },
            { "MinParticleSkipPriority", (parser, x) => x.MinParticleSkipPriority = parser.ParseEnum<ParticleSystemPriority>() },
            { "ModelLOD", (parser, x) => x.ModelLOD = parser.ParseEnum<LodType>() },
            { "EffectsLOD", (parser, x) => x.EffectsLOD = parser.ParseEnum<LodType>() },
            { "UseShadowMapping", (parser, x) => x.UseShadowMapping = parser.ParseBoolean() },
            { "ShadowLOD", (parser, x) => x.ShadowLOD = parser.ParseEnum<LodType>() },
            { "UseTerrainNormalMap", (parser, x) => x.UseTerrainNormalMap = parser.ParseBoolean() },
            { "UseDistanceDependantTerrainTextures", (parser, x) => x.UseDistanceDependantTerrainTextures = parser.ParseBoolean() },
            { "WaterLOD", (parser, x) => x.WaterLOD = parser.ParseEnum<LodType>() },
            { "ShaderLOD", (parser, x) => x.ShaderLOD = parser.ParseEnum<LodType>() },
            { "ShaderMaterialReplacement", (parser, x) => x.ShaderMaterialReplacement = parser.ParseBoolean() },
            { "UseHeatEffects", (parser, x) => x.UseHeatEffects = parser.ParseBoolean() },
            { "DecalLOD", (parser, x) => x.DecalLOD = parser.ParseEnum<LodType>() },
        };

        public LodType Level { get; private set; }

        public int MinimumFps { get; private set; }
        public int MinimumProcessorFps { get; private set; }
        public int SampleCount2D { get; private set; }
        public int SampleCount3D { get; private set; }
        public int StreamCount { get; private set; }
        public int MaxParticleCount { get; private set; }
        public bool UseShadowVolumes { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool UseAnisotropic { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool UsePixelShaders { get; private set; }

        public bool UseShadowDecals { get; private set; }
        public bool UseCloudMap { get; private set; }
        public bool UseLightMap { get; private set; }
        public bool ShowSoftWaterEdge { get; private set; }
        public int MaxTankTrackEdges { get; private set; }
        public int MaxTankTrackOpaqueEdges { get; private set; }
        public int MaxTankTrackFadeDelay { get; private set; }
        public bool UseBuildupScaffolds { get; private set; }
        public bool UseTreeSway { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ShowProps { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool UseHighQualityVideo { get; private set; }

        public bool UseEmissiveNightMaterials { get; private set; }
        public int TextureReductionFactor { get; private set; }

        public LodType AnimationDetail { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ParticleSystemPriority MinParticlePriority { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ParticleSystemPriority MinParticleSkipPriority { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public LodType ModelLOD { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public LodType EffectsLOD { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool UseShadowMapping { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public LodType ShadowLOD { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool UseTerrainNormalMap { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool UseDistanceDependantTerrainTextures { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public LodType WaterLOD { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public LodType ShaderLOD { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ShaderMaterialReplacement { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool UseHeatEffects { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public LodType DecalLOD { get; private set; }
    }

    public enum LodType
    {
        [IniEnum("VeryLow"), AddedIn(SageGame.Bfme)]
        VeryLow,

        [IniEnum("LOW")]
        Low,

        [IniEnum("MEDIUM")]
        Medium,

        [IniEnum("HIGH")]
        High,

        [IniEnum("VeryHigh")]
        VeryHigh,

        [IniEnum("UltraHigh"), AddedIn(SageGame.Bfme)]
        UltraHigh,

        [IniEnum("Off"), AddedIn(SageGame.Bfme2)]
        Off,
    }
}
