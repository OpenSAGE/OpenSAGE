using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;
using System.Numerics;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class Environment
    {
        public CloudEffect CloudEffect { get; internal set; }
        public RingEffect FireEffect { get; internal set; }
        public GlowEffect GlowEffect { get; internal set; }
        public RingEffect RingEffect { get; internal set; }
        public ShadowMap ShadowMap { get; internal set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class GlowEffect
    {
        internal static GlowEffect Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<GlowEffect> FieldParseTable = new IniParseTable<GlowEffect>
        {
            { "GlowEnabled", (parser, x) => x.GlowEnabled = parser.ParseBoolean() },
            { "GlowDiameter", (parser, x) => x.GlowDiameter = parser.ParseInteger() },
            { "GlowIntensity", (parser, x) => x.GlowIntensity = parser.ParseFloat() },
            { "GlowTextureWidth", (parser, x) => x.GlowTextureWidth = parser.ParseInteger() },
            { "RadiusScale1", (parser, x) => x.RadiusScale1 = parser.ParseFloat() },
            { "Amplitude1", (parser, x) => x.Amplitude1 = parser.ParseFloat() },
            { "RadiusScale2", (parser, x) => x.RadiusScale2 = parser.ParseFloat() },
            { "Amplitude2", (parser, x) => x.Amplitude2 = parser.ParseFloat() },
            { "TerrainGlow", (parser, x) => x.TerrainGlow = parser.ParseBoolean() },
            { "MultipassGlowEnabled", (parser, x) => x.MultipassGlowEnabled = parser.ParseBoolean() }
        };

        public bool GlowEnabled { get; private set; }
        public int GlowDiameter { get; private set; }
        public float GlowIntensity { get; private set; }
        public int GlowTextureWidth { get; private set; }
        public float RadiusScale1 { get; private set; }
        public float Amplitude1 { get; private set; }
        public float RadiusScale2 { get; private set; }
        public float Amplitude2 { get; private set; }
        public bool TerrainGlow { get; private set; }
        public bool MultipassGlowEnabled { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class RingEffect
    {
        internal static RingEffect Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<RingEffect> FieldParseTable = new IniParseTable<RingEffect>
        {
            { "Scale", (parser, x) => x.Scale = parser.ParseFloat() },
            { "Blend", (parser, x) => x.Blend = parser.ParseFloat() },
            { "BaseColor", (parser, x) => x.BaseColor = parser.ParseColorRgb() },
            { "EffectColor", (parser, x) => x.EffectColor = parser.ParseColorRgb() },
            { "EffectSaturation", (parser, x) => x.EffectSaturation = parser.ParseFloat() },
            { "BaseSaturation", (parser, x) => x.BaseSaturation = parser.ParseFloat() },
            { "Velocity", (parser, x) => x.Velocity = parser.ParseFloat() },
            { "TextureCross", (parser, x) => x.TextureCross = parser.ParseInteger() },
            { "TextureRepeatCount", (parser, x) => x.TextureRepeatCount = parser.ParseInteger() },
            { "EffectBlurDiameter", (parser, x) => x.EffectBlurDiameter = parser.ParseInteger() },
            { "BaseBlurDiameter", (parser, x) => x.BaseBlurDiameter = parser.ParseInteger() }
        };

        public float Scale { get; private set; }
        public float Blend { get; private set; }
        public ColorRgb BaseColor { get; private set; }
        public ColorRgb EffectColor { get; private set; }
        public float EffectSaturation { get; private set; }
        public float BaseSaturation { get; private set; }
        public float Velocity { get; private set; }
        public int TextureCross { get; private set; }
        public int TextureRepeatCount { get; private set; }
        public int EffectBlurDiameter { get; private set; }
        public int BaseBlurDiameter { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
    public sealed class CloudEffect
    {
        internal static CloudEffect Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<CloudEffect> FieldParseTable = new IniParseTable<CloudEffect>
        {
            { "CloudTexture", (parser, x) => x.CloudTexture = parser.ParseFileName() },
            { "DarkCloudTexture", (parser, x) => x.DarkCloudTexture = parser.ParseFileName() },
            { "AlphaTexture", (parser, x) => x.AlphaTexture = parser.ParseFileName() },
            { "PropagateSpeed", (parser, x) => x.PropagateSpeed = parser.ParseFloat() },
            { "Angle", (parser, x) => x.Angle = parser.ParseInteger() },
            { "DarkeningFactor", (parser, x) => x.DarkeningFactor = parser.ParseColorRgb() },
            { "DarkeningRate", (parser, x) => x.DarkeningRate = parser.ParseInteger() },
            { "LighteningRate", (parser, x) => x.LighteningRate = parser.ParseInteger() },
            { "CloudScrollSpeed", (parser, x) => x.CloudScrollSpeed = parser.ParseFloat() },
            { "DissipateTexture", (parser, x) => x.DissipateTexture = parser.ParseFileName() },
            { "DissipateStartLevel", (parser, x) => x.DissipateStartLevel = parser.ParseFloat() },
            { "DissipateSpeed", (parser, x) => x.DissipateSpeed = parser.ParseFloat() },
            { "DissipateRateScale", (parser, x) => x.DissipateRateScale = parser.ParseFloat() },
            { "LightningShadows", (parser, x) => x.LightningShadows = parser.ParseBoolean() },
            { "JitterLightningLightIntensity", (parser, x) => x.JitterLightningLightIntensity = parser.ParseBoolean() },
            { "JitterLightningLightPosition", (parser, x) => x.JitterLightningLightPosition = parser.ParseBoolean() },
            { "LightningChance", (parser, x) => x.LightningChance = parser.ParseFloat() },
            { "LightningDuration", (parser, x) => x.LightningDuration = parser.ParseFloat() },
            { "LightningFrequency", (parser, x) => x.LightningFrequency = parser.ParseFloat() },
            { "LightningIntensity", (parser, x) => x.LightningIntensity = parser.ParseFloat() },
            { "LightningShadowColor", (parser, x) => x.LightningShadowColor = parser.ParseColorRgb() },
            { "LightningShadowIntensity", (parser, x) => x.LightningShadowIntensity = parser.ParseFloat() },
            { "LightningLightPosition1", (parser, x) => x.LightningLightPosition1 = parser.ParseVector2() },
            { "LightningLightPosition2", (parser, x) => x.LightningLightPosition2 = parser.ParseVector2() },
            { "LightningLightPosition3", (parser, x) => x.LightningLightPosition3 = parser.ParseVector2() },
            { "LightningFX", (parser, x) => x.LightningFX = parser.ParseAssetReference() },
        };

        public string CloudTexture { get; private set; }
        public string DarkCloudTexture { get; private set; }
        public string AlphaTexture { get; private set; }
        public float PropagateSpeed { get; private set; }
        public int Angle { get; private set; }
        public ColorRgb DarkeningFactor { get; private set; }
        public int DarkeningRate { get; private set; }
        public int LighteningRate { get; private set; }
        public float CloudScrollSpeed { get; private set; }
        public string DissipateTexture { get; private set; }
        public float DissipateStartLevel { get; private set; }
        public float DissipateSpeed { get; private set; }
        public float DissipateRateScale { get; private set; }
        public bool LightningShadows { get; private set; }
        public bool JitterLightningLightIntensity { get; private set; }
        public bool JitterLightningLightPosition { get; private set; }
        public float LightningChance { get; private set; }
        public float LightningDuration { get; private set; }
        public float LightningFrequency { get; private set; }
        public float LightningIntensity { get; private set; }
        public ColorRgb LightningShadowColor { get; private set; }
        public float LightningShadowIntensity { get; private set; }
        public Vector2 LightningLightPosition1 { get; private set; }
        public Vector2 LightningLightPosition2 { get; private set; }
        public Vector2 LightningLightPosition3 { get; private set; }
        public string LightningFX { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public sealed class ShadowMap
    {
        internal static ShadowMap Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<ShadowMap> FieldParseTable = new IniParseTable<ShadowMap>
        {
            { "MapSize", (parser, x) => x.MapSize = parser.ParseInteger() },
            { "MaxViewDistance", (parser, x) => x.MaxViewDistance = parser.ParseFloat() },
            { "MinShadowedTerrainHeight", (parser, x) => x.MinShadowedTerrainHeight = parser.ParseFloat() },
        };

        public int MapSize { get; private set; }
        public float MaxViewDistance { get; private set; }
        public float MinShadowedTerrainHeight { get; private set; }
    }
}
