using System.Numerics;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    public sealed class WaterTransparency
    {
        internal static WaterTransparency Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<WaterTransparency> FieldParseTable = new IniParseTable<WaterTransparency>
        {
            { "TransparentWaterMinOpacity", (parser, x) => x.TransparentWaterMinOpacity = parser.ParseFloat() },
            { "TransparentWaterDepth", (parser, x) => x.TransparentWaterDepth = parser.ParseFloat() },

            { "SkyboxTextureN", (parser, x) => x.SkyboxTextureN = parser.ParseFileName() },
            { "SkyboxTextureE", (parser, x) => x.SkyboxTextureE = parser.ParseFileName() },
            { "SkyboxTextureS", (parser, x) => x.SkyboxTextureS = parser.ParseFileName() },
            { "SkyboxTextureW", (parser, x) => x.SkyboxTextureW = parser.ParseFileName() },
            { "SkyboxTextureT", (parser, x) => x.SkyboxTextureT = parser.ParseFileName() },

            { "StandingWaterColor", (parser, x) => x.StandingWaterColor = parser.ParseColorRgb() },
            { "StandingWaterTexture", (parser, x) => x.StandingWaterTexture = parser.ParseFileName() },
            { "AdditiveBlending", (parser, x) => x.AdditiveBlending = parser.ParseBoolean() },
            { "RadarWaterColor", (parser, x) => x.RadarWaterColor = parser.ParseColorRgb() },
            { "RiverTransparencyMultiplier", (parser, x) => x.RiverTransparencyMultiplier = parser.ParseFloat() },
            { "ReflectionPlaneZ", (parser, x) => x.ReflectionPlaneZ = parser.ParseFloat() },
            { "ReflectionOn", (parser, x) => x.ReflectionOn = parser.ParseBoolean() },
            { "ReflectionGuard", (parser, x) => x.ReflectionGuard = parser.ParseVector2() }
        };

        public float TransparentWaterMinOpacity { get; private set; }
        public float TransparentWaterDepth { get; private set; }

        public string SkyboxTextureN { get; private set; }
        public string SkyboxTextureE { get; private set; }
        public string SkyboxTextureS { get; private set; }
        public string SkyboxTextureW { get; private set; }
        public string SkyboxTextureT { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public ColorRgb StandingWaterColor { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string StandingWaterTexture { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool AdditiveBlending { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public ColorRgb RadarWaterColor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float RiverTransparencyMultiplier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float ReflectionPlaneZ { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ReflectionOn { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public Vector2 ReflectionGuard { get; private set; }
    }
}
