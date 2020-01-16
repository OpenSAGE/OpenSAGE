using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.FX
{
    public sealed class LightPulseFXNuggetData : FXNuggetData
    {
        internal static LightPulseFXNuggetData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LightPulseFXNuggetData> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<LightPulseFXNuggetData>
        {
            { "Color", (parser, x) => x.Color = parser.ParseColorRgb() },
            { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
            { "RadiusAsPercentOfObjectSize", (parser, x) => x.RadiusAsPercentOfObjectSize = parser.ParsePercentage() },
            { "IncreaseTime", (parser, x) => x.IncreaseTime = parser.ParseInteger() },
            { "DecreaseTime", (parser, x) => x.DecreaseTime = parser.ParseInteger() }
        });

        public ColorRgb Color { get; private set; }
        public int Radius { get; private set; }
        public Percentage RadiusAsPercentOfObjectSize { get; private set; }
        public int IncreaseTime { get; private set; }
        public int DecreaseTime { get; private set; }
    }
}
