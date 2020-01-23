using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.FX
{
    public sealed class TracerFXNugget : FXNugget
    {
        internal static TracerFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TracerFXNugget> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<TracerFXNugget>
        {
            { "Color", (parser, x) => x.Color = parser.ParseColorRgb() },
            { "DecayAt", (parser, x) => x.DecayAt = parser.ParseFloat() },
            { "Length", (parser, x) => x.Length = parser.ParseFloat() },
            { "Probability", (parser, x) => x.Probability = parser.ParseFloat() },
            { "Speed", (parser, x) => x.Speed = parser.ParseInteger() },
            { "Width", (parser, x) => x.Width = parser.ParseFloat() },
        });

        public ColorRgb Color { get; private set; }
        public float DecayAt { get; private set; }
        public float Length { get; private set; }
        public float Probability { get; private set; }
        public int Speed { get; private set; }
        public float Width { get; private set; }
    }
}
