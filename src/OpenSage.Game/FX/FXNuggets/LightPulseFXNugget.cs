using System;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.FX
{
    public sealed class LightPulseFXNugget : FXNugget
    {
        internal static LightPulseFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LightPulseFXNugget> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<LightPulseFXNugget>
        {
            { "Color", (parser, x) => x.Color = parser.ParseColorRgb() },
            { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
            { "RadiusAsPercentOfObjectSize", (parser, x) => x.RadiusAsPercentOfObjectSize = parser.ParsePercentage() },
            { "IncreaseTime", (parser, x) => x.IncreaseTime = parser.ParseTimeMilliseconds() },
            { "DecreaseTime", (parser, x) => x.DecreaseTime = parser.ParseTimeMilliseconds() }
        });

        public ColorRgb Color { get; private set; }
        public int? Radius { get; private set; }
        public Percentage? RadiusAsPercentOfObjectSize { get; private set; }
        public TimeSpan IncreaseTime { get; private set; }
        public TimeSpan DecreaseTime { get; private set; }

        internal override void Execute(FXListExecutionContext context)
        {
            // TODO: Add dynamic point light with specified colour and radius to scene,
            // set to expire after IncreaseTime + DecreaseTime.
        }
    }
}
