using OpenSage.Data.Ini.Parser;

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
        };

        public float TransparentWaterMinOpacity { get; private set; }
        public float TransparentWaterDepth { get; private set; }

        public string SkyboxTextureN { get; private set; }
        public string SkyboxTextureE { get; private set; }
        public string SkyboxTextureS { get; private set; }
        public string SkyboxTextureW { get; private set; }
        public string SkyboxTextureT { get; private set; }
    }
}
