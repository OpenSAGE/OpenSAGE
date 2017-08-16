using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class WaterTransparency
    {
        internal static WaterTransparency Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<WaterTransparency> FieldParseTable = new IniParseTable<WaterTransparency>
        {
            { "TransparentWaterMinOpacity", (parser, x) => x.TransparentWaterMinOpacity = parser.ParseFloat() },
            { "TransparentWaterDepth", (parser, x) => x.TransparentWaterDepth = parser.ParseFloat() },
        };

        public float TransparentWaterMinOpacity { get; private set; }
        public float TransparentWaterDepth { get; private set; }
    }
}
